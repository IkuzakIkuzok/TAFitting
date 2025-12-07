
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TAFitting.Stats;

namespace TAFitting.Data;

/// <summary>
/// Represents a collection of decay data corresponding to the wavelengths.
/// </summary>
[DebuggerDisplay("Count = {Count}")]
#if DEBUG
[DebuggerTypeProxy(typeof(DecaysDebugView))]
#endif
internal sealed partial class Decays : IEnumerable<Decay>, IReadOnlyDictionary<double, Decay>
{
    private double time0 = 0.0;
    private readonly OrderedSortedDictionary<double, Decay> decays = [];

    /// <summary>
    /// Gets or sets the decay data at the specified wavelength.
    /// </summary>
    /// <param name="key">The wavelength.</param>
    /// <value>The decay data at the specified wavelength.</value>
    public Decay this[double key]
    {
        get => this.decays[key];
        set
        {
            if (value.TimeUnit != this.TimeUnit)
                throw new ArgumentException("Time unit mismatch.", nameof(value));
            if (value.SignalUnit != this.SignalUnit)
                throw new ArgumentException("Signal unit mismatch.", nameof(value));
            this.decays[key] = value;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<double> Keys => this.decays.Keys;

    /// <inheritdoc/>
    public IEnumerable<Decay> Values => this.decays.Values;

    /// <inheritdoc/>
    public int Count => this.decays.Count;

    /// <inheritdoc/>
    public bool ContainsKey(double key)
        => this.decays.ContainsKey(key);

    /// <inheritdoc/>
    public OrderedSortedDictionary<double, Decay>.Enumerator GetEnumerator()
        => this.decays.GetEnumerator();

    /// <inheritdoc/>
    public bool TryGetValue(double key, [MaybeNullWhen(false)] out Decay value)
        => this.decays.TryGetValue(key, out value);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<double, Decay>> IEnumerable<KeyValuePair<double, Decay>>.GetEnumerator()
        => this.decays.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => this.decays.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<Decay> IEnumerable<Decay>.GetEnumerator()
        => this.Values.GetEnumerator();

    /// <summary>
    /// Gets the time unit.
    /// </summary>
    /// <value>The unit of the time.</value>
    internal TimeUnit TimeUnit { get; }

    /// <summary>
    /// Gets the signal unit.
    /// </summary>
    /// <value>The unit of the signal.</value>
    internal SignalUnit SignalUnit { get; }

    /// <summary>
    /// Gets the maximum time.
    /// </summary>
    /// <value>The maximum time in the unit specified by <see cref="TimeUnit"/>.</value>
    internal double MaxTime
        => this.Values.Max(d => d.TimeMax);

    /// <summary>
    /// Gets the maximum value of the absolute signal.
    /// </summary>
    /// <value>The maximum value of the absolute signal.</value>
    internal double MaxAbsSignal
        => this.Values.Max(d => d.SignalFiniteAbsMax);

    /// <summary>
    /// Gets or sets the time zero.
    /// </summary>
    /// <value>The time zero in thie unit specified by <see cref="TimeUnit"/>.</value>
    internal double Time0
    {
        get => this.time0;
        set
        {
            if (value == this.time0) return;
            ChangeTime0(value);
        }
    }

    private Decays(TimeUnit timeUnit, SignalUnit signalUnit)
    {
        this.TimeUnit = timeUnit;
        this.SignalUnit = signalUnit;
    } // ctor (TimeUnit, SignalUnit)

    /// <summary>
    /// Tries to extract the wavelength from the basename.
    /// </summary>
    /// <param name="basename">The basename of the folder.</param>
    /// <param name="wavelength">The extracted wavelength.</param>
    /// <returns><see langword="true"/> if the wavelength is successfully extracted; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetWavelength(string basename, out double wavelength)
    {
        var len = 0;
        for (; len < basename.Length; len++)
        {
            var c = basename[len];
            if (c == '.')
                goto ScanDecimal;
            if (c is < '0' or > '9')
                goto ParseDouble;
        }
        goto ParseDouble;

    ScanDecimal:
        for (len += 1; len < basename.Length; len++)
        {
            var c = basename[len];
            if (c is < '0' or > '9')
                break;
        }

    ParseDouble:
        if (len == 0)
        {
            wavelength = 0.0;
            return false;
        }
        return double.TryParse(basename.AsSpan(0, len), out wavelength);
    } // internal static bool TryGetWavelength (string, out double)

    /// <summary>
    /// Loads the decay data from the folder.
    /// </summary>
    /// <param name="path">The path to the folder.</param>
    /// <returns>The decay data.</returns>
    /// <exception cref="IOException">No data found in the folder.</exception>
    internal static Decays MicrosecondFromFolder(string path)
    {
        var timeUnit = TimeUnit.Microsecond;
        var signalUnit = SignalUnit.MicroOD;

        var format_ab = Program.AMinusBSignalFormat;
        var format_b = Program.BSignalFormat;
        var simple_ab = FileNameHandler.IsSimpleFormat(format_ab);
        var simple_b = FileNameHandler.IsSimpleFormat(format_b);

        var decays = new Decays(timeUnit, signalUnit);

        var folders = Directory.EnumerateDirectories(path);
        var loader = new FileLoader();
        Parallel.ForEach(folders, (folder) =>
        {
            var basename = Path.GetFileName(folder);
            if (!TryGetWavelength(basename, out var wavelength)) return;
            loader.Register(folder, wavelength);
        });

        var dict = new ConcurrentDictionary<double, Decay>();
        var l_t0 = new double[loader.Count];
        var count = -1;  // start from -1 to use Interlocked.Increment as index
        var min_snr = Program.Config.DecayLoadingConfig.SignalToNoiseRatioThreshold;
        Parallel.ForEach(loader, (l) =>
        {
            var (wavelength, folder) = l;
            var basename = Path.GetFileName(folder);
            var name_ab = simple_ab
                ? FileNameHandler.GetFileNameFastMode(basename, format_ab)
                : FileNameHandler.GetFileName(basename, format_ab);
            var name_b = simple_b
                ? FileNameHandler.GetFileNameFastMode(basename, format_b)
                : FileNameHandler.GetFileName(basename, format_b);

            var file_ab = Path.Combine(folder, name_ab);
            var file_b = Path.Combine(folder, name_b);

            // If one of the files is missing, FileLoader.Register does not add the folder to the list
            // and the wavelength is not iterated here.
            // Therefore, checking the existence of the files is not necessary.

            // Read shorter data first, then longer data.
            // This increases the probability that the lookahead is completed.
            var decay_b = Decay.FromFile(file_b, timeUnit, signalUnit, loader.GetBFileData(wavelength), (2499 >> 1));  // Only the first half of the data is used
            var decay_ab = Decay.FromFile(file_ab, timeUnit, signalUnit, loader.GetAMinusBFileData(wavelength));
            dict.TryAdd(wavelength, decay_ab);

            var (i_t0, b_t0, s_t0) = decay_b.FilndT0();
            var baseline = decay_b.GetSignalsAsSpan(i_t0 >> 1);  // Time enough earlier than the pulse
            var noise = baseline.StandardDeviation();  // Noise level is estimated from the baseline
            var signal = Math.Abs(s_t0);
            var snr = signal / noise;
            if (snr >= min_snr)
                l_t0[Interlocked.Increment(ref count)] = b_t0;
            else
                Debug.WriteLine($"Signal-to-noise ratio is too low at {wavelength} nm: {snr} < {min_snr}");
        });

        if ((++count) == 0)  // count starts from -1, so increment first
            throw new IOException($"No data found in {path}");

        decays.decays.AddRange(dict);

        var t0 = l_t0.AsSpan(0, count).SmirnovGrubbs().Average();
        decays.Time0 = t0;

        return decays;
    } // internal static Decays MicrosecondFromFolder (string)

    /// <summary>
    /// Loads the decay data from the CSV file.
    /// </summary>
    /// <param name="path">The path to the CSV file.</param>
    /// <returns>The decay data.</returns>
    internal static Decays FemtosecondFromCsvFile(string path)
    {
        var timeUnit = TimeUnit.Picosecond;
        var signalUnit = SignalUnit.MilliOD;

        var lines = File.ReadAllBytes(path).GetText().Split('\n');

        var header = lines[0].Split(',')[1..];
        var times = header.Select(double.Parse).ToArray();

        var decays = new Decays(timeUnit, signalUnit);
        foreach (var line in lines[1..])
        {
            var parts = line.Split(',');
            if (!double.TryParse(parts[0], out var wl)) break;
            var signals = parts[1..].Select(ParseDouble).Select(s => s / signalUnit).ToArray();
            var decay = new Decay(times, timeUnit, signals, signalUnit, TasMode.Femtosecond);
            decay.RemoveNaN();
            decays.decays.Add(wl, decay);
        }

        return decays;
    } // internal static Decays FemtosecondFromCsvFile (string)

    /// <summary>
    /// Parses a double value from a string, handling special cases for NaN and infinity.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <returns>The parsed double value.</returns>
    /// <exception cref="FormatException"><paramref name="s"/> is not a valid double value.</exception>
    private static double ParseDouble(string s)
    {
        if (double.TryParse(s, out var d)) return d;
        if (s == "NaN") return double.NaN;
        if (s == "Inf") return double.PositiveInfinity;
        if (s == "-Inf") return double.NegativeInfinity;
        throw new FormatException($"Invalid double value: {s}");
    } // private static double ParseDouble (string)

    /// <summary>
    /// Loads the decay data from the UFS file.
    /// </summary>
    /// <param name="path">The path to the UFS file.</param>
    /// <returns>The decay data.</returns>
    /// <exception cref="IOException">The file is not a valid UFS file or the version is unsupported.</exception>
    internal static Decays FemtosecondFromUfsFile(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new UfsReader(stream, leaveOpen: true);

        var versionStr = reader.ReadString();
        const string versionPrefix = "Version";
        if (!versionStr.StartsWith(versionPrefix, StringComparison.InvariantCulture)) throw new IOException("Invalid version string.");
        if (!int.TryParse(versionStr.AsSpan(versionPrefix.Length), out var version)) throw new IOException("Invalid version number.");
        if (version > UfsIOHelper.Version) throw new IOException("Unsupported version.");

        reader.SkipString();  // wavelength name
        reader.SkipString();  // wavelength unit

        var wavelengthCount = reader.ReadInt32();
        var wavelengths = (stackalloc double[wavelengthCount]);
        reader.ReadDoubles(wavelengths);

        reader.SkipString();  // "Time"
        var tu = reader.ReadString();
        var timeUnit = tu switch
        {
            "fs" => TimeUnit.Femtosecond,
            "ps" => TimeUnit.Picosecond,
            "ns" => TimeUnit.Nanosecond,
            "us" => TimeUnit.Microsecond,
            "ms" => TimeUnit.Millisecond,
            "s"  => TimeUnit.Second,
            _ => throw new IOException($"Unsupported time unit: {tu}")
        };

        var timeCount = reader.ReadInt32();
        var times = reader.ReadDoubles(timeCount);

        var dataLabel = reader.ReadString();
        if (dataLabel != "DA") throw new IOException("Invalid data label.");

        var padding = reader.ReadInt32();
        if (padding != 0) throw new IOException("Invalid padding.");

        var wc = reader.ReadInt32();  // wavelength count
        if (wc != wavelengthCount) throw new IOException("Invalid wavelength count.");
        var tc = reader.ReadInt32();  // time count
        if (tc != timeCount) throw new IOException("Invalid time count.");

        var signalUnit = SignalUnit.MilliOD;
        var decays = new Decays(timeUnit, signalUnit);

        for (var i = 0; i < wavelengthCount; i++)
        {
            var wavelength = wavelengths[i];
            var signals = reader.ReadDoubles(timeCount).Select(s => s / signalUnit).ToArray();
            var decay = new Decay(times, timeUnit, signals, signalUnit, TasMode.Femtosecond);
            decay.RemoveNaN();
            decays.decays.Add(wavelength, decay);
        }

        return decays;
    } // internal static Decays FemtosecondFromUfsFile (string)

    private void ChangeTime0(double time0)
    {
        var diff = time0 - this.time0;
        this.time0 = time0;
        foreach (var decay in this.Values)
            decay.AddTime(-diff);
    } // private void ChangeTime0 (double)

    /// <summary>
    /// Removes the decay data.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <returns><see langword="true"/> if the decay data is successfully removed; otherwise, <see langword="false"/>.</returns>
    internal bool Remove(double wavelength)
        => this.decays.Remove(wavelength);

    /// <summary>
    /// Interpolates the decay data so that the time zero is the same.
    /// </summary>
    internal void Interpolate()
    {
        foreach (var decay in this.Values)
            decay.Interpolate();
    } // internal void Interpolate ()

#if DEBUG
    private sealed class DecaysDebugView(Decays decays)
    {
        private readonly Decays decays = decays;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public DecayDebugViewItem[] Items
        {
            get
            {
                var items = new DecayDebugViewItem[this.decays.Count];
                var i = 0;
                foreach ((var wavelength, var decay) in this.decays)
                    items[i++] = new DecayDebugViewItem(wavelength, decay);
                return items;
            }
        }

        [DebuggerDisplay("[{wavelength} nm] {decay}")]
        public sealed class DecayDebugViewItem(double wavelength, Decay decay)
        {
            private readonly double wavelength = wavelength;
            private readonly Decay decay = decay;
        }
    } // private sealed class DecaysDebugView (Decays)
#endif
} // internal sealed class Decays : IEnumerable<Decay>, IReadOnlyDictionary<double, Decay>

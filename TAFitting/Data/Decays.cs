
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading;
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
    private static readonly Regex re_wavelength = RegexWavelength();

    private double time0 = 0.0;
    private readonly Dictionary<double, Decay> decays = [];

    /// <summary>
    /// Gets the decay data at the specified wavelength.
    /// </summary>
    /// <param name="key">The wavelength.</param>
    /// <value>The decay data at the specified wavelength.</value>
    public Decay this[double key] => this.decays[key];

    public IEnumerable<double> Keys => this.decays.Keys;

    public IEnumerable<Decay> Values => this.decays.Values;

    public int Count => this.decays.Count;

    public bool ContainsKey(double key)
        => this.decays.ContainsKey(key);

    public IEnumerator<KeyValuePair<double, Decay>> GetEnumerator()
        => this.decays.GetEnumerator();

    public bool TryGetValue(double key, [MaybeNullWhen(false)] out Decay value)
        => this.decays.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => this.decays.GetEnumerator();

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
        => this.Values.Max(d => d.Absolute.SignalFiniteMax);

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

        var decays = new Decays(timeUnit, signalUnit);
        var l_t0 = new ConcurrentStack<double>();

        var folders = Directory.EnumerateDirectories(path);
        var loader = new FileLoader();
        foreach (var folder in folders)
        {
            var basename = Path.GetFileName(folder);
            var wl = re_wavelength.Match(basename).Groups[1].Value;
            if (!double.TryParse(wl, out var wavelength)) continue;
            loader.Register(folder, wavelength);
        }

        var dict = new ConcurrentDictionary<double, Decay>();
        Parallel.ForEach(loader, (l) =>
        {
            var (wavelength, folder) = l;
            var basename = Path.GetFileName(folder);
            var name_ab = FileNameHandler.GetFileName(basename, format_ab);
            var name_b = FileNameHandler.GetFileName(basename, format_b);

            var file_ab = Path.Combine(folder, name_ab);
            var file_b = Path.Combine(folder, name_b);

            // If one of the files is missing, FileLoader.Register does not add the folder to the list
            // and the wavelength is not iterated here.
            // Therefore, checking the existence of the files is not necessary.

            var decay_ab = Decay.FromFile(file_ab, timeUnit, signalUnit, loader.GetAMinusBFileData(wavelength));
            var decay_b = Decay.FromFile(file_b, timeUnit, signalUnit, loader.GetBFileData(wavelength), (2499 >> 1));  // Only the first half of the data is used
            dict.TryAdd(wavelength, decay_ab);

            var b_t0 = decay_b.FilndT0();
            var baseline = decay_b.OfRange(0, b_t0 * 0.5);  // Time enough earlier than the pulse
            var noise = baseline.Signals.StandardDeviation();  // Noise level is estimated from the baseline
            var signal = Math.Abs(decay_b[b_t0]);
            var snr = signal / noise;
            if (snr > 2) l_t0.Push(b_t0);  // S/N > 2
            else Debug.WriteLine($"Signal-to-noise ratio is too low at {wavelength} nm: {snr}");
        });
        foreach ((var wavelength, var decay) in dict.OrderBy(kv => kv.Key))
            decays.decays.Add(wavelength, decay);

        if (l_t0.IsEmpty) throw new IOException($"No data found in {path}");

        //var t0 = l_t0.SmirnovGrubbs().Average();
        var t0 = l_t0.ToArray().AsSpan().SmirnovGrubbs().Average();
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
            var decay = new Decay(times, timeUnit, signals, signalUnit);
            decay.RemoveNaN();
            decays.decays.Add(wl, decay);
        }

        return decays;
    } // internal static Decays FemtosecondFromCsvFile (string)

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
        if (!versionStr.StartsWith("Version", StringComparison.InvariantCulture)) throw new IOException("Invalid version string.");
        if (!int.TryParse(versionStr.AsSpan(7), out var version)) throw new IOException("Invalid version number.");
        if (version > UfsIOHelper.Version) throw new IOException("Unsupported version.");

        reader.SkipString();  // wavelength name
        reader.SkipString();  // wavelength unit

        var wavelengthCount = reader.ReadInt32();
        var wavelengths = reader.ReadDoubles(wavelengthCount);

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
            var decay = new Decay(times, timeUnit, signals, signalUnit);
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

    [GeneratedRegex(@"(\d+).*")]
    private static partial Regex RegexWavelength();


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

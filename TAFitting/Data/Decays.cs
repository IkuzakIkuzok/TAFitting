﻿
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
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
    /// <returns>The decay data at the specified wavelength.</returns>
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
    internal string TimeUnit { get; }

    /// <summary>
    /// Gets the signal unit.
    /// </summary>
    internal string SignalUnit { get; }

    /// <summary>
    /// Gets the maximum time.
    /// </summary>
    internal double MaxTime
        => this.Values.Max(d => d.TimeMax);

    /// <summary>
    /// Gets the maximum value of the absolute signal.
    /// </summary>
    internal double MaxAbsSignal
        => this.Values.Max(d => d.Absolute.SignalMax);

    /// <summary>
    /// Gets or sets the time zero.
    /// </summary>
    internal double Time0
    {
        get => this.time0;
        set
        {
            if (value == this.time0) return;
            ChangeTime0(value);
        }
    }

    private Decays(string timeUnit, string signalUnit)
    {
        this.TimeUnit = timeUnit;
        this.SignalUnit = signalUnit;
    } // ctor (string, string)

    /// <summary>
    /// Loads the decay data from the folder.
    /// </summary>
    /// <param name="path">The path to the folder.</param>
    /// <returns>The decay data.</returns>
    /// <exception cref="IOException">No data found in the folder.</exception>
    internal static Decays MicrosecondFromFolder(string path)
    {
        var format_ab = Program.AMinusBSignalFormat;
        var format_b = Program.BSignalFormat;

        var decays = new Decays("µs", "ΔµOD");
        var l_t0 = new List<double>();

        var folders = Directory.EnumerateDirectories(path);
        var wl_folders = new Dictionary<double, string>();
        foreach (var folder in folders)
        {
            var basename = Path.GetFileName(folder);
            var wl = re_wavelength.Match(basename).Groups[1].Value;
            if (!double.TryParse(wl, out var wavelength)) continue;
            wl_folders.Add(wavelength, folder);
        }

        foreach ((var wavelength, var folder) in wl_folders.OrderBy(kv => kv.Key))
        {
            var basename = Path.GetFileName(folder);
            var name_ab = FileNameHandler.GetFileName(basename, format_ab);
            var name_b = FileNameHandler.GetFileName(basename, format_b);

            var file_ab = Path.Combine(folder, name_ab);
            var file_b = Path.Combine(folder, name_b);

            if (!File.Exists(file_ab) || !File.Exists(file_b)) continue;

            var decay_ab = Decay.FromFile(file_ab, 1e6, 1e6);
            var decay_b = Decay.FromFile(file_b, 1e6, 1e6);
            decays.decays.Add(wavelength, decay_ab);

            l_t0.Add(decay_b.FilndT0());
        }

        if (l_t0.Count == 0) throw new IOException($"No data found in {path}");

        var t0 = l_t0.SmirnovGrubbs().Average();
        decays.Time0 = t0;

        return decays;
    } // internal static Decays MicrosecondFromFolder (string)

    /// <summary>
    /// Loads the decay data from the file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>The decay data.</returns>
    internal static Decays FemtosecondFromFile(string path)
    {
        var lines = File.ReadAllBytes(path).GetText().Split('\n');

        var header = lines[0].Split(',')[1..];
        var times = header.Select(double.Parse).ToArray();

        var decays = new Decays("ps", "ΔmOD");
        foreach (var line in lines[1..])
        {
            var parts = line.Split(',');
            if (!double.TryParse(parts[0], out var wl)) break;
            var signals = parts[1..].Select(double.Parse).Select(s => s * 1e3).ToArray();
            var decay = new Decay(times, signals);
            decay.RemoveNaN();
            decays.decays.Add(wl, decay);
        }

        return decays;
    } // internal static Decays FemtosecondFromFile (string)

    private void ChangeTime0(double time0)
    {
        var diff = time0 - this.time0;
        this.time0 = time0;
        foreach ((var wavelength, var decay) in this)
            this.decays[wavelength] = decay.AddTime(-diff);
    } // private void ChangeTime0 (double)

    /// <summary>
    /// Removes the decay data.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <returns><see langword="true"/> if the decay data is successfully removed; otherwise, <see langword="false"/>.</returns>
    internal bool Remove(double wavelength)
        => this.decays.Remove(wavelength);

    [GeneratedRegex(@"(\d+).*", RegexOptions.Compiled)]
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

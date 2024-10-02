
// (c) 2024 Kazuki KOHZUKI

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TAFitting.Stats;

namespace TAFitting.Data;

[DebuggerDisplay("Count = {Count}")]
internal sealed partial class Decays : IEnumerable<Decay>, IReadOnlyDictionary<double, Decay>
{
    private static readonly Regex re_wavelength = RegexWavelength();

    private double time0 = 0.0;
    private readonly Dictionary<double, Decay> decays = [];

    public Decay this[double key] => ((IReadOnlyDictionary<double, Decay>)this.decays)[key];

    public IEnumerable<double> Keys => ((IReadOnlyDictionary<double, Decay>)this.decays).Keys;

    public IEnumerable<Decay> Values => ((IReadOnlyDictionary<double, Decay>)this.decays).Values;

    public int Count => ((IReadOnlyCollection<KeyValuePair<double, Decay>>)this.decays).Count;

    public bool ContainsKey(double key)
        => ((IReadOnlyDictionary<double, Decay>)this.decays).ContainsKey(key);

    public IEnumerator<KeyValuePair<double, Decay>> GetEnumerator()
        => ((IEnumerable<KeyValuePair<double, Decay>>)this.decays).GetEnumerator();

    public bool TryGetValue(double key, [MaybeNullWhen(false)] out Decay value)
        => ((IReadOnlyDictionary<double, Decay>)this.decays).TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)this.decays).GetEnumerator();

    IEnumerator<Decay> IEnumerable<Decay>.GetEnumerator()
        => this.Values.GetEnumerator();

    internal string TimeUnit { get; }

    internal string SignalUnit { get; }

    internal double MaxTime
        => this.Values.Max(d => d.TimeMax);

    internal double MaxAbsSignal
        => this.Values.Max(d => d.Absolute.SignalMax);

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

    internal static Decays FemtosecondFromFile(string path)
    {
        var lines = File.ReadAllBytes(path).GetText().Split('\n');

        var header = lines[0].Split(',')[1..];
        var times = header.Select(double.Parse).ToArray();

        var decays = new Decays("fs", "ΔmOD");
        foreach (var line in lines[1..])
        {
            var parts = line.Split(',');
            if (!double.TryParse(parts[0], out var wl)) break;
            var signals = parts[1..].Select(double.Parse).Select(s => s * 1e3).ToArray();

            for (var i = 0; i < signals.Length; i++)
            {
                if (!double.IsNaN(signals[i])) continue;
                var left = i > 0 ? signals[i - 1] : 0.0;
                var right = i < signals.Length - 1 ? signals[i + 1] : 0.0;
                if (double.IsNaN(left) || double.IsNaN(right))
                    signals[i] = 0.0;
                else
                    signals[i] = (left + right) / 2.0;
            }

            var decay = new Decay(times, signals);
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

    internal bool Remove(double wavelength)
        => this.decays.Remove(wavelength);

    [GeneratedRegex(@"(\d+).*", RegexOptions.Compiled)]
    private static partial Regex RegexWavelength();
} // internal sealed class Decays : IEnumerable<Decay>, IReadOnlyDictionary<double, Decay>

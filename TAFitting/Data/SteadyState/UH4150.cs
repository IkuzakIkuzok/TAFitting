
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data.SteadyState;

/// <summary>
/// Represents a steady-state spectrum measured with a UH4150 spectrophotometer.
/// </summary>
internal sealed  partial class UH4150 : SteadyStateSpectrum
{
    override internal void LoadFile(string path)
    {
        using var reader = new StreamReader(path, TextUtils.CP932);

        string? line;
        var abs = false;
        while ((line = reader.ReadLine()) is not null)
        {
            if (!line.StartsWith("nm")) continue;
            var fields = line.Split('\t');
            if (fields.Length < 2) continue;
            abs = fields[1].Contains("Abs");
            break;
        }

        Func<double, double> a_map = abs ? FromAbsorbance : FromTransmittance;
        while ((line = reader.ReadLine()) is not null)
        {
            var fields = line.Split('\t');
            if (fields.Length < 2) continue;
            if (!double.TryParse(fields[0], out var wl)) continue;
            if (!double.TryParse(fields[1], out var i)) continue;
            var a = a_map(i);
            this._spectrum.Add((wl, a));
        }
    } // override internal void LoadFile (string)

    private static double FromTransmittance(double x)
        => -Math.Log10(x);

    private static double FromAbsorbance(double x)
        => x;
} // internal sealed partial class UH4150 : SteadyStateSpectrum

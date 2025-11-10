
// (c) 2025 Kazuki Kohzuki

using TAFitting.Controls;

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
            if (!line.StartsWith("nm", StringComparison.Ordinal)) continue;
            var fields = line.Split('\t');
            if (fields.Length < 2) continue;
            abs = fields[1].Contains("Abs", StringComparison.Ordinal);
            break;
        }

        Func<double, double> a_map = abs ? FromAbsorbance : FromTransmittance;
        var values = (stackalloc double[2]);
        while ((line = reader.ReadLine()) is not null)
        {
            if (NegativeSignHandler.ParseDoubles(line.AsSpan(), '\t', values) < 2)
                continue;

            var wl = values[0];
            var i = values[1];
            var a = a_map(i);
            this._spectrum.Add((wl, a));
        }
    } // override internal void LoadFile (string)

    private static double FromTransmittance(double x)
        => -Math.Log10(x);

    private static double FromAbsorbance(double x)
        => x;
} // internal sealed partial class UH4150 : SteadyStateSpectrum

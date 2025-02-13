
// (c) 2025 Kazuki Kohzuki

using System.Numerics;

namespace TAFitting.Filter.Fourier;

/// <summary>
/// Represents a filter that uses Fourier transform.
/// </summary>
internal abstract class FourierFilter : IFilter
{
    protected double cutoff = 1;

    /// <inheritdoc/>
    public string Name => $"{GetScaled(this.cutoff, "Hz")} ({GetScaled(1 / this.cutoff, "s")})";

    /// <inheritdoc/>
    public string Description
        => $"A filter that uses Fourier transform with a cutoff frequency of {GetScaled(this.cutoff, "Hz")} ({GetScaled(1 / this.cutoff, "s")}).";

    /// <inheritdoc/>
    public IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
    {
        if (time.Count != signal.Count)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Count < 2)
            throw new ArgumentException($"The number of points must be greater than or equal to 2.");

        var arr = signal.ToArray();

        var n = (int)Math.Pow(2, Math.Floor(Math.Log2(time.Count)));  // FFT works only for the number of points that is a power of 2.
        var sampleRate = n / (time[n] - time[0]);

        var result = new double[time.Count];
        Array.Copy(arr, 0, result, 0, time.Count);

        // Take data points after the first non-negative time point if the number of positive time points is greater than `n`;
        // otherwise, take the last `n` data points.
        int offset;
        for (offset = 0; offset < time.Count - n - 1; ++offset)
            if (time[offset] >= 0) break;

        var buffer = arr.Skip(offset).Take(n).Select(v => new Complex(v, 0)).ToArray();
        DiscreteFourierTransform.Forward(buffer);
        var freq = DiscreteFourierTransform.FrequencyScale(n, sampleRate, false);

        for (var i = 0; i < n; ++i)
        {
            var f = Math.Abs(freq[i]);
            if (f > this.cutoff)
                buffer[i] = 0;
        }

        var filtered = DiscreteFourierTransform.InverseReal(buffer);
        Array.Copy(filtered, 0, result, offset, n);

        return result;
    } // public IReadOnlyList<double> Filter (IReadOnlyList<double>, IReadOnlyList<double>)

    private static readonly string[] SI_PREFIXES = [
        "Q", "R", "Y", "Z", "E", "P", "T", "G", "M", "k", "", "m", "μ", "n", "p", "f", "a", "z", "y", "r", "q"
    ];

    private static string GetScaled(double value, string unit)
    {
        var o = (int)Math.Clamp(Math.Floor(Math.Log10(Math.Abs(value))) / 3, -10, 10);
        var prefix = SI_PREFIXES[10 - o];
        var order = Math.Pow(10, 3 * o);
        return (value / order).ToString("0.###") + " " + prefix + unit;
    } // private static string GetScaled (double, string)
} // internal abstract class FourierFilter : IFilter

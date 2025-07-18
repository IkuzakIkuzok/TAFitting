
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.Numerics;

namespace TAFitting.Filter.Fourier;

/// <summary>
/// Represents a filter that uses Fourier transform.
/// </summary>
internal abstract class FourierFilter : IFilter
{
    /// <summary>
    /// The cutoff frequency of the filter.
    /// </summary>
    protected double cutoff = 1;

    /// <inheritdoc/>
    public string Name => GetName();

    /// <inheritdoc/>
    public string Description => GetDescription();

    /// <summary>
    /// Gets the name of the current filter.
    /// </summary>
    /// <returns>The name of the current filter.</returns>
    protected virtual string GetName()
        => $"{GetScaled(this.cutoff, "Hz")} ({GetScaled(1 / this.cutoff, "s")})";

    /// <summary>
    /// Gets the description of the current filter.
    /// </summary>
    /// <returns>The description of the current filter.</returns>
    protected virtual string GetDescription()
        => $"A filter that uses Fourier transform with a cutoff frequency of {GetScaled(this.cutoff, "Hz")} ({GetScaled(1 / this.cutoff, "s")}).";

    /// <inheritdoc/>
    public virtual IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
    {
        if (time.Count != signal.Count)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Count < 2)
            throw new ArgumentException($"The number of points must be greater than or equal to 2.");

        Debug.Assert(FastFourierTransform.CheckEvenlySpaced(time), "The time points must be evenly spaced.");

        // FFT works only significantly fast when the number of points that is a power of 2.
        // Extend the number of points to the nearest power of 2.
        var n = 1 << (int)Math.Ceiling(Math.Log2(time.Count));
        var sampleRate = (time.Count - 1) / (time[^1] - time[0]);

        // Pad before and after the signal with zeros.
        // This reduces the discontinuity at the edges and improves the result especially at the edges.
        var offset = (n - time.Count) >> 1;
        // Max stackalloc size is 256 KiB.
        var buffer = n <= 0x4000 ? stackalloc Complex[n] : new Complex[n];
        for (var i = 0; i < time.Count; ++i)
            buffer[i + offset] = new(signal[i], 0);

        FastFourierTransform.ForwardSplitRadix(buffer);
        var freq = FastFourierTransform.FrequencyScale(n, sampleRate, false);

        for (var i = 0; i < n; ++i)
        {
            var f = Math.Abs(freq[i]);
            if (f > this.cutoff)
                buffer[i] = 0;
        }

        var filtered = FastFourierTransform.InverseReal(buffer);
        var result = new double[time.Count];
        Array.Copy(filtered, offset, result, 0, time.Count);

        return result;
    } // public virtual IReadOnlyList<double> Filter (IReadOnlyList<double>, IReadOnlyList<double>)

    private static readonly string[] SI_PREFIXES = [
        "Q", "R", "Y", "Z", "E", "P", "T", "G", "M", "k", "", "m", "μ", "n", "p", "f", "a", "z", "y", "r", "q"
    ];

    private static string GetScaled(double value, string unit)
    {
        var o = (int)Math.Clamp(Math.Floor(Math.Log10(Math.Abs(value))) / 3, -10, 10);
        var order = Math.Pow(10, 3 * o);
        value /= order;
        if (value < 0.1 && o > -10)
        {
            value *= 1000;
            o -= 1;
        }
        var prefix = SI_PREFIXES[10 - o];
        return value.ToInvariantString("0.###") + " " + prefix + unit;
    } // private static string GetScaled (double, string)
} // internal abstract class FourierFilter : IFilter

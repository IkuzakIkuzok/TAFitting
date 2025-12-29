
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.Numerics;
using TAFitting.Buffers;

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
    public virtual void Filter(ReadOnlySpan<double> time, ReadOnlySpan<double> signal, Span<double> output)
    {
        if (time.Length != signal.Length)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Length < 2)
            throw new ArgumentException($"The number of points must be greater than or equal to 2.");

        Debug.Assert(FastFourierTransform.CheckEvenlySpaced(time), "The time points must be evenly spaced.");

        var n = time.Length;
        var sampleRate = (time.Length - 1) / (time[^1] - time[0]);

        var a = sampleRate / n;
        var b = this.cutoff / a;
        var i_min = (int)Math.Ceiling(b);
        var i_max = (int)Math.Floor(n - b);

        var half = n >> 1;
        if (i_min >= half || i_max < half)
        {
            // No frequency points to filter. Just copy the input signal to output.
            signal.CopyTo(output);
            return;
        }

        // Max stackalloc size is 256 KiB.
        using var pooled = new PooledBuffer<Complex>(n);
        var buffer = n <= 0x4000 ? stackalloc Complex[n] : pooled.GetSpan();
        for (var i = 0; i < time.Length; ++i)
            buffer[i] = new(signal[i], 0);

        FastFourierTransform.Forward(buffer);
        buffer[i_min..(i_max + 1)].Clear();

        // Store the result back to output buffer.
        FastFourierTransform.InverseReal(buffer, output);
    } // public virtual void Filter (ReadOnlySpan<double>, ReadOnlySpan<double>, Span<double>)

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

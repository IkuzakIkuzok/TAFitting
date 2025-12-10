
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Filter.Fourier.FourierAuto;

/// <summary>
/// A filter that uses Fourier transform with a cutoff frequency determined based on the time bandwidth.
/// </summary>
internal abstract class FourierFilterAuto : FourierFilter
{
    protected double ratio = 0.1;

    /// <inheritdoc/>
    override protected string GetName()
        => $"{this.ratio * 100}%";

    /// <inheritdoc/>
    override protected string GetDescription()
        => $"A filter that uses Fourier transform with a cutoff frequency of {this.ratio * 100}% of time bandwidth.";

    /// <inheritdoc/>
    override public void Filter(ReadOnlySpan<double> time, ReadOnlySpan<double> signal, Span<double> output)
    {
        this.cutoff = 1 / ((time[^1] - time[0]) * this.ratio);
        base.Filter(time, signal, output);
    } // override public void Filter (ReadOnlySpan<double>, ReadOnlySpan<double>, Span<double>)
} // internal abstract class FourierFilterAuto : FourierFilter


// (c) 2025 Kazuki Kohzuki

using System.Diagnostics.CodeAnalysis;

namespace TAFitting.Filter;

/// <summary>
/// Represents a filter with convolution.
/// </summary>
internal abstract class ConvolutionFilter : IFilter
{
    protected string name, description;
    protected double coefficient0;
    protected double[] coefficients;
    protected readonly int n;

    /// <inheritdoc/>
    public string Name => this.name;

    /// <inheritdoc/>
    public string Description => this.description;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvolutionFilter"/> class.
    /// </summary>
    public ConvolutionFilter()
    {
        Initialize();
        this.n = this.coefficients.Length;
    } // ctor ()

    /// <summary>
    /// Initializes the filter.
    /// </summary>
    /// <remarks>
    /// This method must be overridden in a derived class,
    /// and it must set the <see cref="name"/>, <see cref="description"/>, <see cref="coefficient0"/>, and <see cref="coefficients"/> fields.
    /// </remarks>
    [MemberNotNull(nameof(name), nameof(description), nameof(coefficient0), nameof(coefficients))]
    abstract protected void Initialize();

    /// <inheritdoc/>
    public void Filter(ReadOnlySpan<double> time, ReadOnlySpan<double> signal, Span<double> output)
    {
        if (time.Length != signal.Length)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Length < this.n)
            throw new ArgumentException($"The number of points must be greater than or equal to {this.n}.");

        for (var i = 0; i < output.Length; ++i)
        {
            if (i < this.n || i >= time.Length - this.n)
            {
                output[i] = signal[i];
                continue;
            }

            output[i] = signal[i] * this.coefficient0;
            for (var j = 0; j < this.coefficients.Length; ++j)
            {
                output[i] += signal[i - this.n + j] * this.coefficients[j];
                output[i] += signal[i + this.n - j] * this.coefficients[j];
            }
        }
    } // public void Filter (ReadOnlySpan<double>, ReadOnlySpan<double>, Span<double>)
} // internal abstract class ConvolutionFilter : IFilter

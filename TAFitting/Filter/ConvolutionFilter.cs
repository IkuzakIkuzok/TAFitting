
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
    public IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
    {
        if (time.Count != signal.Count)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Count < this.n)
            throw new ArgumentException($"The number of points must be greater than or equal to {this.n}.");

        // Handling the signal as a span improves the performance of indexing.
        var span = CollectionsMarshal.AsSpan<double>(signal.ToList());
        var filtered = new double[time.Count];
        for (var i = 0; i < filtered.Length; ++i)
        {
            if (i < this.n || i >= time.Count - this.n)
            {
                filtered[i] = span[i];
                continue;
            }

            filtered[i] = span[i] * this.coefficient0;
            for (var j = 0; j < this.coefficients.Length; ++j)
            {
                filtered[i] += span[i - this.n + j] * this.coefficients[j];
                filtered[i] += span[i + this.n - j] * this.coefficients[j];
            }
        }

        return filtered;
    } // public IReadOnlyList<double> Filter (IReadOnlyList<double>, IReadOnlyList<double>)
} // internal abstract class ConvolutionFilter : IFilter

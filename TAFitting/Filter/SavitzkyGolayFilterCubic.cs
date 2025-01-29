
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics.CodeAnalysis;

namespace TAFitting.Filter;

internal abstract class SavitzkyGolayFilterCubic : IFilter
{
    protected string name, description;
    protected double coefficient0;
    protected double[] coefficients;
    protected readonly int n;

    /// <inheritdoc/>
    public string Name => this.name;

    /// <inheritdoc/>
    public string Description => this.description;

    public SavitzkyGolayFilterCubic()
    {
        Initialize();
        this.n = this.coefficients.Length;
    } // ctor ()

    [MemberNotNull(nameof(name), nameof(description), nameof(coefficient0), nameof(coefficients))]
    abstract protected void Initialize();

    /// <inheritdoc/>
    public IReadOnlyList<double> Filter(IReadOnlyList<double> time, IReadOnlyList<double> signal)
    {
        if (time.Count != signal.Count)
            throw new ArgumentException("The number of time points and signal points must be the same.");

        if (time.Count < this.n)
            throw new ArgumentException("The number of points must be greater than or equal to 15.");

        var filtered = new double[time.Count];
        for (var i = 0; i < time.Count; ++i)
        {
            if (i < this.n || i >= time.Count - this.n)
            {
                filtered[i] = signal[i];
                continue;
            }

            filtered[i] = signal[i] * this.coefficient0;
            for (var j = 0; j < this.n; ++j)
            {
                filtered[i] += signal[i - this.n + j] * this.coefficients[j];
                filtered[i] += signal[i + this.n - j] * this.coefficients[j];
            }
        }

        return filtered;
    } // public IReadOnlyList<double> Filter (IReadOnlyList<double>, IReadOnlyList<double>)
} // internal abstract class SavitzkyGolayFilterCubic : IFilter


// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Stats;

internal sealed class TDist : StatsDist
{
    private static readonly double SqrtPi = Math.Sqrt(Math.PI);

    /// <inheritdoc/>
    public override double ProbabilityDensityFunction(double x, int dof)
        => Gamma(dof + 1) / Math.Sqrt(Math.PI * dof) / Gamma(dof) * Math.Pow(1 + x * x / dof, -(dof + 1) / 2.0);

    /// <summary>
    /// Computes gamma function for a half of a positive integer.
    /// </summary>
    /// <param name="n">An integer whose half value is at which gamma function is computed.</param>
    /// <returns>The value of gamma function at <paramref name="n"/>/2.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static double Gamma(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        if (n % 2 == 0) // Γ(n/2) is equal to the factorial of n/2 if n is an even integer.
        {
            var g = 1;

            var a = n >> 1;
            for (var i = 2; i < a; ++i)
                g *= i;

            return g;
        }
        else // Γ(n/2)
        {
            var g = SqrtPi;

            for (var i = 1; i < n; i += 2)
                g *= i / 2.0;

            return g;
        }
    } // private static double Gamma (int)
} // internal sealed class TDist : StatsDist

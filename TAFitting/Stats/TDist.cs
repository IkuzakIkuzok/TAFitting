
// (c) 2024-2025 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Stats;

/// <summary>
/// Represents a t-distribution.
/// </summary>
internal sealed class TDist : StatsDist
{
    /*
     * t-distribution becomes a standard normal distribution as the degree of freedom approaches infinity,
     * which can be computed significantly faster than the t-distribution.
     */
    private const int DOF_THRESHOLD = 30;
    private const int CACHE_SIZE = 128;

    private static readonly double SqrtPi = Math.Sqrt(Math.PI);
    private static readonly double SqrtTwoPi = Math.Sqrt(2 * Math.PI);
    private static readonly double[] gamma_cache = new double[CACHE_SIZE];

    /// <inheritdoc/>
    override public double ProbabilityDensityFunction(double x, int dof)
    {
        if (dof >= DOF_THRESHOLD)
            return MathUtils.FastExp(-x * x / 2) / SqrtTwoPi;  // Standard normal distribution N(0, 1)

        // Gamma(dof + 1) / Math.Sqrt(Math.PI * dof) / Gamma(dof) * Math.Pow(1 + x * x / dof, -(dof + 1) / 2.0);

        var g = Gamma(dof + 1) / Gamma(dof);
        var p = MathUtils.FastPower(1 / (1 + x * x / dof), dof + 1);
        var s = Math.Sqrt(p / (Math.PI * dof));
        return g * s;
    } // override public double ProbabilityDensityFunction (double, int)

    /// <inheritdoc/>
    override public double CumulativeDistributionFunction(double x, int dof)
    {
        return 1 - SurvivalFunction(x, dof);
    } // override public double CumulativeDistributionFunction (double, int)

    /// <inheritdoc/>
    override public double SurvivalFunction(double x, int dof)
    {
        // ACM Algorithm #395
        // See
        //     https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/november/test-run-the-t-test-using-csharp
        // for C# implementation.

        double n = dof; // to sync with ACM parameter name
        double a, b, y;

        x *= x;
        y = x / n;
        b = y + 1.0;
        if (y > 1.0e-6) y = Math.Log(b);
        a = n - 0.5;
        b = 48.0 * a * a;
        y = a * y;
        y = (((((-0.4 * y - 3.3) * y - 24.0) * y - 85.5) /
          (0.8 * y * y + 100.0 + b) + y + 3.0) / b + 1.0) *
          Math.Sqrt(y);

        return StatsUtils.Gaussian(-y); // ACM algorithm 209
    } // private static double Gamma (int)

    /// <summary>
    /// Computes gamma function for a half of a positive integer.
    /// </summary>
    /// <param name="n">An integer whose half value is at which gamma function is computed.</param>
    /// <returns>The value of gamma function at <paramref name="n"/>/2.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static double Gamma(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        if (n < CACHE_SIZE && gamma_cache[n] > 0) return gamma_cache[n];

        if (n % 2 == 0) // Γ(n/2) is equal to the factorial of n/2 if n is an even integer.
        {
            var g = 1;

            var a = n >> 1;
            for (var i = 2; i < a; ++i)
                g *= i;

            if (n < CACHE_SIZE)
                gamma_cache[n] = g;
            return g;
        }
        else // Γ(n/2)
        {
            var g = SqrtPi;

            for (var i = 1; i < n; i += 2)
                g *= i;

            g /= (2 << ((n >> 1) - 1));
            if (n < CACHE_SIZE)
                gamma_cache[n] = g;
            return g;
        }
    } // private static double Gamma (int)
} // internal sealed class TDist : StatsDist

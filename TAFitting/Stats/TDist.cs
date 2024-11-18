
// (c) 2024 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Stats;

internal sealed class TDist : StatsDist
{
    private const int CACHE_SIZE = 128;

    private static readonly double SqrtPi = Math.Sqrt(Math.PI);
    private static readonly double[] gamma_cache = new double[CACHE_SIZE];

    /// <inheritdoc/>
    override public double ProbabilityDensityFunction(double x, int dof)
    {
        // Gamma(dof + 1) / Math.Sqrt(Math.PI * dof) / Gamma(dof) * Math.Pow(1 + x * x / dof, -(dof + 1) / 2.0);

        var g = Gamma(dof + 1) / Gamma(dof);
        var p = FastPower(1 / (1 + x * x / dof), dof + 1);
        var s = Math.Sqrt(p / (Math.PI * dof));
        return g * s;
    } // override  public double ProbabilityDensityFunction (double, int)

    /// <summary>
    /// Computes a power of a number quickly.
    /// </summary>
    /// <param name="a">A double-precision floating-point number to be raised to a power.</param>
    /// <param name="b">An integer number that specifies a power.</param>
    /// <returns>The number <paramref name="a"/> raised to the power <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double FastPower(double a, int b)
    {
        var r = 1.0;
        while (b > 0)
        {
            if ((b & 1) == 1) r *= a;
            a *= a;
            b >>= 1;
        }
        return r;
    } // private static double FastPower (double, int)

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


// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Stats;

/// <summary>
/// A base class of statistical distributions.
/// </summary>
internal abstract class StatsDist
{
    private const double EPSILON = 1e-10;

    /// <summary>
    /// Evaluates a probability density function with the specified value and degree of freedom.
    /// </summary>
    /// <param name="x">The value at which the function is evaluated.</param>
    /// <param name="dof">The degree of freedom.</param>
    /// <returns>The calculated function value.</returns>
    public virtual double ProbabilityDensityFunction(double x, int dof)
        => throw new NotImplementedException();

    /// <summary>
    /// Curries a probability density function with a constant degree of freedom.
    /// </summary>
    /// <param name="dof">The degree of freedom.</param>
    /// <returns>A possibility density function with the specified degree of freedom.</returns>
    protected virtual Func<double, double> PDF(int dof)
        => (x) => ProbabilityDensityFunction(x, dof);

    /// <summary>
    /// Evaluates a cumulative distribution function with the specified value and degree of freedom.
    /// </summary>
    /// <param name="x">The value at which the function is evaluated.</param>
    /// <param name="dof">The degree of freedom.</param>
    /// <returns>The calculated function value.</returns>
    public virtual double CumulativeDistributionFunction(double x, int dof)
        => Integrate(PDF(dof), -1024.0, x);

    /// <summary>
    /// Evaluates a survival function with the specified value and degree of freedom.
    /// </summary>
    /// <param name="x">The value at which the function is evaluated.</param>
    /// <param name="dof">The degree of freedom.</param>
    /// <returns>The calculated function value.</returns>
    public virtual double SurvivalFunction(double x, int dof)
        => Integrate(PDF(dof), x, 1024.0);

    /// <summary>
    /// Evaluates an inverse survival function with the specified value and degree of freedom.
    /// </summary>
    /// <param name="x">The value at which the function is evaluated.</param>
    /// <param name="dof">The degree of freedom.</param>
    /// <returns>The calculated function value.</returns>
    public virtual double InverseSurvivalFunction(double x, int dof)
    {
        var x0 = double.MinValue;
        var x1 = .0;

        // Newton's method
        // The first derivative of the survival function is a negative of the probability density function.
        var iter = 0;
        while (!CompareDoubles(x0, x1) && iter++ < 128)
        {
            x0 = x1;
            x1 += (SurvivalFunction(x1, dof) - x) / ProbabilityDensityFunction(x1, dof);
        }

        return x1;
    } // public virtual double InverseSurvivalFunction (double, int)

    /// <summary>
    /// Integrates the specified function on an interval [a, b].
    /// </summary>
    /// <param name="func">The function to be integrated.</param>
    /// <param name="a">The lower bound of integration range.</param>
    /// <param name="b">The upper bound of integration range.</param>
    /// <returns>The calculated value.</returns>
    protected static double Integrate(Func<double, double> func, double a, double b)
    {
        var s0 = double.MinValue;
        var s1 = .0;

        var n = 4096;
        var iter = 0;
        while (!CompareDoubles(s0, s1) && iter++ < 8)
        {
            s0 = s1;
            s1 = Integrate(func, a, b, n);
            n <<= 1;
            if ((b - a) / n < EPSILON) break;
        }

        return s1;
    } // protected static double Integrate (Func<double, double>, double, double)

    /// <summary>
    /// Integrates the specified function on an interval [a, b] with the specified iteration count.
    /// </summary>
    /// <param name="func">The function to be integrated.</param>
    /// <param name="a">The lower bound of integration range.</param>
    /// <param name="b">The upper bound of integration range.</param>
    /// <param name="n">The number of sections for each iteration.</param>
    /// <returns>The calculated value.</returns>
    protected static double Integrate(Func<double, double> func, double a, double b, int n)
    {
        var S = .0;
        var w = (b - a) / n;
        for (var i = 1; i < n - 1; ++i)
        {
            var x1 = a + i * w;
            S += func(x1);
        }

        S *= w;
        S += (func(a) + func(b)) * w / 2;
        return S;
    } // protected static double Integrate (Func<double, double>, double, double, int)

    /// <summary>
    /// Compares two <see cref="double"/> value.
    /// </summary>
    /// <param name="x">The first value to be compared.</param>
    /// <param name="y">The second value to be compared.</param>
    /// <returns><see langword="true"/> if two values are seem to be equal to each other;
    /// otherwise, <see langword="false"/>.</returns>
    protected static bool CompareDoubles(double x, double y)
        => Math.Abs(x - y) < EPSILON * Math.Max(Math.Abs(x), Math.Abs(y));
} // internal abstract class StatsDist

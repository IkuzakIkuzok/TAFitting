
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Stats;

/// <summary>
/// Provides extension methods for statistical calculations.
/// </summary>
internal static class StatsUtils
{
    private static readonly TDist TDist = new();

    /// <summary>
    /// Calculates the variance of a sequence of double-precision floating-point numbers.
    /// </summary>
    /// <param name="source">The sequence of double-precision floating-point numbers.</param>
    /// <param name="ddof">The delta degrees of freedom.</param>
    /// <returns>The variance of the sequence of double-precision floating-point numbers.</returns>
    internal static double Variance(this IEnumerable<double> source, int ddof = 1)
    {
        ArgumentNullException.ThrowIfNull(source);

        var s1 = .0;
        var s2 = .0;
        var n = -ddof;
        foreach (var x in source)
        {
            s1 += x;
            s2 += x * x;
            ++n;
        }
        var avg = s1 / n;
        return s2 / n - avg * avg;
    } // internal static double Variance (this IEnumerable<double>, [int])

    /// <summary>
    /// Calculates the standard deviation of a sequence of double-precision floating-point numbers.
    /// </summary>
    /// <param name="source">The sequence of double-precision floating-point numbers.</param>
    /// <param name="ddof">The delta degrees of freedom.</param>
    /// <returns>The standard deviation of the sequence of double-precision floating-point numbers.</returns>
    internal static double StandardDeviation(this IEnumerable<double> source, int ddof = 0)
        => Math.Sqrt(source.Variance(ddof));

    /// <summary>
    /// Removes the outliers from a sequence of double-precision floating-point numbers using the Smirnov-Grubbs test.
    /// </summary>
    /// <param name="source">The sequence of double-precision floating-point numbers.</param>
    /// <param name="alpha">The significance level.</param>
    /// <returns>The sequence of double-precision floating-point numbers without outliers.</returns>
    internal static List<double> SmirnovGrubbs(this IEnumerable<double> source, double alpha = .05)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source.Order().ToList();
        while (true)
        {
            var n = list.Count;
            if (n <= 2) break;
            var t = TDist.InverseSurvivalFunction(alpha / (n << 1), n - 2);
            var tau = (n - 1) * t / Math.Sqrt(n * (n - 2) + n * t * t);
            var mu = list.Average();
            var std = list.StandardDeviation();
            var i_far = Math.Abs(list[n - 1] - mu) > Math.Abs(list[0] - mu) ? n - 1 : 0;
            var tau_far = Math.Abs((list[i_far] - mu) / std);
            if (tau_far < tau) break;
            list.RemoveAt(i_far);
        }

        return list;
    } // internal static List<double> SmirnovGrubbs (IEnumerable<double>, [double])
} // internal static class StatsUtils

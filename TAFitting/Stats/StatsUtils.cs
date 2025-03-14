
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Stats;

/// <summary>
/// Provides extension methods for statistical calculations.
/// </summary>
internal static class StatsUtils
{
    private static readonly TDist TDist = new();

    /// <summary>
    /// Calculates the cumulative distribution function of the standard normal distribution.
    /// </summary>
    /// <param name="z">The value.</param>
    /// <returns>The p value under the standard normal distribution from negative infinity to the <paramref name="z"/>.</returns>
    internal static double Gaussian(double z)
    {
        // ACM Algorithm #209
        // See
        //     https://learn.microsoft.com/en-us/archive/msdn-magazine/2015/november/test-run-the-t-test-using-csharp
        // for C# implementation.

        double y; // 209 scratch variable
        double p; // result. called 'z' in 209
        double w; // 209 scratch variable

        if (z == 0.0)
        {
            p = 0.0;
        }
        else
        {
            y = Math.Abs(z) / 2;
            if (y >= 3.0)
            {
                p = 1.0;
            }
            else if (y < 1.0)
            {
                w = y * y;
                p = ((((((((0.000124818987 * w
                  - 0.001075204047) * w + 0.005198775019) * w
                  - 0.019198292004) * w + 0.059054035642) * w
                  - 0.151968751364) * w + 0.319152932694) * w
                  - 0.531923007300) * w + 0.797884560593) * y * 2.0;
            }
            else
            {
                y -= 2.0;
                p = (((((((((((((-0.000045255659 * y
                  + 0.000152529290) * y - 0.000019538132) * y
                  - 0.000676904986) * y + 0.001390604284) * y
                  - 0.000794620820) * y - 0.002034254874) * y
                  + 0.006549791214) * y - 0.010557625006) * y
                  + 0.011630447319) * y - 0.009279453341) * y
                  + 0.005353579108) * y - 0.002141268741) * y
                  + 0.000535310849) * y + 0.999936657524;
            }
        }

        if (z > 0.0)
            return (p + 1.0) / 2;
        else
            return (1.0 - p) / 2;
    } // internal static double Gaussian (double)

    /// <summary>
    /// Calculates the average of a sequence of double-precision floating-point numbers.
    /// </summary>
    /// <param name="source">The sequence of double-precision floating-point numbers.</param>
    /// <returns>The average of the sequence of double-precision floating-point numbers.</returns>
    internal static double AverageNumbers(this IEnumerable<double> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var sum = .0;
        var count = 0;
        foreach (var value in source)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) continue;
            sum += value;
            ++count;
        }
        return sum / count;
    } // internal static double AverageNumbers (IEnumerable<double>)

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
    /// Calculates the average and standard deviation of a sequence of double-precision floating-point numbers.
    /// </summary>
    /// <param name="source">The sequence of double-precision floating-point numbers.</param>
    /// <param name="ddof">The delta degrees of freedom.</param>
    /// <returns>The average and standard deviation of the sequence of double-precision floating-point numbers.</returns>
    internal static (double, double) AverageAndStandardDeviation(this IEnumerable<double> source, int ddof = 0)
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
        var var = s2 / n - avg * avg;
        return (avg, Math.Sqrt(var));
    } // internal static (double, double) AverageAndStandardDeviation (this IEnumerable<double>, [int])

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
            var (mu, std) = list.AverageAndStandardDeviation();
            var i_far = Math.Abs(list[n - 1] - mu) > Math.Abs(list[0] - mu) ? n - 1 : 0;
            var tau_far = Math.Abs((list[i_far] - mu) / std);
            if (tau_far < tau) break;
            list.RemoveAt(i_far);
        }

        return list;
    } // internal static List<double> SmirnovGrubbs (IEnumerable<double>, [double])
} // internal static class StatsUtils


// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Provides interpolation methods.
/// </summary>
internal static class Interpolation
{
    /// <summary>
    /// Interpolates the sampled data to the resampled data using the specified interpolation mode.
    /// </summary>
    /// <param name="mode">The interpolation mode.</param>
    /// <param name="sample_x">The x values of the sampled data.</param>
    /// <param name="sample_y">The y values of the sampled data.</param>
    /// <param name="resampled_x">The x values of the resampled data.</param>
    /// <param name="resampled_y">The y values of the resampled data.</param>
    /// <exception cref="NotSupportedException">The specified interpolation mode is not supported.</exception>
    /// <exception cref="ArgumentException">
    /// The lengths of <paramref name="sample_x"/> and <paramref name="sample_y"/> must be the same.
    /// - or -
    /// The lengths of <paramref name="resampled_x"/> and <paramref name="resampled_y"/> must be the same.
    /// - or -
    /// <paramref name="sample_x"/> must be monotonically increasing.
    /// - or -
    /// The length of <paramref name="sample_x"/> and <paramref name="sample_y"/> must be greater than 2.
    /// - or -
    /// The length of <paramref name="resampled_x"/> and <paramref name="resampled_y"/> must be greater than 1.
    /// </exception>
    internal static void Interpolate(InterpolationMode mode, ReadOnlySpan<double> sample_x, ReadOnlySpan<double> sample_y, Span<double> resampled_x, Span<double> resampled_y)
    {
        if (sample_x.Length != sample_y.Length)
            throw new ArgumentException("The lengths of sample_x and sample_y must be the same.");
        if (resampled_x.Length != resampled_y.Length)
            throw new ArgumentException("The lengths of resampled_x and resampled_y must be the same.");
        if (!VerifyMonotonicIncreasing(sample_x))
            throw new ArgumentException("sample_x must be monotonically increasing.");

        if (sample_x.Length <= 2)
            throw new ArgumentException("The length of sample_x and sample_y must be greater than 2.");
        if (resampled_x.Length <= 1)
            throw new ArgumentException("The length of resampled_x and resampled_y must be greater than 1.");

        switch (mode)
        {
            case InterpolationMode.Linear:
                LinearInterpolate(sample_x, sample_y, resampled_x, resampled_y);
                break;
            case InterpolationMode.Spline:
                SplineInterpolate(sample_x, sample_y, resampled_x, resampled_y);
                break;
            default:
                throw new NotSupportedException($"The interpolation mode '{mode}' is not supported.");
        }
    } // internal static void Interpolate (InterpolationMode, ReadOnlySpan<double>, ReadOnlySpan<double>, Span<double>, Span<double>)

    /// <summary>
    /// Performs linear interpolation from the sampled data to the resampled data.
    /// </summary>
    /// <param name="sample_x">The x values of the sampled data.</param>
    /// <param name="sample_y">The y values of the sampled data.</param>
    /// <param name="resampled_x">The x values of the resampled data.</param>
    /// <param name="resampled_y">The y values of the resampled data.</param>
    private static void LinearInterpolate(ReadOnlySpan<double> sample_x, ReadOnlySpan<double> sample_y, Span<double> resampled_x, Span<double> resampled_y)
    {
        var sample_n = sample_x.Length;
        var resampled_n = resampled_x.Length;

        var dx = (sample_x[^1] - sample_x[0]) / (resampled_n - 1);

        resampled_x[0] = sample_x[0];
        resampled_x[^1] = sample_x[^1];

        resampled_y[0] = sample_y[0];
        resampled_y[^1] = sample_y[^1];

        for (var i = 1; i < resampled_n - 1; ++i)
        {
            var x = sample_x[0] + i * dx;
            resampled_x[i] = x;

            var index = sample_x.BinarySearch(x);
            if (index < 0) index = ~index;

            if (index == 0)
            {
                resampled_y[i] = sample_y[0];
                continue;
            }
            if (index == sample_n)
            {
                resampled_y[i] = sample_y[^1];
                continue;
            }

            var t1 = sample_x[index - 1];
            var t2 = sample_x[index];
            var s1 = sample_y[index - 1];
            var s2 = sample_y[index];
            resampled_y[i] = s1 + (s2 - s1) * (x - t1) / (t2 - t1);
        }
    } // private static void LinearInterpolate (ReadOnlySpan<double>, ReadOnlySpan<double>, double[], double[])

    /// <summary>
    /// Performs spline interpolation from the sampled data to the resampled data.
    /// </summary>
    /// <param name="sample_x">The x values of the sampled data.</param>
    /// <param name="sample_y">The y values of the sampled data.</param>
    /// <param name="resampled_x">The x values of the resampled data.</param>
    /// <param name="resampled_y">The y values of the resampled data.</param>
    private static void SplineInterpolate(ReadOnlySpan<double> sample_x, ReadOnlySpan<double> sample_y, Span<double> resampled_x, Span<double> resampled_y)
    {
        // 0x1000 for 288 KiB stack allocation at maximum (9 double arrays of size 0x1000)
        // The stack size is 1 MiB by default in .NET applications on Windows.
        const int MAX_STACKALLOC_SIZE = 0x1000;

        var sample_n = sample_x.Length;
        var resampled_n = resampled_x.Length;

        var h = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n]) : new double[sample_n];
        for (var i = 1; i < sample_n; ++i)
            h[i] = sample_x[i] - sample_x[i - 1];

        var alpha = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n + 1]) : new double[sample_n + 1];
        var beta  = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n]) : new double[sample_n];
        var gamma = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n + 1]) : new double[sample_n + 1];
        var delta = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n + 1]) : new double[sample_n + 1];

        alpha[1] = 2 * h[1];
        beta[1] = h[1];
        delta[1] = 3 * (sample_y[1] - sample_y[0]);

        for (var i = 2; i < sample_n; ++i)
        {
            alpha[i] = 2 * (h[i - 1] + h[i]);
            beta[i] = h[i - 1];
            gamma[i] = h[i];
            delta[i] = 3 * (h[i] / h[i - 1] * (sample_y[i - 1] - sample_y[i - 2]) + h[i - 1] / h[i] * (sample_y[i] - sample_y[i - 1]));
        }

        alpha[^1] = 2 * h[^1];
        gamma[^1] = h[^1];
        delta[^1] = 3 * (sample_y[^1] - sample_y[^2]);

        // Forward elimination
        for (var i = 1; i < sample_n; ++i)
        {
            var w = gamma[i + 1] / alpha[i];
            alpha[i + 1] -= w * beta[i];
            delta[i + 1] -= w * delta[i];
        }

        var a = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n]) : new double[sample_n];
        var b = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n]) : new double[sample_n];
        var c = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n + 1]) : new double[sample_n + 1];
        var d = sample_n < MAX_STACKALLOC_SIZE ? (stackalloc double[sample_n]) : new double[sample_n];

        // Back substitution
        c[^1] = delta[^1] / alpha[^1];
        for (var i = sample_n - 1; i >= 1; --i)
            c[i] = (delta[i] - beta[i] * c[i + 1]) / alpha[i];
        for (var i = 1; i < sample_n; ++i)
        {
            var dy_h = (sample_y[i] - sample_y[i - 1]) / h[i];
            a[i] = (1 / (h[i] * h[i])) * (c[i] + c[i + 1] - 2 * dy_h);
            b[i] = (1 / h[i]) * (-2 * c[i] - c[i + 1] + 3 * dy_h);
            d[i] = sample_y[i - 1];
        }

        // Interpolation for resampled points
        var x_min = sample_x[0];
        var dx = (sample_x[^1] - x_min) / (resampled_n - 1);

        resampled_x[0] = x_min;
        resampled_x[^1] = sample_x[^1];

        resampled_y[0] = sample_y[0];
        resampled_y[^1] = sample_y[^1];

        for (var i = 1; i < resampled_n - 1; ++i)
        {
            var x = x_min + i * dx;
            resampled_x[i] = x;

            var index = sample_x.BinarySearch(x);
            if (index < 0) index = ~index;
            if (index == 0)
            {
                resampled_y[i] = sample_y[0];
                continue;
            }
            if (index == sample_n)
            {
                resampled_y[i] = sample_y[^1];
                continue;
            }
            var t = x - sample_x[index - 1];
            resampled_y[i] = a[index] * t * t * t + b[index] * t * t + c[index] * t + d[index];
        }
    } // private static void SplineInterpolate (ReadOnlySpan<double>, ReadOnlySpan<double>, Span<double>, Span<double>)

    /// <summary>
    /// Verifies that the given array is monotonically increasing.
    /// </summary>
    /// <param name="array">The array to verify.</param>
    /// <returns><see langword="true"/> if the array is monotonically increasing; otherwise, <see langword="false"/>.</returns>
    private static bool VerifyMonotonicIncreasing(ReadOnlySpan<double> array)
    {
        for (var i = 1; i < array.Length; i++)
        {
            if (array[i] <= array[i - 1])
                return false;
        }
        return true;
    } // private static bool VerifyMonotonicIncreasing (ReadOnlySpan<double>)
} // internal static class Interpolation

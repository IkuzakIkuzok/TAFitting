
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
    internal static void Interpolate(InterpolationMode mode, double[] sample_x, double[] sample_y, double[] resampled_x, double[] resampled_y)
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
            default:
                throw new NotSupportedException($"The interpolation mode '{mode}' is not supported.");
        }
    } // internal static void Interpolate (InterpolationMode, double[], double[], double[], double[])

    /// <summary>
    /// Performs linear interpolation from the sampled data to the resampled data.
    /// </summary>
    /// <param name="sample_x">The x values of the sampled data.</param>
    /// <param name="sample_y">The y values of the sampled data.</param>
    /// <param name="resampled_x">The x values of the resampled data.</param>
    /// <param name="resampled_y">The y values of the resampled data.</param>
    private static void LinearInterpolate(double[] sample_x, double[] sample_y, double[] resampled_x, double[] resampled_y)
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

            var index = Array.BinarySearch(sample_x, x);
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
    } // private static void LinearInterpolate (double[], double[], double[], double[])

    /// <summary>
    /// Verifies that the given array is monotonically increasing.
    /// </summary>
    /// <param name="array">The array to verify.</param>
    /// <returns><see langword="true"/> if the array is monotonically increasing; otherwise, <see langword="false"/>.</returns>
    private static bool VerifyMonotonicIncreasing(double[] array)
    {
        for (var i = 1; i < array.Length; i++)
        {
            if (array[i] <= array[i - 1])
                return false;
        }
        return true;
    } // private static bool VerifyMonotonicIncreasing (double[])
} // internal static class Interpolation

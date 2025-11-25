
// (c) 2025 Kazuki KOHZUKI

using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Data;

namespace TAFitting.Controls.Charting;

/// <summary>
/// Provides extension methods for the <see cref="Series"/> class.
/// </summary>
internal static class SeriesExtension
{
    private static readonly Dictionary<Series, List<DataPoint>> dataPointsCaches = [];

    extension(Series series)
    {
        /// <summary>
        /// Gets the cached list of data points for the current series.
        /// </summary>
        private List<DataPoint> DataPointsCache => dataPointsCaches.GetOrAdd(series, _ => []);
        
        /// <summary>
        /// Ensures that the data points cache contains at least the specified number of elements.
        /// </summary>
        /// <param name="size">The minimum number of elements that the data points cache should contain. Must be non-negative.</param>
        private void EnsureCacheSize(int size)
        {
            var toAdd = size - series.DataPointsCache.Count;
            if (toAdd <= 0) return;

            series.DataPointsCache.EnsureCapacity(size);

            for (var i = 0; i < toAdd; i++)
                series.DataPointsCache.Add(null!);
        } // private void EnsureCacheSize (int size)

        /// <summary>
        /// Clears all cached data points associated with the current series.
        /// </summary>
        internal void ClearCache()
        {
            series.DataPointsCache.Clear();
            dataPointsCaches.Remove(series);
        } // private void ClearCache ()

        /// <summary>
        /// Retrieves the data point at the specified index, creating and initializing it if it does not already exist.
        /// </summary>
        /// <param name="index">The zero-based index of the data point to retrieve or create. Must be greater than or equal to zero.</param>
        /// <returns>The data point at the specified index. If the data point does not exist or is empty, a new data point is created and returned.</returns>
        private DataPoint GetOrCreateDataPoint(int index)
        {
            var cache = series.DataPointsCache;
            var p = cache[index];

            // Setting DataPoint.IsEmpty property raises redrawing and is computationally more expensive than creating a new DataPoint.
            // Therefore, a new instance is created to replace the empty one.
            if (p?.IsEmpty ?? true)
                p = cache[index] = new(series);

            return p;
        } // internal DataPoint GetOrCreateDataPoint (int index)

        /// <summary>
        /// Returns a span containing the first specified number of data points from the cache.
        /// </summary>
        /// <param name="length">The number of data points to include in the returned span. Must be between 0 and the total number of cached data points, inclusive.</param>
        /// <returns>A span of type DataPoint containing up to the specified number of elements from the beginning of the data points cache.</returns>
        private Span<DataPoint> GetPointsAsSpan(int length)
            => CollectionsMarshal.AsSpan(series.DataPointsCache)[..length];
        
        /// <summary>
        /// Adds a decay sequence to the current series, optionally inverting the signal values.
        /// </summary>
        /// <param name="decay">The decay sequence to add.</param>
        /// <param name="invert">If set to <see langword="true"/>, inverts the signal values of the decay sequence before adding; otherwise, adds them as-is.</param>
        internal void AddDecay(Decay decay, bool invert = false)
            => series.AddPositivePoints(decay.GetTimesAsSpan(), decay.GetSignalsAsSpan(), invert);

        /// <summary>
        /// Adds data points to the series for each positive X value in the provided spans.
        /// </summary>
        /// <param name="xValues">A read-only span containing the X values for the data points.
        /// Each value must be greater than 0. The length must match that of <paramref name="yValues"/>.</param>
        /// <param name="yValues">A read-only span containing the Y values for the data points.
        /// Each value is paired with the corresponding value in <paramref name="xValues"/>.</param>
        /// <param name="invert"><see langword="true"/> to invert the sign of all Y values before adding them to the series; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</param>
        /// <exception cref="ArgumentException">Thrown if the length of <paramref name="xValues"/> does not equal the length of <paramref name="yValues"/>.</exception>
        internal void AddPositivePoints(ReadOnlySpan<double> xValues, ReadOnlySpan<double> yValues, bool invert = false)
        {
            if (xValues.Length != yValues.Length)
                throw new ArgumentException("The length of times must be equal to the length of signals.", nameof(xValues));

            series.EnsureCacheSize(xValues.Length);
            series.Points.Clear();

            var count = 0;
            var sign = invert ? -1 : 1;
            for (var i = xValues.Length - 1; i >= 0; i--)
            {
                var x = xValues[i];
                if (x <= 0) break;

                var y = yValues[i];
                if (!double.IsFinite(y)) continue;
                y = Math.Clamp(y, UIUtils.DecimalMin, UIUtils.DecimalMax) * sign;

                var p = series.GetOrCreateDataPoint(count);
                p.SetValueXY(x, y);
                ++count;
            }
            series.Points.AddRange(series.GetPointsAsSpan(count));
            series.Points.Invalidate();
        } // internal void AddPositivePoints (ReadOnlySpan<double>, ReadOnlySpan<double>, [bool])

        /// <summary>
        /// Adds data points to the series by evaluating a function for each positive X value in the provided span.
        /// </summary>
        /// <remarks>Existing points in the series are cleared before new points are added. Non-finite y-values are ignored.
        /// The resulting y-values are clamped to a predefined range before being added to the series.</remarks>
        /// <param name="xValues">A read-only span of x-values for which the function will be evaluated.
        /// Only positive values are processed; processing stops at the first non-positive value.</param>
        /// <param name="func">A function that computes the y-value for each x-value. The function is called once for each valid x-value.</param>
        /// <param name="invert"><see langword="true"/> to invert the sign of all Y values before adding them to the series; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</param>
        internal void AddPositivePoints(ReadOnlySpan<double> xValues, Func<double, double> func, bool invert = false)
        {
            series.EnsureCacheSize(xValues.Length);
            series.Points.Clear();

            var count = 0;
            var sign = invert ? -1 : 1;
            for (var i = xValues.Length - 1; i >= 0; i--)
            {
                var x = xValues[i];
                if (x <= 0) break;

                var y = func(x);
                if (!double.IsFinite(y)) continue;
                y = Math.Clamp(y, UIUtils.DecimalMin, UIUtils.DecimalMax) * sign;

                var p = series.GetOrCreateDataPoint(count);
                p.SetValueXY(x, y);
                ++count;
            }
            series.Points.AddRange(series.GetPointsAsSpan(count));
            series.Points.Invalidate();
        } // internal void AddPositivePoints (ReadOnlySpan<double>, Func<double, double>, [bool])

        /// <summary>
        /// Adds a data point with the specified X and Y values to the series.
        /// </summary>
        /// <param name="x">The X value of the data point to add.</param>
        /// <param name="y">The Y value of the data point to add.</param>
        internal void AddPoint(double x, double y)
        {
            series.EnsureCacheSize(series.Points.Count + 1);
            var p = series.GetOrCreateDataPoint(series.Points.Count);
            p.SetValueXY(x, y);
            series.Points.Add(p);
            series.Points.Invalidate();
        } // internal void AddPoint (double, double)
    } // extension(Series series)
} // internal static class SeriesExtension

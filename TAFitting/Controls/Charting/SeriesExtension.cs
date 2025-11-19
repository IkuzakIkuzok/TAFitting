
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
            => series.AddDecay(decay.GetTimesAsSpan(), decay.GetSignalsAsSpan(), invert);

        /// <summary>
        /// Adds a decay curve to the series using the specified time and signal values.
        /// </summary>
        /// <param name="times">A span of time values representing the x-coordinates for the decay curve. Each value must be greater than zero to be included.</param>
        /// <param name="signals">A span of signal values corresponding to each time value. Only finite values are included.</param>
        /// <param name="invert"><see langword="true"/> to invert the sign of each signal value; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</param>
        /// <exception cref="ArgumentException">Thrown if the length of <paramref name="times"/> is greater than the length of <paramref name="signals"/>.</exception>
        internal void AddDecay(ReadOnlySpan<double> times, ReadOnlySpan<double> signals, bool invert = false)
        {
            if (times.Length > signals.Length)
                throw new ArgumentException("The length of times must not be greater than that of signals.", nameof(times));

            series.EnsureCacheSize(times.Length);
            series.Points.Clear();

            var count = 0;
            var sign = invert ? -1 : 1;
            for (var i = 0; i < times.Length; i++)
            {
                var x = times[i];
                var y = signals[i];
                if (x <= 0) continue;
                if (!double.IsFinite(y)) continue;
                y = Math.Clamp(y, UIUtils.DecimalMin, UIUtils.DecimalMax) * sign;

                var p = series.GetOrCreateDataPoint(count);
                p.SetValueXY(x, y);
                ++count;
            }
            series.Points.AddRange(series.GetPointsAsSpan(count));
            series.Points.Invalidate();
        } // internal void AddDecay (ReadOnlySpan<double>, ReadOnlySpan<double>, [bool])

        /// <summary>
        /// Adds a series of decay data points to the underlying chart series using the specified time values and decay function.
        /// </summary>
        /// <remarks>Decay values produced by the function are clamped to a valid range before being added to the series.
        /// Existing points in the series are cleared before new points are added.</remarks>
        /// <param name="times">A read-only span of time values, in the same units expected by the decay function. Only positive values are processed; non-positive values are ignored.</param>
        /// <param name="func">A function that computes the decay value for a given time. The function should return a finite double value for valid input times.</param>
        /// <param name="invert"><see langword="true"/> to invert the sign of each signal value; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</param>
        internal void AddDecay(ReadOnlySpan<double> times, Func<double, double> func, bool invert = false)
        {
            series.EnsureCacheSize(times.Length);
            series.Points.Clear();

            var count = 0;
            var sign = invert ? -1 : 1;
            for (var i = 0; i < times.Length; i++)
            {
                var x = times[i];
                if (x <= 0) continue;

                var y = func(x);
                if (!double.IsFinite(y)) continue;
                y = Math.Clamp(y, UIUtils.DecimalMin, UIUtils.DecimalMax) * sign;

                var p = series.GetOrCreateDataPoint(count);
                p.SetValueXY(x, y);
                ++count;
            }
            series.Points.AddRange(series.GetPointsAsSpan(count));
            series.Points.Invalidate();
        } // internal void AddDecay (ReadOnlySpan<double>, Func<double, double>, [bool])
    } // extension(Series series)
} // internal static class SeriesExtension

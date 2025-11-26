
// (c) 2025 Kazuki KOHZUKI

using System.Windows.Forms.DataVisualization.Charting;

namespace TAFitting.Controls.Charting;

/// <summary>
/// Provides a pool for reusing <see cref="CacheSeries"/> instances to optimize resource usage and reduce allocations within charting operations.
/// </summary>
/// <remarks>This class is intended for internal use to manage the lifecycle of <see cref="CacheSeries"/> objects.
/// Callers are responsible for returning rented instances to the pool after use.
/// Reusing series instances can improve performance in scenarios where charts are frequently updated or recreated.</remarks>
internal sealed class SeriesPool
{
    private int seriesCount = 0;
    private readonly Stack<CacheSeries> pool = [];

    /// <summary>
    /// Retrieves a reusable <see cref="CacheSeries"/> instance from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <remarks>Callers are responsible for returning the Series instance to the pool when it is no longer needed.
    /// This method is intended for internal use to optimize resource reuse and reduce allocations.</remarks>
    /// <returns>A <see cref="CacheSeries"/> instance that can be used by the caller. The returned instance may be newly created or previously used.</returns>
    internal CacheSeries Rent()
    {
        var series = this.pool.Count > 0
            ? this.pool.Pop()
            : new CacheSeries($"Pool-{Interlocked.Increment(ref this.seriesCount)}");

        return series;
    } // internal CacheSeries Rent ()

    /// <summary>
    /// Returns a <see cref="CacheSeries"/> instance to the pool for reuse after resetting its state.
    /// </summary>
    /// <remarks>This method clears the points and legend text of the provided Series before returning it to the pool.
    /// After calling this method, the Series should not be used by the caller unless it is retrieved from the pool again.</remarks>
    /// <param name="series">The <see cref="CacheSeries"/> instance to be returned to the pool.</param>
    internal void Return(CacheSeries series)
    {
        series.Points.Clear();
        series.LegendText = string.Empty;
        
        this.pool.Push(series);
    } // internal void Return (CacheSeries series)

    /// <summary>
    /// Returns all series in the specified chart and clears the chart's series collection.
    /// </summary>
    /// <param name="chart">The chart whose series are to be returned and cleared.</param>
    internal void ReturnAll(Chart chart)
    {         
        foreach (var series in chart.Series)
            if (series is CacheSeries cs)
                Return(cs);
        chart.Series.Clear();
    } // internal void ReturnAll (Chart chart)
} // internal sealed class SeriesPool

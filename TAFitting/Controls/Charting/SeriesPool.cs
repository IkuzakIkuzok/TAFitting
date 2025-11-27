
// (c) 2025 Kazuki KOHZUKI

using System.Collections.Concurrent;
using System.Windows.Forms.DataVisualization.Charting;

namespace TAFitting.Controls.Charting;

/// <summary>
/// Provides a pool for reusing <see cref="CacheSeries"/> instances to optimize resource usage and reduce allocations within charting operations. This class is thread-safe.
/// </summary>
/// <remarks>This class is intended for internal use to manage the lifecycle of <see cref="CacheSeries"/> objects.
/// Callers are responsible for returning rented instances to the pool after use.
/// Reusing series instances can improve performance in scenarios where charts are frequently updated or recreated.</remarks>
internal sealed class SeriesPool
{
    private int seriesCount = 0;
    private readonly ConcurrentStack<CacheSeries> pool = [];

    /// <summary>
    /// Retrieves a reusable <see cref="CacheSeries"/> instance from the pool, or creates a new one if the pool is empty.
    /// </summary>
    /// <remarks>Callers are responsible for returning the <see cref="CacheSeries"/> instance to the pool when it is no longer needed.
    /// This method is intended for internal use to optimize resource reuse and reduce allocations.</remarks>
    /// <returns>A <see cref="CacheSeries"/> instance that can be used by the caller. The returned instance may be newly created or previously used.</returns>
    internal CacheSeries Rent()
    {
        if (!this.pool.TryPop(out var series))
            series = new CacheSeries($"Pool-{Interlocked.Increment(ref this.seriesCount)}");

        return series;
    } // internal CacheSeries Rent ()

    /// <summary>
    /// Rents and configures a cached series instance with the specified chart type, color, and optional styling parameters.
    /// </summary>
    /// <param name="chartType">The chart type to apply to the rented series. Determines how data points are visually represented.</param>
    /// <param name="color">The color used to render the series in the chart.</param>
    /// <param name="dashStyle">The dash style for the series border. Defaults to <see cref="ChartDashStyle.NotSet"/> if not specified.</param>
    /// <param name="markerStyle">The marker style for data points in the series. Defaults to <see cref="MarkerStyle.None"/> if not specified.</param>
    /// <param name="borderWidth">The width, in pixels, of the series border. Must be non-negative. Defaults to 0.</param>
    /// <param name="markerSize">The size, in pixels, of the markers for data points. Must be non-negative. Defaults to 0.</param>
    /// <param name="legendText">The text to display for the series in the chart legend. Defaults to an empty string if not specified.</param>
    /// <returns>A <see cref="CacheSeries"/> instance configured with the specified chart type, color, and styling options.</returns>
    internal CacheSeries Rent(
        SeriesChartType chartType, Color color,
        ChartDashStyle dashStyle = ChartDashStyle.NotSet,
        MarkerStyle markerStyle = MarkerStyle.None,
        int borderWidth = 0, int markerSize = 0,
        string legendText = ""
    )
    {
        var series = Rent();

        series.ChartType = chartType;
        series.Color = color;
        series.BorderDashStyle = dashStyle;
        series.MarkerStyle = markerStyle;
        series.BorderWidth = borderWidth;
        series.MarkerSize = markerSize;
        series.LegendText = legendText;

        return series;
    } // internal CacheSeries Rent (SeriesChartType chartType, Color color, [ChartDashStyle], [MarkerStyle], [int], [int], [string])

    /// <summary>
    /// Rents a line chart series configured with the specified color, border width, dash style, marker style, marker size, and legend text.
    /// </summary>
    /// <param name="color">The color used to render the line series.</param>
    /// <param name="borderWidth">The width, in pixels, of the line's border. Must be greater than zero.</param>
    /// <param name="dashStyle">The dash style applied to the line. Defaults to <see cref="ChartDashStyle.Solid"/>.</param>
    /// <param name="markerStyle">The style of marker displayed at each data point. Defaults to <see cref="MarkerStyle.None"/>.</param>
    /// <param name="markerSize">The size, in pixels, of the marker. Must be zero or greater. If zero, no marker is shown.</param>
    /// <param name="legendText">The text to display for the series in the chart legend. If empty, no legend entry is created.</param>
    /// <returns>A <see cref="CacheSeries"/> instance representing the configured line chart series.</returns>
    internal CacheSeries RentLine(
        Color color, int borderWidth,
        ChartDashStyle dashStyle = ChartDashStyle.Solid,
        MarkerStyle markerStyle = MarkerStyle.None,
        int markerSize = 0,
        string legendText = ""
    )
        => Rent(
            SeriesChartType.Line, color,
            dashStyle  : dashStyle,
            markerStyle: markerStyle,
            borderWidth: borderWidth,
            markerSize : markerSize,
            legendText : legendText
        );

    /// <summary>
    /// Returns a <see cref="CacheSeries"/> instance to the pool for reuse after resetting its state.
    /// </summary>
    /// <remarks>This method clears the points and legend text of the provided Series before returning it to the pool.
    /// After calling this method, the Series should not be used by the caller unless it is retrieved from the pool again.</remarks>
    /// <param name="series">The <see cref="CacheSeries"/> instance to be returned to the pool.</param>
    internal void Return(CacheSeries series)
    {
        // If the series is marked to be excluded from pooling, do not return it.
        if (series.ExcludeFromPooling) return;

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

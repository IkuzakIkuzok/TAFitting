
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// A collection of <see cref="DataPlot"/> objects.
/// </summary>
internal class DataPlots
{
    private readonly dynamic dataPlots;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPlots"/> class with the specified data plots.
    /// </summary>
    /// <param name="dataPlots">The Origin data plots object.</param>
    internal DataPlots(dynamic dataPlots)
    {
        this.dataPlots = dataPlots;
    } // ctor (dynamic)

    /// <summary>
    /// Adds a new data plot to the collection.
    /// </summary>
    /// <param name="dataRange">The data range to plot.</param>
    /// <param name="plotType">The type of the plot.</param>
    /// <returns>The new data plot.</returns>
    internal DataPlot Add(DataRange dataRange, PlotTypes plotType)
        => new(this.dataPlots.Add(dataRange.OriginDataRange, plotType));
} // internal class DataPlots

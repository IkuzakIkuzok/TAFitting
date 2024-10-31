
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents a data plot in Origin.
/// </summary>
internal class DataPlot
{
    private readonly dynamic dataPlot;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataPlot"/> class with the specified data plot.
    /// </summary>
    /// <param name="dataPlot">The Origin data plot object.</param>
    internal DataPlot(dynamic dataPlot)
    {
        this.dataPlot = dataPlot;
    } // ctor (dynamic)
} // internal class DataPlot

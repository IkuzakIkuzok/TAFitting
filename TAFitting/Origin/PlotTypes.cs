
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents a plot type in Origin.
/// </summary>
internal enum PlotTypes
{
    /// <summary>
    /// A line plot.
    /// </summary>
    PlotLine = 200,

    /// <summary>
    /// A scatter plot.
    /// </summary>
    PlotScatter = 201,

    /// <summary>
    /// A line plot with symbols.
    /// </summary>
    PlotLineSymbol = 202,

    /// <summary>
    /// A column plot.
    /// </summary>
    PlotColumn = 203,

    /// <summary>
    /// An area plot.
    /// </summary>
    PlotArea = 204,

    /// <summary>
    /// A high-low-close plot.
    /// </summary>
    PlotHiLoClose = 205,

    /// <summary>
    /// A box plot.
    /// </summary>
    PlotBox = 206,

    /// <summary>
    /// A contour plot.
    /// </summary>
    PlotContour = 226,

    /// <summary>
    /// A scatter plot in 3D.
    /// </summary>
    Plot3DScatter = 240,

    /// <summary>
    /// A surface plot in 3D.
    /// </summary>
    Plot3DSurface = 242,

    /// <summary>
    /// A contour plot in xyz.
    /// </summary>
    PlotXyzContour = 243,

    /// <summary>
    /// A ternary plot in xyz.
    /// </summary>
    PlotXyzTernary = 245,
} // internal enum PlotTypes

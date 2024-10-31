
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents a data range in Origin.
/// </summary>
internal class DataRange
{
    private readonly dynamic dataRange;

    /// <summary>
    /// Gets the origin data range.
    /// </summary>
    internal object OriginDataRange => this.dataRange;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRange"/> class with the specified data range.
    /// </summary>
    /// <param name="dataRange">The Origin data range object.</param>
    internal DataRange(dynamic dataRange)
    {
        this.dataRange = dataRange;
    } // ctor (dynamic)
} // internal class DataRange

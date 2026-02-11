
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Specifies the type of segment within an Excel formula template, such as a literal value or a placeholder.
/// </summary>
internal enum ExcelFormulaSegmentType
{
    /// <summary>
    /// Lieteral text segment.
    /// </summary>
    Literal,

    /// <summary>
    /// Placeholder for a parameter name.
    /// </summary>
    ParameterPlaceholder,

    /// <summary>
    /// Placeholder for time variable.
    /// </summary>
    TimePlaceholder
} // enum ExcelFormulaSegmentType
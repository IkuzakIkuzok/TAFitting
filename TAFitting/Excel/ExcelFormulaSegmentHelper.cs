
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Provides extension methods for creating segments of Excel formulas from string literals and parameter placeholders.
/// </summary>
internal static class ExcelFormulaSegmentHelper
{
    /// <summary>
    /// Creates an Excel formula segment representing a parameter placeholder for the specified column index.
    /// </summary>
    /// <param name="columnIndex">The zero-based index of the column for which to create the parameter placeholder segment. Must be non-negative.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> that represents a parameter placeholder for the given column index.</returns>
    internal static ExcelFormulaSegment AsParameterPlaceholderSegment(this int columnIndex)
        => ExcelFormulaSegment.CreateParameterPlaceholder(columnIndex);
} // internal static class ExcelFormulaSegmentHelper

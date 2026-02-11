
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Represents a segment of an Excel formula, including its type and associated value.
/// </summary>
/// <param name="Type">The type of the formula segment, indicating its role within the formula (such as operator, function, or operand).</param>
/// <param name="Value">The value associated with the segment, such as the literal text or token value. May be <see langword="null"/> if the segment type does not require a value.</param>
internal record struct ExcelFormulaSegment(ExcelFormulaSegmentType Type, string? Value);

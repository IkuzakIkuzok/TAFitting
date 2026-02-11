
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Represents a segment of an Excel formula, including its type and associated value.
/// </summary>
/// <param name="Type">The type of the formula segment, indicating its role within the formula (such as operator, function, or operand).</param>
/// <param name="Value">The value associated with the segment, such as the literal text or the column index.</param>
internal record struct ExcelFormulaSegment(ExcelFormulaSegmentType Type, ExcelFormulaSegmentValue Value)
{
    /// <summary>
    /// Calculates the maximum possible length of the formatted address or placeholder represented by this instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int GetMaxLength()
    {
        if (this.Type == ExcelFormulaSegmentType.Literal)
            return this.Value.LiteralText.Length;

        if (this.Type == ExcelFormulaSegmentType.TimePlaceholder)
        {
            // Possible max address is "XFD$1" (row index is always 1 for time)
            // 3 for column letters + 1 for '$' + 1 for row number
            return 5;
        }

        // Parameter placeholder
        // Possible max address is "$XFD1048576"
        // 3 for column letters + 1 for '$' + 7 for row number
        return 11;
    } // internal int GetMaxLength ()
} // internal struct ExcelFormulaSegment

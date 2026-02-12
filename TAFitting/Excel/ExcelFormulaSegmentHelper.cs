
// (c) 2026 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;

namespace TAFitting.Excel;

/// <summary>
/// Provides extension methods for creating segments of Excel formulas from string literals and parameter placeholders.
/// </summary>
internal static class ExcelFormulaSegmentHelper
{
    /// <summary>
    /// Creates a literal formula segment representing a substring of the specified string.
    /// </summary>
    /// <param name="chars">The source string from which the literal segment is extracted.</param>
    /// <param name="start">The zero-based starting index of the substring within the source string. Must be within the bounds of the string.</param>
    /// <param name="length">The number of characters to include in the literal segment. Must not exceed the available characters from the starting index.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> that represents the specified substring as a literal segment.</returns>
    internal static ExcelFormulaSegment AsLiteralSegment(this string chars, int start, int length)
    {
        // This branch is never executed because it is a constant when JIT-compiled
        if (Environment.Is64BitProcess)
        {
            // The cast to uint before the cast to ulong ensures zero-extending rather than sign-extending
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)chars.Length)
                ThrowStartIsOutOfRange(nameof(start));
        }
        else
        {
            // 64-bit arithmetic operation is too expensive to execute on 32-bit machine
            // Only use 32-bit operations to check the arguments
            if ((uint)start > (uint)chars.Length || (uint)length > (uint)(chars.Length - start))
                ThrowStartIsOutOfRange(nameof(start));
        }

        return ExcelFormulaSegment.CreateLiteral(chars, start, length);
    } // internal static ExcelFormulaSegment AsLiteralSegment (this string, int, int)

    /// <summary>
    /// Creates a literal formula segment from the specified substring of the input string.
    /// </summary>
    /// <param name="chars">The string containing the characters to be used for the literal segment.</param>
    /// <param name="start">The zero-based index in <paramref name="chars"/> at which the literal segment begins. Must be between 0 and the length of <paramref name="chars"/>.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> representing the substring of <paramref name="chars"/> starting at <paramref name="start"/> and extending to the end of the string.</returns>
    internal static ExcelFormulaSegment AsLiteralSegment(this string chars, int start)
    {
        if ((uint)start > (uint)chars.Length)
            ThrowStartIsOutOfRange(nameof(start));

        return ExcelFormulaSegment.CreateLiteral(chars, start, chars.Length - start);
    } // internal static ExcelFormulaSegment AsLiteralSegment (this string, int)

    /// <summary>
    /// Creates a literal formula segment representing the specified string.
    /// </summary>
    /// <param name="chars">The string to be used as the content of the literal segment. Cannot be null.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> that represents the literal value of the specified string.</returns>
    internal static ExcelFormulaSegment AsLiteralSegment(this string chars)
        => ExcelFormulaSegment.CreateLiteral(chars, 0, chars.Length);

    /// <summary>
    /// Creates an Excel formula segment representing a parameter placeholder for the specified column index.
    /// </summary>
    /// <param name="columnIndex">The zero-based index of the column for which to create the parameter placeholder segment. Must be non-negative.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> that represents a parameter placeholder for the given column index.</returns>
    internal static ExcelFormulaSegment AsParameterPlaceholderSegment(this int columnIndex)
        => ExcelFormulaSegment.CreateParameterPlaceholder(columnIndex);

    [DoesNotReturn]
    private static void ThrowStartIsOutOfRange(string paramName)
        => throw new ArgumentOutOfRangeException(paramName, "The start index is out of range.");
} // internal static class ExcelFormulaSegmentHelper

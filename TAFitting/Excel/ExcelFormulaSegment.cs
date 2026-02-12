
// (c) 2026 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Represents a segment of an Excel formula, including its type and associated value.
/// </summary>
internal readonly struct ExcelFormulaSegment
{
    private readonly string? _chars;
    private readonly int _index;
    private readonly int _length;

    /// <summary>
    /// Gets the type of the segment.
    /// </summary>
    internal ExcelFormulaSegmentType Type
    {
        get
        {
            if (this._chars is not null)
                return ExcelFormulaSegmentType.Literal;
            if (this._index < 0)
                return ExcelFormulaSegmentType.TimePlaceholder;
            return ExcelFormulaSegmentType.ParameterPlaceholder;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the current instance represents a literal value.
    /// </summary>
    /// <value><see langword="true"/> if the current instance represents a literal value; otherwise, <see langword="false"/>.</value>
    [MemberNotNullWhen(true, nameof(_chars))]
    internal bool IsLiteral
        => this._chars is not null;

    /// <summary>
    /// Gets a value indicating whether the current instance represents an empty literal.
    /// </summary>
    /// <value><see langword="true"/> if the current instance represents an empty literal; otherwise, <see langword="false"/>.</value>
    internal bool IsEmpty
        => this.IsLiteral && this._length == 0;

    /// <summary>
    /// Gets a read-only span of characters representing the literal value, if available.
    /// </summary>
    /// <value>
    /// A read-only span of characters representing the literal value.
    /// If the current instance does not represent a literal, an empty span is returned.
    /// </value>
    internal ReadOnlySpan<char> Span
    {
        get
        {
            if (this.IsLiteral)
                return this._chars.AsSpan(this._index, this._length);
            return [];
        }
    }

    /// <summary>
    /// Gets the length of the segment.
    /// </summary>
    internal int Length => this._length;

    /// <summary>
    /// Gets the column index for parameter placeholders; returns -1 for literals and time placeholders.
    /// </summary>
    internal int ColumnIndex
    {
        get
        {
            if (this.IsLiteral) return -1;
            return this._index;
        }
    }

    private ExcelFormulaSegment(string? chars, int index, int length)
    {
        this._chars = chars;
        this._index = index;
        this._length = length;
    } // ctor (string?, int, int)

    /// <summary>
    /// Creates a new literal segment representing a substring within an Excel formula.
    /// </summary>
    /// <param name="chars">The source string containing the formula characters.</param>
    /// <param name="index">The zero-based starting index of the substring to be used as the literal segment.
    /// Must be non-negative and within the bounds of <paramref name="chars"/>.</param>
    /// <param name="length">The number of characters to include in the literal segment.
    /// Must be non-negative and not exceed the length of <paramref name="chars"/> starting at <paramref name="index"/>.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> instance representing the specified substring as a literal segment.</returns>
    internal static ExcelFormulaSegment CreateLiteral(string chars, int index, int length)
        => new(chars, index, length);

    /// <summary>
    /// Creates a placeholder segment for a formula parameter at the specified column index.
    /// </summary>
    /// <param name="columnIndex">The index of the column where the parameter placeholder will be positioned.</param>
    /// <returns>An instance of <see cref="ExcelFormulaSegment"/> representing a parameter placeholder at the given column index.</returns>
    internal static ExcelFormulaSegment CreateParameterPlaceholder(int columnIndex)
        => new(null, columnIndex, 0);

    /// <summary>
    /// Creates a placeholder segment representing a time value within an Excel formula.
    /// </summary>
    /// <returns>An <see cref="ExcelFormulaSegment"/> instance configured as a time placeholder segment.</returns>
    internal static ExcelFormulaSegment CreateTimePlaceholder()
        => new(null, -1, 0);

    /// <summary>
    /// Calculates the maximum possible length of the formatted address or placeholder represented by this instance.
    /// </summary>
    /// <returns>The maximum length of the formatted address or placeholder.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int GetMaxLength()
    {
        if (this.IsLiteral)
            return this._length;

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

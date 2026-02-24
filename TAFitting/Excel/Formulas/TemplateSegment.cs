
// (c) 2026 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Represents a segment of an Excel formula, including its type and associated value.
/// </summary>
internal readonly struct TemplateSegment
{
    private readonly string? _chars;
    private readonly int _index;
    private readonly int _length; // For literal segments, this represents the length of the substring. For placeholders, this represents the constant length of the segment in the formula.

    /// <summary>
    /// Gets the type of the segment.
    /// </summary>
    internal TemplateSegmentType Type
    {
        get
        {
            if (this._chars is not null)
                return TemplateSegmentType.Literal;
            if (this._index < 0)
                return TemplateSegmentType.TimePlaceholder;
            return TemplateSegmentType.ParameterPlaceholder;
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
        => this._length == 0;

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
    internal int Length => this.IsLiteral ? this._length : 0;

    /// <summary>
    /// Gets the zero-based index of the parameter placeholder, if applicable.
    /// </summary>
    internal int ParameterIndex
    {
        get
        {
            if (this.IsLiteral) return -1;
            return this._index;
        }
    }

    private TemplateSegment(string? chars, int index, int length)
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
    /// <returns>An <see cref="TemplateSegment"/> instance representing the specified substring as a literal segment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TemplateSegment CreateLiteral(string chars, int index, int length)
        => new(chars, index, length);

    /// <summary>
    /// Creates a placeholder segment for a formula parameter at the specified column index.
    /// </summary>
    /// <param name="columnIndex">The index of the column where the parameter placeholder will be positioned.</param>
    /// <returns>An instance of <see cref="TemplateSegment"/> representing a parameter placeholder at the given column index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TemplateSegment CreateParameterPlaceholder(int columnIndex)
    {
        const uint columnOffset = 2u; // +1 for wavelength column, +1 for 1-based index
        var constLen = FormattingHelper.GetColumnIndexLength((uint)columnIndex + columnOffset) + 1; // +1 for '$';

        return new(null, columnIndex, constLen);
    } // internal static TemplateSegment CreateParameterPlaceholder (int)

    /// <summary>
    /// Creates a placeholder segment representing a time value within an Excel formula.
    /// </summary>
    /// <returns>An <see cref="TemplateSegment"/> instance configured as a time placeholder segment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static TemplateSegment CreateTimePlaceholder()
        => new(null, -1, 2); // Const part = "$1", length = 2

    /// <summary>
    /// Calculates the constant length of the formula segment based on its type and properties.
    /// </summary>
    /// <returns>The number of characters representing the segment in its constant form.
    /// The value varies depending on whether the segment is a literal, a time placeholder, or a column reference.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int GetConstLength()
        => this._length;
} // internal struct TemplateSegment

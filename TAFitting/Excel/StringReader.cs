
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Provides sequential reading and span-based searching capabilities over a string, enabling efficient parsing and extraction of character segments.
/// </summary>
internal ref struct StringReader
{
    private readonly string _text;
    private ReadOnlySpan<char> _span;
    private int _read;

    internal readonly bool IsEnd => this._span.IsEmpty;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringReader"/> class that reads from the specified string.
    /// </summary>
    /// <param name="str">The string to be read by the <see cref="StringReader"/>.</param>
    internal StringReader(string str)
    {
        this._text = str;
        this._span = str.AsSpan();
    } // internal StringReader (string)

    /// <summary>
    /// Returns the zero-based index of the first occurrence of the specified substring within the current span.
    /// </summary>
    /// <param name="value">The substring to locate within the span.</param>
    /// <returns>The zero-based index of the first occurrence of the specified substring, or -1 if the substring is not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int Indexof(ReadOnlySpan<char> value)
        => this._span.IndexOf(value);

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified character within the current span.
    /// </summary>
    /// <param name="value">The character to locate within the span.</param>
    /// <returns>The zero-based index of the first occurrence of the specified character, or -1 if the character is not found.</returns>
    [method:MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int IndexOf(char value)
        => this._span.IndexOf(value);

    /// <summary>
    /// Reports the zero-based index of the first occurrence of the specified string within the current span.
    /// </summary>
    /// <param name="value">The string to locate within the span. Can be null or empty, in which case -1 is returned.</param>
    /// <returns>The zero-based index position of the first occurrence of the specified string, or -1 if the string is not found.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly int IndexOf(string value)
        => this._span.IndexOf(value);

    /// <summary>
    /// Advances the current position by the specified number of elements.
    /// </summary>
    /// <remarks>Calling this method updates the internal state to reflect the new position.
    /// If the specified count is invalid, the behavior is undefined.</remarks>
    /// <param name="count">The number of elements to advance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Advance(int count)
    {
        this._span = this._span[count..];
        this._read += count;
    } // internal void Advance (int)

    /// <summary>
    /// Returns a read-only memory segment containing the next specified number of characters from the underlying text buffer.
    /// </summary>
    /// <param name="length">The number of characters to read from the current position.</param>
    /// <returns>A read-only memory region of characters representing the requested segment.
    /// The region will be empty if the specified length is zero.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ReadOnlyMemory<char> Read(int length)
    {
        var start = this._read;
        Advance(length);
        return this._text.AsMemory(start, length);
    } // internal ReadOnlyMemory<char> Read (int)

    /// <summary>
    /// Reads a literal segment from the current formula text to the end of the span, returning it as an <see cref="ExcelFormulaSegment"/>.
    /// </summary>
    /// <returns>An <see cref="ExcelFormulaSegment"/> representing the literal segment extracted from the formula text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExcelFormulaSegment ReadLiteralSegment()
    {
        var start = this._read;
        var length = this._span.Length;
        Advance(length);
        return ExcelFormulaSegment.CreateLiteral(this._text, start, length);
    } // internal readonly ExcelFormulaSegment ReadLiteralSegment ()

    /// <summary>
    /// Reads a literal segment of the specified length from the current formula text, returning it as an <see cref="ExcelFormulaSegment"/>.
    /// </summary>
    /// <param name="length">The number of characters to include in the literal segment.</param>
    /// <returns>An <see cref="ExcelFormulaSegment"/> representing the literal segment read from the formula text.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ExcelFormulaSegment ReadLiteralSegment(int length)
    {
        var start = this._read;
        Advance(length);
        return ExcelFormulaSegment.CreateLiteral(this._text, start, length);
    } // internal readonly ExcelFormulaSegment ReadLiteralSegment (int)
} // internal ref struct StringReader

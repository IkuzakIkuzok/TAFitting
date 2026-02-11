
// (c) 2026 Kazuki KOHZUKI

using TAFitting.Buffers;

namespace TAFitting.Excel;

/// <summary>
/// Represents a template for generating model function formulas with parameterized segments and placeholders.
/// </summary>
internal readonly ref struct ModelFunctionTemplate
{
    private readonly ReadOnlySpan<ModelFunctionSegment> _segments;
    private readonly int _rowIndex;
    private readonly int _maxLength;

    /// <summary>
    /// Initializes a new instance of the ModelFunctionTemplate class with the specified segments and row index.
    /// </summary>
    /// <param name="segments">A read-only span of ModelFunctionSegment values that defines the segments to be used by the template.</param>
    /// <param name="rowIndex">The index of the row associated with this template.</param>
    internal ModelFunctionTemplate(ReadOnlySpan<ModelFunctionSegment> segments, int rowIndex)
    {
        this._segments = segments;
        this._rowIndex = rowIndex;

        this._maxLength = 0;
        foreach (ref readonly var segment in segments)
            this._maxLength += segment.GetMaxLength();
    } // ctor (ReadOnlySpan<ModelFunctionSegment>, int)

    /// <summary>
    /// Generates a formula string representation for the specified column index.
    /// </summary>
    /// <param name="columnIndex">The index of the column for which to generate the formula.</param>
    /// <returns>A string containing the formula corresponding to the specified column index.</returns>
    internal string ToFormula(int columnIndex)
    {
        using var pooled = new PooledBuffer<char>(this._maxLength);
        var buffer = this._maxLength <= 1024 ? stackalloc char[this._maxLength] : pooled.GetSpan();
        var length = WriteFormula(buffer, columnIndex);
        return new(buffer[..length]);
    } // internal string ToFormula (int)

    private int WriteFormula(Span<char> span, int columnIndex)
    {
        var len = 0;

        Span<char> WriteLiteral(Span<char> buffer, ReadOnlySpan<char> text)
        {
            text.CopyTo(buffer);
            len += text.Length;
            return buffer[text.Length..];
        } // Span<char> WriteLiteral (Span<char>, ReadOnlySpan<char>)

        Span<char> WriteColumnLetters(Span<char> buffer, int col)
        {
            var letters = (stackalloc char[4]); // Excel columns go up to XFD (16384), +1 for safety
            var pos = 0;
            while (col > 0)
            {
                var mod = (col - 1) % 26;
                letters[pos++] = (char)('A' + mod);
                col = (col - mod) / 26;
            }
            for (var i = pos - 1; i >= 0; i--)
            {
                buffer[0] = letters[i];
                buffer = buffer[1..];
            }
            len += pos;
            return buffer;
        } // Span<char> WriteColumnLetters (Span<char>, int)

        Span<char> WriteValue<T>(Span<char> buffer, T value) where T : ISpanFormattable
        {
            if (!value.TryFormat(buffer, out var written, format: null, provider: null))
                throw new FormatException("Failed to format the value.");

            len += written;
            return buffer[written..];
        } // Span<char> WriteValue<T> (Span<char>, T) where T : ISpanFormattable

        foreach (ref readonly var segment in this._segments)
        {
            if (segment.IsLiteral)
            {
                span = WriteLiteral(span, segment.Text.Span);
                continue;
            }

            if (segment.ArgIndex == 0) // Time placeholder
            {
                
                span = WriteColumnLetters(span, columnIndex);
                span = WriteLiteral(span, "$1");
                continue;
            }

            // Parameter placeholder
            span = WriteLiteral(span, "$");
            span = WriteColumnLetters(span, segment.ArgIndex);
            span = WriteValue(span, this._rowIndex);
        }

        return len;
    } // private int WriteFormula (Span<char>, int)
} // internal readonly ref struct ModelFunctionTemplate

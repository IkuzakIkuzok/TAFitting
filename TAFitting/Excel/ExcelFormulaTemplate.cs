
// (c) 2026 Kazuki KOHZUKI

using TAFitting.Buffers;
using TAFitting.Collections;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a parsed Excel formula template that supports parameter and time placeholders for dynamic formula generation.
/// </summary>
internal sealed class ExcelFormulaTemplate
{
    #region cache

    private static volatile ExcelFormulaTemplate? _cache;

    /// <summary>
    /// Retrieves an instance of an Excel formula template for the specified template string.
    /// </summary>
    /// <param name="template">The formula template string to retrieve or create an instance for.</param>
    /// <returns>An instance of <see cref="ExcelFormulaTemplate"/> corresponding to the specified template string.</returns>
    internal static ExcelFormulaTemplate GetInstance(IFittingModel model)
    {
        var entry = _cache;

        if (string.Equals(model.ExcelFormula, entry?.BaseString, StringComparison.Ordinal))
            return entry!;

        return _cache = new ExcelFormulaTemplate(model);
    } // static ExcelFormulaTemplate GetInstance (IFittingModel)

    #endregion cache

    private readonly ExcelFormulaSegment[] _segments;
    private readonly int _maxLength;

    /// <summary>
    /// Gets the base string associated with the current instance.
    /// </summary>
    internal string BaseString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFormulaTemplate"/> class using the specified template string.
    /// </summary>
    /// <param name="template">The template string that defines the structure of the Excel formula.</param>
    internal ExcelFormulaTemplate(IFittingModel model)
    {
        this.BaseString = model.ExcelFormula;
        this._segments = ParseTemplateInternal(model, out this._maxLength);
    } // ctor (IFittingModel)

    /// <summary>
    /// Parses the Excel formula template from the specified fitting model and returns an array of formula segments representing literals and placeholders.
    /// </summary>
    /// <param name="model">The fitting model containing the Excel formula template and its associated parameters to be parsed.</param>
    /// <param name="maxLength">When this method returns, contains the maximum length of the resulting formula after all placeholders are expanded.
    /// This parameter is passed uninitialized.</param>
    /// <returns>An array of <see cref="ExcelFormulaSegment"/> objects representing the parsed segments of the formula template, including literals and parameter or time placeholders.</returns>
    /// <exception cref="FormatException">Thrown if the formula template contains an unmatched '[' character, indicating a malformed placeholder.</exception>
    private static ExcelFormulaSegment[] ParseTemplateInternal(IFittingModel model, out int maxLength)
    {
        var template = model.ExcelFormula;
        var parameters = model.Parameters;

        var paramMapInlineBuffer = new StructInlineArray<ParameterMapEntry>();
        using var paramMapPooledBuffer = new PooledBuffer<ParameterMapEntry>(parameters.Count);
        var parameterMap =
            parameters.Count <= StructInlineArray<ParameterMapEntry>.Capacity
            ? ((Span<ParameterMapEntry>)paramMapInlineBuffer)[..parameters.Count]
            : paramMapPooledBuffer.GetSpan();

        for (var i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            parameterMap[i] = new(param.Name.AsMemory(), i + 2); // +1 for wavelength column, +1 for 1-based index
        }

        var span = template.AsSpan();

        var segmentInlineBuffer = new StructInlineArray<ExcelFormulaSegment>();
        using var list = new ExcelFormulaSegmentList(segmentInlineBuffer);
        var read = 0;

        while (!span.IsEmpty)
        {
            var timeIdx = span.IndexOf("$X");
            var nameIdx = span.IndexOf('[');

            if (timeIdx < 0 && nameIdx < 0)
            {
                // No more placeholders; add the rest as a literal and break
                var literalSegment = template.AsLiteralSegment(read);
                list.Add(literalSegment);
                break;
            }

            if (timeIdx >= 0 && (timeIdx < nameIdx || nameIdx < 0)) // Time placeholder found before parameter placeholder
            {
                if (timeIdx > 0)
                {
                    var literalSegment = template.AsLiteralSegment(read, timeIdx);
                    list.Add(literalSegment);
                }
                var timeSegment = ExcelFormulaSegment.CreateTimePlaceholder(); ;
                list.Add(timeSegment);
                span = span[(timeIdx + 2)..];
                read += timeIdx + 2;
                continue;
            }

            // Name placeholder found before time placeholder
            if (nameIdx > 0)
            {
                var literalSegment = template.AsLiteralSegment(read, nameIdx);
                list.Add(literalSegment);
            }
            span = span[(nameIdx + 1)..]; // Skip '['
            read += nameIdx + 1;

            var endIdx = span.IndexOf(']');
            if (endIdx < 0)
                throw new FormatException("Unmatched '[' in the formula template.");

            var name = template.AsMemory(read, endIdx);
            var parameterSegment = GetParameterColumnIndex(name, parameterMap).AsParameterPlaceholderSegment();
            list.Add(parameterSegment);
            span = span[(endIdx + 1)..]; // Skip ']'
            read += endIdx + 1;
        }

        maxLength = list.TotalMaxLength;
        return list.ToArray();
    } // static ExcelFormulaSegment[] ParseTemplateInternal (IFittingModel)

    /// <summary>
    /// Retrieves the column index associated with the specified parameter name from the provided parameter map.
    /// </summary>
    /// <param name="name">The name of the parameter to locate.</param>
    /// <param name="parameterMap">A read-only span of parameter map entries to search for the specified parameter name.</param>
    /// <returns>The column index corresponding to the specified parameter name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> does not exist in the <paramref name="parameterMap"/>.</exception>
    private static int GetParameterColumnIndex(ReadOnlyMemory<char> name, ReadOnlySpan<ParameterMapEntry> parameterMap)
    {
        foreach (ref readonly var entry in parameterMap)
        {
            if (entry.Matches(name))
                return entry.ColumnIndex;
        }
        throw new KeyNotFoundException($"Parameter '{name}' not found in the model.");
    } // static int GetParameterColumnIndex (ReadOnlyMemory<char>, ReadOnlySpan<ParameterEntry>)

    /// <summary>
    /// Generates a formula string representation for the specified column index.
    /// </summary>
    /// <param name="rowIndex">The index of the row for which to generate the formula.</param>
    /// <param name="columnIndex">The index of the column for which to generate the formula.</param>
    /// <returns>A string containing the formula corresponding to the specified row and column index.</returns>
    internal string ToFormula(int rowIndex, int columnIndex)
    {
        using var pooled = new PooledBuffer<char>(this._maxLength);
        var buffer = this._maxLength <= 1024 ? stackalloc char[this._maxLength] : pooled.GetSpan();
        var length = WriteFormula(buffer, rowIndex, columnIndex);
        return new(buffer[..length]);
    } // internal string ToFormula (int, int)

    /// <summary>
    /// Writes the Excel formula represented by the current object into the specified character buffer, using the provided row and column indices for placeholder substitution.
    /// </summary>
    /// <param name="span">The character buffer to which the formula will be written.</param>
    /// <param name="rowIndex">The row index to use when substituting row placeholders in the formula.</param>
    /// <param name="columnIndex">The column index to use when substituting column placeholders in the formula.</param>
    /// <returns>The number of characters written to the buffer.</returns>
    /// <exception cref="FormatException">Thrown if a value in the formula cannot be formatted as a string.</exception>
    private int WriteFormula(Span<char> span, int rowIndex, int columnIndex)
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

        for (var i = 0; i < this._segments.Length; i++)
        {
            ref readonly var segment = ref this._segments[i];
            if (segment.Type == ExcelFormulaSegmentType.Literal)
            {
                span = WriteLiteral(span, segment.Span);
                continue;
            }

            if (segment.Type == ExcelFormulaSegmentType.TimePlaceholder)
            {
                span = WriteColumnLetters(span, columnIndex);
                span = WriteLiteral(span, "$1");
                continue;
            }

            // Parameter placeholder
            span = WriteLiteral(span, "$");
            span = WriteColumnLetters(span, segment.ColumnIndex);
            span = WriteValue(span, rowIndex);
        }

        return len;
    } // private int WriteFormula (Span<char>, int, int)
} // internal sealed class ExcelFormulaTemplate


// (c) 2026 Kazuki KOHZUKI

using System.Diagnostics;
using System.Runtime.CompilerServices;
using TAFitting.Buffers;
using TAFitting.Collections;
using TAFitting.Model;

namespace TAFitting.Excel;

/// <summary>
/// Represents a parsed Excel formula template that supports parameter and time placeholders for dynamic formula generation.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ExcelFormulaTemplate"/> class is designed to parse and store an Excel formula template string that may contain
/// placeholders for parameters (denoted by square brackets, e.g., [ParameterName]) and time values (denoted by $X).
/// It provides functionality to generate complete Excel formulas by substituting these placeholders with actual row and column indices.
/// For example, given a template like "[A] * $X + [B]", the class can generate formulas such as "$B2 * D$1 + $C2" for specific rows and columns.
/// Note that the row index for time placeholders is fixed at 1, as time values are typically located in the first row of the Excel sheet.
/// This class does not care whether placeholders are present within the quote characters or not; this keeps the implementation simple and efficient.
/// </para>
/// <para>
/// Generating formulas from a template consists of two main steps:
/// <list type="number">
/// <item>Parsing: The template string is parsed to identify literal segments and placeholders. This is done once when the template is created.</item>
/// <item>Formula Generation: For each required formula, the parsed segments are combined, substituting placeholders with the appropriate row and column indices.</item>
/// </list>
/// The first step is computationally intensive and is optimized by caching the parsed template.
/// </para>
/// <para>
/// The class needs only limited heap allocation:
/// <list type="bullet">
/// <item>An instance of the <see cref="ExcelFormulaTemplate"/> class itself</item>
/// <item>An array of <see cref="ExcelFormulaSegment"/> objects representing the parsed segments of the formula template</item>
/// <item>Final formula string instances generated on demand</item>
/// </list>
/// Any other intermediate data used during parsing or formula generation is allocated on the stack to minimize heap usage.
/// Along with caching, this design significantly reduces memory allocations and improves performance
/// when generating multiple formulas from the same template.
/// </para>
/// </remarks>
internal sealed class ExcelFormulaTemplate
{
    #region cache

    /*
     * The cache is currently designed to store only one instance of ExcelFormulaTemplate, 
     * since only one model is used during the application lifetime in most scenarios.
     * If cache misses are frequent and additional cache capacity is planned,
     * a linear search with a small fixed-size array would be a simple and effective approach 
     * than a more complex structure like Dictionary.
     */

    private static volatile ExcelFormulaTemplate? _cache;

    /// <summary>
    /// Retrieves an instance of an Excel formula template for the specified template string.
    /// </summary>
    /// <param name="model">The fitting model containing the Excel formula template to be retrieved.</param>
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
    private readonly int _constLength, _paramCount, _timeCount;

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
        this._segments = ParseTemplateInternal(model, out this._constLength, out this._paramCount, out this._timeCount);
    } // ctor (IFittingModel)

    /// <summary>
    /// Parses the Excel formula template from the specified fitting model and returns an array of formula segments representing literals and placeholders.
    /// </summary>
    /// <param name="model">The fitting model containing the Excel formula template and its associated parameters to be parsed.</param>
    /// <param name="constLength">Outputs the total length of constant segments in the parsed formula template.</param>
    /// <param name="paramCount">Outputs the total number of parameter placeholders found in the parsed formula template.</param>
    /// <param name="timeCount">Outputs the total number of time placeholders found in the parsed formula template.</param>
    /// <returns>An array of <see cref="ExcelFormulaSegment"/> objects representing the parsed segments of the formula template, including literals and parameter or time placeholders.</returns>
    /// <exception cref="FormatException">Thrown if the formula template contains an unmatched '[' character, indicating a malformed placeholder.</exception>
    private static ExcelFormulaSegment[] ParseTemplateInternal(IFittingModel model, out int constLength, out int paramCount, out int timeCount)
    {
        var reader = new StringReader(model.ExcelFormula);
        var parameters = model.Parameters;

        var paramMapInlineBuffer = new StructInlineArray<ParameterMapEntry>();
        using var paramMapPooledBuffer = new PooledBuffer<ParameterMapEntry>(parameters.Count);
        var parameterMap =
            parameters.Count <= StructInlineArray<ParameterMapEntry>.Capacity
            ? paramMapInlineBuffer.AsSpan(parameters.Count)
            : paramMapPooledBuffer.GetSpan();

        for (var i = 0; i < parameters.Count; i++)
        {
            var param = parameters[i];
            parameterMap[i] = new(param.Name, i); 
        }

        var segmentInlineBuffer = new StructInlineArray<ExcelFormulaSegment>();
        using var list = new ExcelFormulaSegmentList(segmentInlineBuffer);

        while (!reader.IsEnd)
        {
            var timeIdx = reader.IndexOf("$X");
            var nameIdx = reader.IndexOf('[');

            /*
             * Optimization Strategy:
             * 1. Prioritize checking name placeholders as they are more frequent than time placeholders.
             * 2. If both are non-negative, select the smaller value (earliest occurrence).
             * 3. Cast negative values to 'uint' to treat them as large integers (> int.MaxValue).
             *    This avoids explicit negative checks and reduces branch instructions.
             * 4. The final 'else' handles the case where both are negative (expected at most once).
             */

            if ((uint)nameIdx < (uint)timeIdx)
            {
                // Name placeholder found before time placeholder (nameIdx < timeIdx),
                // or only name placeholder found (timeIdx == -1)

                list.Add(reader.ReadLiteralSegment(nameIdx));
                reader.Advance(1); // Skip '['

                var endIdx = reader.IndexOf(']');
                if (endIdx < 0)
                    throw new FormatException("Unmatched '[' in the formula template.");

                var name = reader.Read(endIdx);
                var paramIndex = GetParameterIndex(name, parameterMap);
                var parameterSegment = ExcelFormulaSegment.CreateParameterPlaceholder(paramIndex);
                list.Add(parameterSegment);
                reader.Advance(1); // Skip ']'
            }
            else if (timeIdx >= 0)
            {
                // Time placeholder found before parameter placeholder (nameIdx > timeIdx),
                // or only time placeholder found (nameIdx == -1)

                list.Add(reader.ReadLiteralSegment(timeIdx));
                var timeSegment = ExcelFormulaSegment.CreateTimePlaceholder(); ;
                list.Add(timeSegment);
                reader.Advance(2); // Skip '$X'
            }
            else
            {
                // No more placeholders (nameIdx == -1 && timeIdx == -1);
                // add the rest as a literal and break

                list.Add(reader.ReadLiteralSegment());
                break;
            }
        }

        constLength = list.ConstantLength;
        paramCount = list.ParameterPlaceholderCount;
        timeCount = list.TimePlaceholderCount;
        return list.ToArray();
    } // static ExcelFormulaSegment[] ParseTemplateInternal (IFittingModel, out int, out int, out int)

    /// <summary>
    /// Retrieves the index associated with the specified parameter name from the provided parameter map.
    /// </summary>
    /// <param name="name">The name of the parameter to locate.</param>
    /// <param name="parameterMap">A read-only span of parameter map entries to search for the specified parameter name.</param>
    /// <returns>The zero-based index corresponding to the specified parameter name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> does not exist in the <paramref name="parameterMap"/>.</exception>
    private static int GetParameterIndex(ReadOnlySpan<char> name, ReadOnlySpan<ParameterMapEntry> parameterMap)
    {
        foreach (ref readonly var entry in parameterMap)
        {
            if (entry.Matches(name))
                return entry.ParameterIndex;
        }
        throw new KeyNotFoundException($"Parameter '{name}' not found in the model.");
    } // static int GetParameterIndex (ReadOnlySpan<char>, ReadOnlySpan<ParameterEntry>)

    /// <summary>
    /// Generates a formula string representation for the specified row and column indices
    /// by substituting the placeholders in the template with the provided values.
    /// </summary>
    /// <param name="rowIndex">The index of the row for which to generate the formula.</param>
    /// <param name="columnIndex">The index of the column for which to generate the formula.</param>
    /// <returns>A string containing the formula corresponding to the specified row and column indices.</returns>
    internal string ToFormula(int rowIndex, int columnIndex)
    {
        var row = (uint)rowIndex;
        var col = (uint)columnIndex;

        var totalLength =
            this._constLength
            + this._paramCount * ExcelFormulaFormattingHelper.GetRowIndexLength(row)
            + this._timeCount * ExcelFormulaFormattingHelper.GetColumnIndexLength(col);

        return string.Create(totalLength, new CreateState(this, row, col), WriteFormula);
    } // internal string ToFormula (int, int)

    /// <summary>
    /// Represents the state required to create an Excel formula at a specific cell location using a given template.
    /// </summary>
    /// <param name="Template">The template used to generate the Excel formula.</param>
    /// <param name="RowIndex">The row index of the cell where the formula will be created.</param>
    /// <param name="ColumnIndex">The column index of the cell where the formula will be created.</param>
    private readonly record struct CreateState(ExcelFormulaTemplate Template, uint RowIndex, uint ColumnIndex);

    /// <summary>
    /// Writes the Excel formula represented by the current object into the specified character buffer, using the provided row and column indices for placeholder substitution.
    /// </summary>
    /// <param name="span">The character buffer to which the formula will be written.</param>
    /// <param name="state">The state containing the template, row index, and column index for formula generation.</param>
    /// <returns>The number of characters written to the buffer.</returns>
    /// <exception cref="FormatException">Thrown if a value in the formula cannot be formatted as a string.</exception>
    private static void WriteFormula(Span<char> span, CreateState state)
    {
        var template = state.Template;
        var rowIndex = state.RowIndex;
        var columnIndex = state.ColumnIndex;

        // Prepare row index string on stack
        var rowDigit = ExcelFormulaFormattingHelper.GetRowIndexLength(rowIndex);
        var rowIndexSpan = (stackalloc char[rowDigit]);
        {
            ref var refRow = ref Unsafe.Add(ref MemoryMarshal.GetReference(rowIndexSpan), rowDigit);
            var d = rowDigit;
            while (rowIndex != 0 && d > 0)
            {
                d--;
                (rowIndex, var remainder) = Math.DivRem(rowIndex, 10);
                refRow = ref Unsafe.Subtract(ref refRow, 1);
                refRow = (char)('0' + remainder);
            }
        }

        // Prepare column letters on stack
        var colLettersSpan = (stackalloc char[3]);
        ref var refCol = ref MemoryMarshal.GetReference(colLettersSpan);
        var colLettersLength = WriteColumnLetters(ref refCol, columnIndex);

        ref var refDst = ref MemoryMarshal.GetReference(span);

        // Use Unsafe to avoid bounds checking on each iteration

        var segments = template._segments;
        ref var refSegment = ref segments[0];
        ref var refEnd = ref Unsafe.Add(ref refSegment, segments.Length);

        while (Unsafe.IsAddressLessThan(ref refSegment, ref refEnd))
        {
            /*
             * Segment type occurrence (for most cases):
             *   Literal > ParameterPlaceholder > TimePlaceholder
             *   
             * Branches are ordered accordingly to minimize mispredictions.
             */

            if (refSegment.Type == ExcelFormulaSegmentType.Literal)
            {
                var literal = refSegment.Span;
                ref var refLiteral = ref MemoryMarshal.GetReference(literal);

                Copy(ref refDst, literal);
                refDst = ref Unsafe.Add(ref refDst, literal.Length);
            }
            else if (refSegment.Type == ExcelFormulaSegmentType.ParameterPlaceholder)
            {
                Write(ref refDst, '$');
                refDst = ref Unsafe.Add(ref refDst, 1);

                var len = WriteColumnLetters(ref refDst, (uint)refSegment.ParameterIndex + 2u); // +1 for wavelength column, +1 for 1-based index
                refDst = ref Unsafe.Add(ref refDst, len);

                Copy(ref refDst, rowIndexSpan);
                refDst = ref Unsafe.Add(ref refDst, rowDigit);
            }
            else // if (refSegment.Type == ExcelFormulaSegmentType.TimePlaceholder)
            {
                Copy(ref refDst, colLettersSpan, colLettersLength);
                refDst = ref Unsafe.Add(ref refDst, colLettersLength);

                // Time values are always in the first row
                Write(ref refDst, '$');
                refDst = ref Unsafe.Add(ref refDst, 1);
                Write(ref refDst, '1');
                refDst = ref Unsafe.Add(ref refDst, 1);
            }

            refSegment = ref Unsafe.Add(ref refSegment, 1);
        }
    } // private static void WriteFormula (Span<char>, CreateState)

    /// <summary>
    /// Writes the specified character to the destination.
    /// </summary>
    /// <param name="refDst">A reference to the destination character..</param>
    /// <param name="c">The character to write to the destination.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Write(ref char refDst, char c)
    {
        refDst = c;
    } // private static void Write (ref char, char)

    /// <summary>
    /// Copies the contents of the specified source span to the destination location.
    /// </summary>
    /// <param name="refDst">A reference to the destination character.</param>
    /// <param name="src">The source span of characters to copy.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(ref char refDst, ReadOnlySpan<char> src)
        => Copy(ref refDst, src, src.Length);

    /// <summary>
    /// Copies a specified number of characters from the source span to the destination reference location.
    /// </summary>
    /// <param name="refDst">A reference to the destination memory location where characters will be copied.</param>
    /// <param name="src">The source span containing the characters to copy.</param>
    /// <param name="length">The number of characters to copy from the source span. Must be non-negative and not greater than the length of the source span.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Copy(ref char refDst, ReadOnlySpan<char> src, int length)
    {
        Debug.Assert((uint)length <= (uint)src.Length, "Length must be non-negative and not greater than the length of the source span.");

        ref var refSrc = ref MemoryMarshal.GetReference(src);
        Unsafe.CopyBlockUnaligned(
            ref Unsafe.As<char, byte>(ref refDst),
            ref Unsafe.As<char, byte>(ref refSrc),
            (uint)length * sizeof(char)
        );
    } // private static void Copy (ref char, ReadOnlySpan<char>, int)

    /// <summary>
    /// Writes the Excel-style column letter representation of the specified column index to the destination character reference.
    /// </summary>
    /// <param name="refDst">A reference to the destination character buffer where the column letters will be written.</param>
    /// <param name="col">The one-based column index to convert to column letters.</param>
    /// <returns>The number of characters written to the destination buffer.</returns>
    private static int WriteColumnLetters(ref char refDst, uint col)
    {
        var len = ExcelFormulaFormattingHelper.GetColumnIndexLength(col);

        ref var dstEnd = ref Unsafe.Add(ref refDst, len - 1);
        uint q, r;

        // Loop unrolling + optimization to avoid division for the last character

        if (len == 1) goto L1;
        if (len == 2) goto L2;

        q = (--col) / 26;
        r = col - (q * 26);
        col = q;
        dstEnd = (char)('A' + r);
        dstEnd = ref Unsafe.Subtract(ref dstEnd, 1);

    L2:
        q = (--col) / 26;
        r = col - (q * 26);
        col = q;
        dstEnd = (char)('A' + r);
        dstEnd = ref Unsafe.Subtract(ref dstEnd, 1);

    L1:
        dstEnd = (char)('A' - 1 + col);

        return len;
    } // private static int WriteColumnLetters (ref char, uint)
} // internal sealed class ExcelFormulaTemplate

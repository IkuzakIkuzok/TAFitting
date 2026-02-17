
// (c) 2026 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using TAFitting.Buffers;
using TAFitting.Collections;
using TAFitting.Model;

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Parses an Excel formula template into a sequence of segments, identifying literal text, parameter placeholders, and time placeholders for further processing.
/// </summary>
internal ref struct TemplateParser
{
    private readonly IFittingModel _model;
    private int _constLength, _paramPlaceholderCount, _timePlaceholderCount;

    /// <summary>
    /// Gets the constant length value associated with this instance.
    /// </summary>
    internal readonly int ConstantLength => this._constLength;

    /// <summary>
    /// Gets the number of parameter placeholders present in the associated template.
    /// </summary>
    internal readonly int ParameterPlaceholderCount => this._paramPlaceholderCount;

    /// <summary>
    /// Gets the number of time placeholders present in the associated template.
    /// </summary>
    internal readonly int TimePlaceholderCount => this._timePlaceholderCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateParser"/> class using the specified fitting model.
    /// </summary>
    /// <param name="model">The fitting model to be used for formula parsing operations.</param>
    internal TemplateParser(IFittingModel model)
    {
        this._model = model;
    } // ctor (IFittingModel)

    /// <summary>
    /// Parses the Excel formula template and returns an array of segments representing literals and placeholders.
    /// </summary>
    /// <returns>An array of <see cref="TemplateSegment"/> objects that represent the parsed segments of the formula.
    /// The array contains both literal text and placeholders for parameters and time values, in the order they appear in the template.</returns>
    /// <exception cref="FormatException">Thrown if the formula template contains an unmatched '[' character,
    /// indicating a malformed parameter placeholder.</exception>
    internal TemplateSegment[] Parse()
    {
        var reader = new TemplateReader(this._model.ExcelFormula);
        var parameters = this._model.Parameters;

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

        var segmentInlineBuffer = new StructInlineArray<TemplateSegment>();
        using var list = new TemplateSegmentList(segmentInlineBuffer);

        /*
         * Optimization Strategy:
         * 1. Update absolute indices only when the current position surpasses them, ensuring efficient parsing without unnecessary scans of the template string.
         * 2. Record whether a placeholder may exist afterward; if not, do not perform index updates, as they would be redundant.
         * 3. Prioritize checking name placeholders as they are more frequent than time placeholders.
         */

        /*
         * Absolute Indices Tracking:
         *   -1          : Not found yet but may exist later (initial state)
         *   >= 0        : Found at this absolute index
         *   int.MaxValue: Not found and will not be found later (final state)
         *   
         * If the cursor position surpasses the absolute index (or -1, indicating that it may be found later),
         * the next occurrence of the placeholder will be searched for and the absolute index will be updated.
         * If the current absolute index is int.MaxValue, indicating that the placeholder will not be found later,
         * the `absIdx < cursor` condition will always be false, and the check will be skipped.
         */
        const int NeedsSearch = -1;
        const int Exhausted = int.MaxValue;

        var nameAbsIdx = NeedsSearch;
        var timeAbsIdx = NeedsSearch;
        while (!reader.IsEnd)
        {
            var cursor = reader.Position;
            if (nameAbsIdx < cursor)
            {
                var rel = reader.IndexOf('[');
                nameAbsIdx = rel >= 0 ? cursor + rel : Exhausted;
            }

            if (timeAbsIdx < cursor)
            {
                var rel = reader.IndexOf("$X");
                timeAbsIdx = rel >= 0 ? cursor + rel : Exhausted;
            }

            if (nameAbsIdx < timeAbsIdx)
            {
                // Name placeholder found before time placeholder (nameIdx < timeIdx)

                var nameRelIdx = nameAbsIdx - cursor;
                list.Add(reader.ReadLiteralSegment(nameRelIdx));
                reader.Advance(1); // Skip '['

                var endIdx = reader.IndexOf(']');
                if (endIdx < 0)
                    throw new FormatException("Unmatched '[' in the formula template.");

                var name = reader.Read(endIdx);
                var paramIndex = GetParameterIndex(name, parameterMap);
                var parameterSegment = TemplateSegment.CreateParameterPlaceholder(paramIndex);
                list.Add(parameterSegment);
                reader.Advance(1); // Skip ']'
            }
            else if (timeAbsIdx < Exhausted)
            {
                // Time placeholder found before parameter placeholder (nameIdx > timeIdx)

                var timeRelIdx = timeAbsIdx - cursor;
                list.Add(reader.ReadLiteralSegment(timeRelIdx));
                var timeSegment = TemplateSegment.CreateTimePlaceholder(); ;
                list.Add(timeSegment);
                reader.Advance(2); // Skip '$X'
            }
            else
            {
                // No more placeholders; add the rest as a literal and break

                list.Add(reader.ReadLiteralSegment());
                break;
            }
        }

        this._constLength = list.ConstantLength;
        this._paramPlaceholderCount = list.ParameterPlaceholderCount;
        this._timePlaceholderCount = list.TimePlaceholderCount;
        return list.ToArray();
    } // internal TemplateSegment[] Parse ()

    /// <summary>
    /// Retrieves the index associated with the specified parameter name from the provided parameter map.
    /// </summary>
    /// <param name="name">The name of the parameter to locate.</param>
    /// <param name="parameterMap">A read-only span of parameter map entries to search for the specified parameter name.</param>
    /// <returns>The zero-based index corresponding to the specified parameter name.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="name"/> does not exist in the <paramref name="parameterMap"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetParameterIndex(ReadOnlySpan<char> name, ReadOnlySpan<ParameterMapEntry> parameterMap)
    {
        foreach (ref readonly var entry in parameterMap)
        {
            if (entry.Matches(name))
                return entry.ParameterIndex;
        }

        ThrowKeyNotFoundException(name);
        return -1; // Unreachable, but required to satisfy the compiler's definite assignment rules
    } // private static int GetParameterIndex (ReadOnlySpan<char>, ReadOnlySpan<ParameterEntry>)

    /// <summary>
    /// Throws a <see cref="KeyNotFoundException"/> to indicate that a parameter with the specified name was not found in the model.
    /// </summary>
    /// <param name="name">The name of the parameter that could not be found.</param>
    /// <exception cref="KeyNotFoundException">Always thrown to indicate that the specified parameter name does not exist in the model.</exception>
    [DoesNotReturn]
    private static void ThrowKeyNotFoundException(ReadOnlySpan<char> name)
        => throw new KeyNotFoundException($"Parameter '{name}' not found in the model.");
} // internal ref struct TemplateParser

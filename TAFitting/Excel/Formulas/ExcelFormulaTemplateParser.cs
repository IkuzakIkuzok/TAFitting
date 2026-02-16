
// (c) 2026 Kazuki KOHZUKI

using TAFitting.Buffers;
using TAFitting.Collections;
using TAFitting.Model;

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Parses an Excel formula template into a sequence of segments, identifying literal text, parameter placeholders, and time placeholders for further processing.
/// </summary>
internal ref struct ExcelFormulaTemplateParser
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
    /// Initializes a new instance of the <see cref="ExcelFormulaTemplateParser"/> class using the specified fitting model.
    /// </summary>
    /// <param name="model">The fitting model to be used for formula parsing operations.</param>
    internal ExcelFormulaTemplateParser(IFittingModel model)
    {
        this._model = model;
    } // ctor (IFittingModel)

    /// <summary>
    /// Parses the Excel formula template and returns an array of segments representing literals and placeholders.
    /// </summary>
    /// <returns>An array of <see cref="ExcelFormulaSegment"/> objects that represent the parsed segments of the formula.
    /// The array contains both literal text and placeholders for parameters and time values, in the order they appear in the template.</returns>
    /// <exception cref="FormatException">Thrown if the formula template contains an unmatched '[' character,
    /// indicating a malformed parameter placeholder.</exception>
    internal ExcelFormulaSegment[] Parse()
    {
        var reader = new StringReader(this._model.ExcelFormula);
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

        this._constLength = list.ConstantLength;
        this._paramPlaceholderCount = list.ParameterPlaceholderCount;
        this._timePlaceholderCount = list.TimePlaceholderCount;
        return list.ToArray();
    } // internal ExcelFormulaSegment[] Parse ()

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
    } // private static int GetParameterIndex (ReadOnlySpan<char>, ReadOnlySpan<ParameterEntry>)
} // internal ref struct ExcelFormulaTemplateParser

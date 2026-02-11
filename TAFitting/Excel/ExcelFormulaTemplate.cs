
// (c) 2026 Kazuki KOHZUKI

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
    internal static ExcelFormulaTemplate GetInstance(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        var entry = _cache;

        if (string.Equals(template, entry?.BaseString, StringComparison.Ordinal))
            return entry!;

        return _cache = new ExcelFormulaTemplate(template);
    } // static ExcelFormulaTemplate GetInstance (string)

    #endregion cache

    private readonly ExcelFormulaSegment[] _segments;

    /// <summary>
    /// Gets the number of segments contained in the collection.
    /// </summary>
    internal int SegmentCount => this._segments.Length;

    /// <summary>
    /// Gets the base string associated with the current instance.
    /// </summary>
    internal string BaseString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExcelFormulaTemplate"/> class using the specified template string.
    /// </summary>
    /// <param name="template">The template string that defines the structure of the Excel formula.</param>
    internal ExcelFormulaTemplate(string template)
    {
        ArgumentNullException.ThrowIfNull(template);

        this.BaseString = template;
        this._segments = ParseTemplateInternal(template);
    } // ctor (string)

    private static ExcelFormulaSegment[] ParseTemplateInternal(string template)
    {
        var span = template.AsSpan();
        var list = new List<ExcelFormulaSegment>();
        var read = 0;

        while (!span.IsEmpty)
        {
            var timeIdx = span.IndexOf("$X");
            var nameIdx = span.IndexOf('[');

            if (timeIdx < 0 && nameIdx < 0)
            {
                // No more placeholders; add the rest as a literal and break
                list.Add(new(ExcelFormulaSegmentType.Literal, template.AsMemory(read)));
                break;
            }

            if (timeIdx >= 0 && (timeIdx < nameIdx || nameIdx < 0)) // Time placeholder found before parameter placeholder
            {
                if (timeIdx > 0)
                {
                    var literal = template.AsMemory(read, timeIdx);
                    list.Add(new(ExcelFormulaSegmentType.Literal, literal));
                }
                list.Add(new(ExcelFormulaSegmentType.TimePlaceholder, null));
                span = span[(timeIdx + 2)..];
                read += timeIdx + 2;
                continue;
            }
            
            if (nameIdx > 0)
            {
                var literal = template.AsMemory(read, nameIdx);
                list.Add(new(ExcelFormulaSegmentType.Literal, literal));
            }
            span = span[(nameIdx + 1)..]; // Skip '['
            read += nameIdx + 1;

            var endIdx = span.IndexOf(']');
            if (endIdx < 0)
                throw new FormatException("Unmatched '[' in the formula template.");

            var name = template.AsMemory(read, endIdx);
            list.Add(new(ExcelFormulaSegmentType.ParameterPlaceholder, name));
            span = span[(endIdx + 1)..]; // Skip ']'
            read += endIdx + 1;
        }

        return [.. list];
    } // static ExcelFormulaSegment[] ParseTemplateInternal (ReadOnlySpan<char>)

    /// <summary>
    /// Binds parameter values to the function template segments for a specific row, producing a new model function template with resolved parameters.
    /// </summary>
    /// <param name="rowIndex">The index of the row for which parameters are being bound.</param>
    /// <param name="paramToColumnMap">A read-only dictionary mapping parameter names to their corresponding column indices.
    /// Each parameter placeholder in the template must have an entry in this map.</param>
    /// <param name="buffer">A span of segments used as a buffer to hold the resulting bound segments.</param>
    /// <returns>A new <see cref="ModelFunctionTemplate"/> instance with parameters bound for the specified row.</returns>
    /// <exception cref="ArgumentException">Thrown if the length of <paramref name="buffer"/> is less than the number of segments in the template.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if a parameter placeholder in the template does not have a corresponding entry in <paramref name="paramToColumnMap"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a segment in the template has an unknown segment type.</exception>
    internal ModelFunctionTemplate BindParameters(int rowIndex, ParamToColumnMap paramToColumnMap, Span<ModelFunctionSegment> buffer)
    {
        if (buffer.Length < this.SegmentCount)
            throw new ArgumentException("The buffer is too small to hold the segments.", nameof(buffer));

        for (var i = 0; i < this.SegmentCount; i++)
        {
            ref readonly var segment = ref this._segments[i];
            buffer[i] = segment.Type switch
            {
                ExcelFormulaSegmentType.Literal => new ModelFunctionSegment(segment.Value, -1),
                ExcelFormulaSegmentType.ParameterPlaceholder =>
                    paramToColumnMap.TryGetValue(segment.Value, out var colIndex) && colIndex > 0
                    ? new ModelFunctionSegment(ReadOnlyMemory<char>.Empty, colIndex)
                    : throw new KeyNotFoundException($"Parameter '{segment.Value}' not found in the parameter-to-column map."),
                ExcelFormulaSegmentType.TimePlaceholder => new ModelFunctionSegment(ReadOnlyMemory<char>.Empty, 0),
                _ => throw new InvalidOperationException("Unknown segment type."),
            };
        }

        return new(buffer[..this.SegmentCount], rowIndex);
    } // internal ModelFunctionTemplate BindParameters (int, ParamToColumnMap, Span<ModelFunctionSegment>)
} // internal sealed class ExcelFormulaTemplate

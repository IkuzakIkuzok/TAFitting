
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Specifies the type of segment within an Excel formula template, such as a literal value or a placeholder.
/// </summary>
internal enum TemplateSegmentType
{
    /// <summary>
    /// Lieteral text segment.
    /// </summary>
    Literal,

    /// <summary>
    /// Placeholder for a parameter name.
    /// </summary>
    ParameterPlaceholder,

    /// <summary>
    /// Placeholder for time variable.
    /// </summary>
    TimePlaceholder
} // internal enum TemplateSegmentType

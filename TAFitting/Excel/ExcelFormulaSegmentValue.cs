
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Represents a segment of an Excel formula, which may be either a column index or a literal text value.
/// </summary>
internal readonly struct ExcelFormulaSegmentValue
{
    private readonly int _columnIndex;

    private readonly ReadOnlyMemory<char> _literalText;

    /// <summary>
    /// Gets the index of the column associated with this instance.
    /// </summary>
    internal int ColumnIndex => this._columnIndex;

    /// <summary>
    /// Gets the literal text associated with this instance.
    /// </summary>
    internal ReadOnlyMemory<char> LiteralText => this._literalText;

    private ExcelFormulaSegmentValue(int columnIndex)
    {
        this._columnIndex = columnIndex;
        this._literalText = ReadOnlyMemory<char>.Empty;
    } // ctor (int)

    private ExcelFormulaSegmentValue(ReadOnlyMemory<char> literalText)
    {
        this._literalText = literalText;
        this._columnIndex = -1;
    } // ctor (ReadOnlyMemory<char>)

    public static implicit operator ExcelFormulaSegmentValue(int columnIndex) => new(columnIndex);

    public static implicit operator ExcelFormulaSegmentValue(ReadOnlyMemory<char> literalText) => new(literalText);
} // internal struct ExcelFormulaSegmentValue

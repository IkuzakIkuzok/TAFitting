
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Wraps a worksheet.
/// </summary>
internal class Worksheet
{
    private readonly dynamic worksheet;
    private readonly Columns columns;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    internal string Name
    {
        get => this.worksheet.Name;
        set => this.worksheet.Name = value;
    }

    /// <summary>
    /// Gets the number of columns.
    /// </summary>
    internal int ColumnsCount => this.worksheet.Cols;

    /// <summary>
    /// Gets the number of rows.
    /// </summary>
    internal int RowsCount => this.worksheet.Rows;

    /// <summary>
    /// Gets the columns.
    /// </summary>
    internal Columns Columns => this.columns;

    /// <summary>
    /// Gets the data range.
    /// </summary>
    /// <value>The data range for whole worksheet.</value>
    internal DataRange DataRange => new(this.worksheet.NewDataRange(0, 0, -1, -1));

    /// <summary>
    /// Initializes a new instance of the <see cref="Worksheet"/> class.
    /// </summary>
    /// <param name="worksheet">The worksheet.</param>
    internal Worksheet(dynamic worksheet)
    {
        this.worksheet = worksheet;
        this.columns = new(this.worksheet.Columns);
    } // ctor (dynamic)
} // internal class Worksheet

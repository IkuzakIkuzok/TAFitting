
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// A column in a <see cref="Worksheet"/>.
/// </summary>
internal class Column
{
    private readonly dynamic column;

    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    internal string Name
    {
        get => this.column.Name;
        set => this.column.Name = value;
    }

    /// <summary>
    /// Gets or sets the long name of the column.
    /// </summary>
    internal string LongName
    {
        get => this.column.LongName;
        set => this.column.LongName = value;
    }

    /// <summary>
    /// Gets or sets the units of the column.
    /// </summary>
    internal string Units
    {
        get => this.column.Units;
        set => this.column.Units = value;
    }

    /// <summary>
    /// Gets or sets the comments of the column.
    /// </summary>
    internal string Comments
    {
        get => this.column.Comments;
        set => this.column.Comments = value;
    }

    /// <summary>
    /// Gets the index of the column.
    /// </summary>
    internal int Index => this.column.Index;

    /// <summary>
    /// Gets the number of rows in the column.
    /// </summary>
    internal int RowsCount => this.column.Rows;

    /// <summary>
    /// Gets or sets the data of the column.
    /// </summary>
    /// <param name="index">The index of the row.</param>
    /// <returns>The data of the row specified by the index.</returns>
    internal object this[int index]
    {
        get => this.column.GetData(ArrayDataFormat.Array1DVariant, index, index);
        set => this.column.SetData(new[] { value }, index);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Column"/> class with the specified column.
    /// </summary>
    /// <param name="column">The Origin column object.</param>
    internal Column(dynamic column)
    {
        this.column = column;
    } // ctor (dynamic)

    /// <summary>
    /// Sets the data of the column.
    /// </summary>
    /// <param name="data">The data to set.</param>
    internal void SetData(object[] data)
    {
        this.column.SetData(data, 0);
    } // internal void SetData (object[])
} // internal class Column

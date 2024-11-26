
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Origin;

/// <summary>
/// A collection of <see cref="Column"/> objects.
/// </summary>
internal partial class Columns : IReadOnlyList<Column>
{
    private readonly dynamic _columns;
    private readonly List<Column> columns = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Columns"/> class with the specified columns.
    /// </summary>
    /// <param name="columns">The Origin columns object.</param>
    internal Columns(dynamic columns)
    {
        this._columns = columns;
        for (var i = 0; i < columns.Count; i++)
            this.columns.Add(new(columns[i]));
    } // ctor (dynamic)

    /// <summary>
    /// Adds a new column to the collection.
    /// </summary>
    /// <returns>The new column.</returns>
    internal Column Add()
    {
        var col = new Column(this._columns.Add());
        this.columns.Add(col);
        return col;
    } // internal Column Add ()

    public Column this[int index] => this.columns[index];

    public int Count => this.columns.Count;

    public IEnumerator<Column> GetEnumerator()
        => this.columns.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.columns.GetEnumerator();
} // internal partial class Columns : IReadOnlyList<Column>

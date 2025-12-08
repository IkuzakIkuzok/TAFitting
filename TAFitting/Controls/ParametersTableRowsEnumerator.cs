
// (c) 2025 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Controls;

/// <summary>
/// Enumerates the rows in a DataGridViewRowCollection that are of type ParametersTableRow.
/// </summary>
internal struct ParametersTableRowsEnumerator : IEnumerator<ParametersTableRow>, IEnumerator
{
    private readonly IList _rows;
    private int _index;
    private ParametersTableRow? _current;
    private readonly Func<ParametersTableRow, bool>? predicate;

    public readonly ParametersTableRow Current => this._current!;

    readonly object IEnumerator.Current => this.Current;

    /// <summary>
    /// Initializes a new instance of the ParametersTableRowsEnumerator class that iterates through the specified collection of DataGridView rows.
    /// </summary>
    /// <param name="rows">The collection of DataGridViewRow objects to enumerate.</param>
    /// <param name="predicate">An optional predicate to filter the rows.</param>
    internal ParametersTableRowsEnumerator(IList rows, Func<ParametersTableRow, bool>? predicate)
    {
        this._rows = rows;
        this._index = -1;
        this._current = null;
        this.predicate = predicate;
    } // ctor (DataGridViewRowCollection, Func<ParametersTableRow, bool>?)

    public bool MoveNext()
    {
        for (this._index += 1; this._index < this._rows.Count; this._index++)
        {
            var row = this._rows[this._index];
            if (row is ParametersTableRow pRow)
            {
                if (!(this.predicate?.Invoke(pRow) ?? true)) continue;
                this._current = pRow;
                return true;
            }
        }
        this._current = null;
        return false;
    } // public bool MoveNext ()

    public void Reset()
    {
        this._index = -1;
        this._current = null;
    } // public void Reset ()

    public readonly void Dispose() { }
} // internal struct ParametersTableRowsEnumerator : IEnumerator<ParametersTableRow>, IEnumerator

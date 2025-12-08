
//(c) 2025 Kazuki KOHZUKI

using System.Collections;
using System.Runtime.CompilerServices;

namespace TAFitting.Controls;

/// <summary>
/// Provides an enumerable collection of <see cref="ParametersTableRow"/> items, optionally filtered by a predicate.
/// </summary>
[CollectionBuilder(typeof(ParametersTableRowsEnumerable), nameof(Create))]
internal readonly struct ParametersTableRowsEnumerable : IEnumerable<ParametersTableRow>
{
    private readonly IList _rows;
    private readonly Func<ParametersTableRow, bool>? _predicate;

    /// <summary>
    /// Initializes a new instance of the ParametersTableRowsEnumerable class with the specified collection of rows.
    /// </summary>
    /// <param name="rows">The collection of rows to be enumerated.</param>
    internal ParametersTableRowsEnumerable(IList rows)
    {
        this._rows = rows;
    } // ctor (IList)

    /// <summary>
    /// Initializes a new instance of the ParametersTableRowsEnumerable class
    /// with the specified collection of rows and a predicate to filter them.
    /// </summary>
    /// <param name="rows">The collection of rows to be enumerated.</param>
    /// <param name="predicate">A function that determines whether a given ParametersTableRow should be included in the enumeration.</param>
    internal ParametersTableRowsEnumerable(IList rows, Func<ParametersTableRow, bool> predicate)
    {
        this._rows = rows;
        this._predicate = predicate;
    } // ctor (IList, Func<ParametersTableRow, bool>)

    internal static ParametersTableRowsEnumerable Create(ReadOnlySpan<ParametersTableRow> rows)
        => new(rows.ToArray());

    public readonly ParametersTableRowsEnumerator GetEnumerator()
        => new(this._rows, this._predicate);
   
    readonly IEnumerator<ParametersTableRow> IEnumerable<ParametersTableRow>.GetEnumerator()
        => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
} // internal struct ParametersTableRowsEnumerable

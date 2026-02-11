
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Provides a mapping between parameter names and their corresponding column indexes for internal use.
/// This class is NOT thread-safe and is intended for use within a single thread context.
/// </summary>
internal sealed class ParamToColumnMap
{
    private record struct Entry(ReadOnlyMemory<char> ParamName, int ColumnIndex);

    private readonly Entry[] _entries;
    private int _count = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParamToColumnMap"/> class with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The number of entries that the map can initially hold.</param>
    internal ParamToColumnMap(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity, nameof(capacity));

        this._entries = new Entry[capacity];
    } // ctor (int)

    /// <summary>
    /// Adds a parameter name and its associated column index to the map.
    /// </summary>
    /// <param name="name">The parameter name to associate with the specified column index.</param>
    /// <param name="columnIndex">The index of the column to map to the parameter name.</param>
    /// <exception cref="InvalidOperationException">Thrown if the map has reached its capacity and cannot accept additional entries.</exception>
    /// <exception cref="ArgumentValidation">Thrown if the <paramref name="name"/> is empty.</exception>
    internal void Add(ReadOnlyMemory<char> name, int columnIndex)
    {
        if (this._count >= this._entries.Length)
            throw new InvalidOperationException("The ParamToColumnMap has reached its capacity.");
        if (name.IsEmpty)
            throw new ArgumentException("Parameter name cannot be empty.");

        this._entries[this._count++] = new(name, columnIndex);
    } // internal void Add (ReadOnlyMemory<char>, int)

    /// <summary>
    /// Attempts to retrieve the column index associated with the specified parameter name.
    /// </summary>
    /// <param name="name">The parameter name to search for.</param>
    /// <param name="columnIndex">When this method returns, contains the column index associated with the specified parameter name if found;
    /// otherwise, contains -1.</param>
    /// <returns><see langword="true"/> if the <paramref name="name"/> was found and <paramref name="columnIndex"/> contains the associated index; otherwise, <see langword="false"/> .</returns>
    internal bool TryGetValue(ReadOnlyMemory<char> name, out int columnIndex)
    {
        var nameSpan = name.Span;

        for (var i = 0; i < this._count; i++)
        {
            ref readonly var entry = ref this._entries[i];
            var entrySpan = entry.ParamName.Span;
            if (entrySpan.SequenceEqual(nameSpan))
            {
                columnIndex = entry.ColumnIndex;
                return true;
            }
        }

        columnIndex = -1;
        return false;
    } // internal bool TryGetValue (ReadOnlyMemory<char>, out int)
} // internal sealed class ParamToColumnMap

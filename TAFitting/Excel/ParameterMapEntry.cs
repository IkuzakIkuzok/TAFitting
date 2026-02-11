
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Represents a parameter entry with its name and positional index within a collection or sequence.
/// </summary>
/// <param name="name">The name of the parameter, provided as a read-only memory region of characters.</param>
/// <param name="columnIndex">The column index of the parameter.</param>
internal readonly struct ParameterMapEntry(ReadOnlyMemory<char> name, int columnIndex)
{
    /// <summary>
    /// Gets the name represented as a read-only sequence of characters.
    /// </summary>
    internal ReadOnlyMemory<char> Name { get; } = name;

    /// <summary>
    /// Gets the index of the column associated with this instance.
    /// </summary>
    internal int ColumnIndex { get; } = columnIndex;

    /// <summary>
    /// Determines whether the current name matches the specified name using an ordinal, case-sensitive comparison.
    /// </summary>
    /// <param name="otherName">The name to compare with the current name, represented as a read-only memory region of characters.</param>
    /// <returns><see langword="true"/> if the specified name matches the current name; otherwise, <see langword="false"/>.</returns>
    internal bool Matches(ReadOnlyMemory<char> otherName)
        => this.Name.Span.SequenceEqual(otherName.Span);
} // internal readonly struct ParameterMapEntry (ReadOnlyMemory<char>, int)

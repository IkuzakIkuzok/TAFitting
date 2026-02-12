
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Excel;

/// <summary>
/// Represents a parameter entry with its name and positional index within a collection or sequence.
/// </summary>
/// <param name="name">The name of the parameter.</param>
/// <param name="columnIndex">The column index of the parameter.</param>
internal readonly struct ParameterMapEntry(string name, int columnIndex)
{
    /// <summary>
    /// Gets the name represented as a read-only sequence of characters.
    /// </summary>
    internal string Name { get; } = name;

    /// <summary>
    /// Gets the index of the column associated with this instance.
    /// </summary>
    internal int ColumnIndex { get; } = columnIndex;

    /// <summary>
    /// Determines whether the current name matches the specified name using an ordinal, case-sensitive comparison.
    /// </summary>
    /// <param name="otherName">The name to compare with the current name.</param>
    /// <returns><see langword="true"/> if the specified name matches the current name; otherwise, <see langword="false"/>.</returns>
    internal bool Matches(ReadOnlySpan<char> otherName)
        => this.Name.AsSpan().SequenceEqual(otherName);
} // internal readonly struct ParameterMapEntry (ReadOnlyMemory<char>, int)

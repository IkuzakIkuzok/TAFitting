
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Excel;

/// <summary>
/// Represents a segment of a model function, which may be either a literal text fragment or a placeholder for an argument.
/// </summary>
/// <param name="Text">The text content of the segment. If <paramref name="Text"/> is empty, the segment represents an argument placeholder.</param>
/// <param name="ArgIndex">The zero-based index of the argument associated with the segment. Ignored if <paramref name="Text"/> is not empty.</param>
internal readonly record struct ModelFunctionSegment(ReadOnlyMemory<char> Text, int ArgIndex)
{
    /// <summary>
    /// Gets a value indicating whether the current instance represents a literal value.
    /// </summary>
    internal bool IsLiteral => !this.Text.IsEmpty;

    /// <summary>
    /// Calculates the maximum possible length of the formatted address or placeholder represented by this instance.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetMaxLength()
    {
        if (this.IsLiteral) return this.Text.Length;
        if (this.ArgIndex == 0)
        {
            // Time placeholder "$X"
            // Possible max address is "XFD$1" (row index is always 1 for time)
            // 3 for column letters + 1 for '$' + 1 for row number
            return 5;
        }

        // Parameter placeholder
        // Possible max address is "$XFD1048576"
        // 3 for column letters + 1 for '$' + 7 for row number
        return 11;
    } // internal int GetMaxLength ()
} // internal readonly ref struct ModelFunctionSegment (string?, int)

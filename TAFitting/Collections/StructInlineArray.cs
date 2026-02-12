
// (c) 2026 Kazuki KOHZUKI

using System.Runtime.CompilerServices;

namespace TAFitting.Collections;

/// <summary>
/// Represents a fixed-size, inline array of value type elements.
/// </summary>
/// <typeparam name="T">The value type of elements stored in the array.</typeparam>
[InlineArray(Capacity)]
internal struct StructInlineArray<T> where T : struct
{
    internal const int Capacity = 32;

    private T _value0;

    /// <summary>
    /// Returns a span that contains the first specified number of elements of the current sequence.
    /// </summary>
    /// <param name="length">The number of elements to include in the returned span. Must be non-negative and less than or equal to the length of the current sequence.</param>
    /// <returns>A span containing the first specified number of elements from the current sequence.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<T> AsSpan(int length)
        => MemoryMarshal.CreateSpan(ref this._value0, length);
} // internal struct StructInlineArray<T> where T : struct

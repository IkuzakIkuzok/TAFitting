
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
} // internal struct StructInlineArray<T> where T : struct

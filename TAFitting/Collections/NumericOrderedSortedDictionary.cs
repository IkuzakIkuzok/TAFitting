
// (c) 2025 Kazuki KOHZUKI

using System.Numerics;

namespace TAFitting.Collections;

/// <summary>
/// Represents an ordered, sorted dictionary whose keys are numeric types supporting arithmetic and comparison operations.
/// </summary>
/// <remarks>This class extends <see cref="OrderedSortedDictionary{TKey, TValue}"/> to provide additional functionality for numeric key types,
/// such as finding the nearest key to a given value. Keys are maintained in sorted
/// order, enabling efficient numeric lookups and range queries.</remarks>
/// <typeparam name="TKey">The type of the keys in the dictionary. Must implement <see cref="INumber{TKey}"/> to support numeric operations and ordering.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
internal class NumericOrderedSortedDictionary<TKey, TValue> : OrderedSortedDictionary<TKey, TValue> where TKey : INumber<TKey>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NumericOrderedSortedDictionary{TKey, TValue}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the dictionary.</param>
    internal NumericOrderedSortedDictionary(int capacity) : base(capacity) { }

    /// <summary>
    /// Finds the key in the collection that is closest to the specified value.
    /// If the exact key exists, returns it; otherwise, returns the nearest key by value.
    /// </summary>
    /// <param name="value">The value to compare against the keys in the collection. Must be within the range of the key type supported by
    /// the collection.</param>
    /// <returns>The key that is equal to or nearest to the specified value. If multiple keys are equally close, returns the lower key.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the collection contains no keys.</exception>
    internal TKey FindNearestKey(TKey value)
    {
        if (this.keys.Count == 0)
            throw new InvalidOperationException("The dictionary is empty.");

        if (this.keys.Count == 1) return this.keys[0];
        if (value <= this.keys[0]) return this.keys[0];
        if (value >= this.keys[^1]) return this.keys[^1];

        var index = GetKeyIndex(value);
        if (index >= 0)
            // Exact match found; no need to get nearest through indexing
            return value;

        var i_next = ~index;
        var i_prev = i_next - 1;

        var key_next = this.keys[i_next];
        var key_prev = this.keys[i_prev];

        var diff_next = TKey.Abs(key_next - value);
        var diff_prev = TKey.Abs(value - key_prev);
        return diff_next < diff_prev ? key_next : key_prev;
    } // internal TKey FindNearestKey(TKey value)
} // internal class NumericOrderedSortedDictionary<TKey, TValue> : OrderedSortedDictionary<TKey, TValue> where TKey : INumber<TKey>

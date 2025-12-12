
// (c) 2025 Kazuki KOHZUKI

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TAFitting.Collections;

/// <summary>
/// Represents a generic dictionary that maintains its entries in strictly increasing key order, enforcing that each new key added is greater than all previously added keys.
/// </summary>
/// <remarks>This collection enforces that keys are added in strictly increasing order;
/// attempting to add a key that is less than or equal to the last key will result in an exception.
/// The dictionary provides efficient lookup and enumeration of key-value pairs in the order they were added.
/// It is not thread-safe and does not support concurrent modifications.</remarks>
/// <typeparam name="TKey">The type of keys in the dictionary. Must implement <see cref="IComparable{TKey}"/> to support ordering.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
/// <param name="capacity">The initial capacity of the dictionary.</param>
internal class OrderedSortedDictionary<TKey, TValue>(int capacity) : IDictionary<TKey, TValue> where TKey : IComparable<TKey>
{
    protected readonly List<TKey> keys = new(capacity);
    protected readonly List<TValue> values = new(capacity);

    /// <summary>
    /// Gets the number of entries contained in the collection.
    /// </summary>
    public int Count => this.keys.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get or set.</param>
    /// <returns>The value associated with the specified key.</returns>
    public TValue this[TKey key]
    {
        get
        {
            var index = GetKeyIndex(key);
            if (index < 0) ThrowKeyNotFoundException(key);
            return this.values[index];
        }
        set
        {
            var index = GetKeyIndex(key);
            if (index < 0)
                InsertAt(~index, key, value);
            else
                this.values[index] = value;
        }
    } // public TValue this[TKey]

    /// <summary>
    /// Gets the key/value pair at the specified index in the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve. Must be within the bounds of the collection.</param>
    /// <returns>A <see cref="KeyValuePair{TKey, TValue}"/> representing the key and value at the specified index.</returns>
    public KeyValuePair<TKey, TValue> this[int index]
    {
        get
        {
            IndexOutOfRangeException.ThorwIfIndexOutOfRange(index, this.keys.Count);
            return new(this.keys[index], this.values[index]);
        }
    } // public KeyValuePair<TKey, TValue> this[int]

    /// <inheritdoc/>
    /// <remarks>
    /// Use <see cref="GetKeyEnumerable"/> to get an enumerable collection of keys without allocations.
    /// </remarks>
    public ICollection<TKey> Keys => this.keys;

    /// <inheritdoc/>
    /// <remarks>
    /// Use <see cref="GetValueEnumerable"/> to get an enumerable collection of values without allocations.
    /// </remarks>
    public ICollection<TValue> Values => this.values;

    /// <summary>
    /// Adds a key-value pair to the collection, ensuring that the key is greater than all previously added keys.
    /// </summary>
    /// <remarks>This method enforces that keys are added in strictly increasing order.remarks>
    /// <param name="key">The key to add to the collection. Must be greater than the last key currently in the collection.</param>
    /// <param name="value">The value associated with the specified key.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is less than or equal to the last key in the collection.</exception>
    public void Add(TKey key, TValue value)
    {
        // new key must be greater than the last key
        if (this.keys.Count > 0)
        {
            var lastKey = this.keys[^1];
            if (key.CompareTo(lastKey) <= 0)
                throw new ArgumentException("The new key must be greater than the last key in the collection.", nameof(key));
        }

        this.keys.Add(key);
        this.values.Add(value);
    } // public void Add (TKey, TValue)

    /// <summary>
    /// Adds the specified key/value pair to the collection.
    /// </summary>
    /// <param name="item">The key/value pair to add to the collection. The key must not already exist in the collection.</param>
    public void Add(KeyValuePair<TKey, TValue> item)
        => Add(item.Key, item.Value);

    /// <summary>
    /// Adds the elements of the specified collection to the current dictionary in ascending key order.
    /// </summary>
    /// <remarks>Elements are added in order of ascending key, as determined by the default comparer for the key type.
    /// If any key in the source collection already exists in the dictionary, an exception will be thrown for that key and the operation will not be atomic.</remarks>
    /// <param name="source">The collection of key/value pairs to add to the dictionary. Each key must not already exist in the dictionary.</param>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        foreach (var (key, value) in source.OrderBy(kv => kv.Key))
            Add(key, value);
    } // public void AddRange (IEnumerable<KeyValuePair<TKey, TValue>>)

    /// <summary>
    /// Inserts a key and value at the specified index within the collection.
    /// </summary>
    /// <param name="index">The zero-based index at which the key and value should be inserted.</param>
    /// <param name="key">The key to insert at the specified index.</param>
    /// <param name="value">The value to insert at the specified index.</param>
    protected void InsertAt(int index, TKey key, TValue value)
    {
        this.keys.Insert(index, key);
        this.values.Insert(index, value);
    } // protected void InsertAt (int, TKey, TValue)

    public bool Remove(TKey key)
    {
        var index = GetKeyIndex(key);
        if (index < 0) return false;

        this.keys.RemoveAt(index);
        this.values.RemoveAt(index);
        return true;
    } // public bool Remove (TKey)

    /// <summary>
    /// Removes the specified key and value pair from the collection if it exists.
    /// </summary>
    /// <param name="item">The key and value pair to remove. The pair is removed only if both the key and value match an entry in the collection.</param>
    /// <returns><see cref="true"/> if the specified key and value pair was found and removed; otherwise, <see cref="false"/>.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        var index = GetKeyIndex(item.Key);
        if (index < 0) return false;
        if (!EqualityComparer<TValue>.Default.Equals(this.values[index], item.Value))
            return false;

        this.keys.RemoveAt(index);
        this.values.RemoveAt(index);
        return true;
    } // public bool Remove (KeyValuePair<TKey, TValue>)

    public void Clear()
    {
        this.keys.Clear();
        this.values.Clear();
    } // public void Clear ()

    /// <summary>
    /// Copies the elements of the collection to a specified array of key/value pairs, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional array of key/value pairs that is the destination of the elements copied from the collection.</param>
    /// <param name="arrayIndex">The zero-based index in the destination array at which copying begins.</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        ArgumentOutOfRangeException.ThrowIfNegative(arrayIndex, nameof(arrayIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(arrayIndex, array.Length, nameof(arrayIndex));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(this.Count, array.Length - arrayIndex,
            "The number of elements in the source collection is greater than the available space from arrayIndex to the end of the destination array.");

        for (var i = 0; i < this.Count; i++)
            array[arrayIndex + i] = new(this.keys[i], this.values[i]);
    } // public void CopyTo (KeyValuePair<TKey, TValue>[], int)

    /// <summary>
    /// Determines whether the dictionary contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary. Cannot be null.</param>
    /// <returns><see cref="true"/> if the dictionary contains an element with the specified key; otherwise, <see cref="false"/>.</returns>
    public bool ContainsKey(TKey key)
        => GetKeyIndex(key) >= 0;

    /// <summary>
    /// Determines whether the dictionary contains the specified key and value pair.
    /// </summary>
    /// <remarks>The value comparison uses the default equality comparer for the value type.
    /// This method checks both the presence of the key and that its associated value equals the specified value.</remarks>
    /// <param name="item">The key and value pair to locate in the dictionary.
    /// The key is used to find the entry, and the value is compared for equality with the stored value.</param>
    /// <returns><see cref="true"/> if the dictionary contains an element with the specified key and value; otherwise, <see cref="false"/>.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        var index = GetKeyIndex(item.Key);
        if (index < 0) return false;
        return EqualityComparer<TValue>.Default.Equals(this.values[index], item.Value);
    } // public bool Contains (KeyValuePair<TKey, TValue>)

    /// <summary>
    /// Attempts to retrieve the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose associated value is to be retrieved.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found;
    /// otherwise, the default value for the type of the value parameter.</param>
    /// <returns><see langword="true"/> if the key was found and its value was retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var index = GetKeyIndex(key);
        if (index < 0)
        {
            value = default;
            return false;
        }

        value = this.values[index];
        return true;
    } // public bool TryGetValue (TKey, out TValue)

    /// <summary>
    /// Returns an enumerator that iterates through the collection of key-value pairs.
    /// </summary>
    /// <returns>An <see cref="Enumerator"/> that can be used to iterate through the keys and values in the collection.</returns>
    public Enumerator GetEnumerator()
        => new(this.keys, this.values);

    /// <summary>
    /// Returns an enumerator that iterates through the collection of key/value pairs.
    /// </summary>
    /// <returns>An enumerator for the collection of <see cref="KeyValuePair{TKey, TValue}"/> elements.</returns>
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        => this.Count == 0 ? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator() : GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => this.Count == 0 ? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator() : GetEnumerator();

    /// <summary>
    /// Returns an enumerable collection that iterates over all keys in the current instance.
    /// </summary>
    /// <returns>An <see cref="EnumerableStruct{TKey}"/> that provides enumeration of the keys contained in this instance.</returns>
    public EnumerableStruct<TKey> GetKeyEnumerable()
        => new(this.keys);

    /// <summary>
    /// Returns an enumerable struct that provides a value-based iteration over the collection.
    /// </summary>
    /// <returns>An <see cref="EnumerableStruct{TValue}"/> that can be used to enumerate the values in the collection.</returns>
    public EnumerableStruct<TValue> GetValueEnumerable()
        => new(this.values);

    /// <summary>
    /// Searches for the specified key and returns the zero-based index within the sorted key collection.
    /// </summary>
    /// <param name="key">The key to locate in the collection.</param>
    /// <returns>The zero-based index of the key if found;
    /// otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than the key,
    /// or, if there is no larger element, the bitwise complement of the collection's count.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetKeyIndex(TKey key)
        => this.keys.BinarySearch(key);

    /// <summary>
    /// Attempts to get the key that precedes the specified key in the collection.
    /// </summary>
    /// <param name="key">The key for which to find the preceding key in the collection.</param>
    /// <param name="previousKey">When this method returns, contains the previous key if found; otherwise, the default value for <typeparamref name="TKey"/>.</param>
    /// <returns><see langword="true"/> if a previous key was found; otherwise, <see langword="false"/>.</returns>
    internal bool TryGetPreviousKey(TKey key, [MaybeNullWhen(false)] out TKey previousKey)
    {
        var index = GetKeyIndex(key);
        if (index > 0)
        {
            previousKey = this.keys[index - 1];
            return true;
        }
        else if (index < 0)
        {
            var insertIndex = ~index;
            if (insertIndex > 0)
            {
                previousKey = this.keys[insertIndex - 1];
                return true;
            }
        }

        previousKey = default;
        return false;
    } // internal bool TryGetPreviousKey (TKey, out TKey)

    /// <summary>
    /// Attempts to retrieve the key that immediately follows the specified key in the collection.
    /// </summary>
    /// <param name="key">The key for which to find the next sequential key in the collection.</param>
    /// <param name="nextKey">When this method returns, contains the next key in sequence if found; otherwise, the default value for <typeparamref name="TKey"/>.</param>
    /// <returns><see langword="true"/> if a next key exists and was returned in <paramref name="nextKey"/>; otherwise, <see langword="false"/>.</returns>
    internal bool TryGetNextKey(TKey key, [MaybeNullWhen(false)] out TKey nextKey)
    {
        var index = GetKeyIndex(key);
        if (index >= 0)
        {
            if (index + 1 < this.keys.Count)
            {
                nextKey = this.keys[index + 1];
                return true;
            }
        }
        else
        {
            var insertIndex = ~index;
            if (insertIndex < this.keys.Count)
            {
                nextKey = this.keys[insertIndex];
                return true;
            }
        }

        nextKey = default;
        return false;
    } // internal bool TryGetNextKey (TKey, out TKey)

    [DoesNotReturn]
    private static void ThrowKeyNotFoundException(TKey key)
    {
        throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
    } // private static void ThrowKeyNotFoundException (TKey)

    /// <summary>
    /// Enumerates the key/value pairs of a collection represented by separate key and value lists.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
    {
        private readonly List<TKey> keys;
        private readonly List<TValue> values;
        private int index;

        public readonly KeyValuePair<TKey, TValue> Current
        {
            get
            {
                if ((uint)this.index >= (uint)this.keys.Count)
                    throw new InvalidOperationException("The enumerator is positioned before the first element or after the last element of the collection.");

                return new(this.keys[this.index], this.values[this.index]);
            }
        } // public KeyValuePair<TKey, TValue> Current

        readonly object IEnumerator.Current => this.Current;

        /// <summary>
        /// Initializes a new instance of the Enumerator class that iterates over the specified lists of keys and values.
        /// </summary>
        /// <param name="keys">The list of keys to be enumerated.</param>
        /// <param name="values">The list of values to be enumerated.</param>
        /// <exception cref="ArgumentException">Thrown when the number of keys does not match the number of values.</exception>
        internal Enumerator(List<TKey> keys, List<TValue> values)
        {
            if (keys.Count != values.Count)
                throw new ArgumentException("The number of keys must match the number of values.", nameof(keys));

            this.keys = keys;
            this.values = values;
            this.index = -1;
        } // internal Enumerator (List<TKey>, List<TValue>)

        public bool MoveNext()
        {
            this.index++;
            return (uint)this.index < (uint)this.keys.Count;
        } // public bool MoveNext ()

        public void Reset()
            => this.index = -1;

        public readonly void Dispose() { }
    } // public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator

    /// <summary>
    /// Provides a value type wrapper around a list, enabling enumeration of its elements using standard .NET collection interfaces.
    /// </summary>   
    /// <typeparam name="T">The type of elements contained in the collection.</typeparam>
    public readonly struct EnumerableStruct<T> : IEnumerable<T>
    {
        private readonly List<T> list;

        /// <summary>
        /// Initializes a new instance of the EnumerableStruct<T> struct using the specified list as the underlying
        /// collection.
        /// </summary>
        /// <param name="list">The list of elements to be wrapped by the struct.</param>
        internal EnumerableStruct(List<T> list)
            => this.list = list;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A <see cref="List{T}.Enumerator"/> for the collection.</returns>
        /// <remarks>This method provides a value-type enumerator to avoid heap allocations during iteration.</remarks>
        public readonly List<T>.Enumerator GetEnumerator()
            => this.list.GetEnumerator();

        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => this.list.GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator()
            => this.list.GetEnumerator();
    } // public struct EnumerableStruct<T> : IEnumerable<T>
} // internal class OrderedSortedDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : IComparable<TKey>

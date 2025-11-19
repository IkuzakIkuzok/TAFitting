
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting;

/// <summary>
/// Provides extension methods for working with <see cref="Dictionary{TKey, TValue}"/>.
/// </summary>
internal static class DictionaryExtension
{
    extension<TKey, TValue> (Dictionary<TKey, TValue> dict) where TKey : notnull
    {
        /// <summary>
        /// Gets the value associated with the specified key, or adds a new value created by the specified factory function if the key does not exist.
        /// </summary>
        /// <param name="key">The key whose value to retrieve or add.</param>
        /// <param name="valueFactory">A function used to generate a value for the specified key if it does not exist in the collection.
        /// The function is invoked with the key as its argument.</param>
        /// <returns>The value associated with the specified key.
        /// If the key does not exist, the value returned by the factory function is added and returned.</returns>
        internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dict[key] = value;
            }
            return value;
        } // internal TValue GetOrAdd (TKey, Func<TKey, TValue>)
    } // extension<TKey, TValue> (Dictionary<TKey, TValue>)
} // internal static class DictionaryExtension

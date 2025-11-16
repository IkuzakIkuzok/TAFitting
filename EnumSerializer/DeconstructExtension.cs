
// (c) 2025 Kazuki Kohzuki

namespace EnumSerializer;

internal static class DeconstructExtension
{
    internal static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    } // internal static void DeconstructExtension<TKey, TValue> (this KeyValuePair<TKey, TValue>, out TKey, out TValue)
} // internal static class DeconstructExtension

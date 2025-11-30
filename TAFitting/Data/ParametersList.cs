
// (c) 2025 Kazuki Kohzuki

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TAFitting.Data;

/// <summary>
/// Represents a collection that maps double-precision keys to read-only lists of double values,
/// providing dictionary-like access and management of parameter sets.
/// </summary>
internal sealed class ParametersList : IDictionary<double, IReadOnlyList<double>>, IReadOnlyDictionary<double, IReadOnlyList<double>>
{
    private readonly Dictionary<double, IReadOnlyList<double>> parametersDict = [];

    /// <summary>
    /// Gets the current state token to detect changes in the parameters.
    /// </summary>
    /// <remarks>
    /// The token is intended to be used for change detection.
    /// Different states should produce different tokens,
    /// but different tokens do not necessarily imply different states (false positives are acceptable).
    /// The implementation may be hash-based or versioning-based, or any other suitable method.
    /// </remarks>
    // NOTE: Change implementation to versioning if changed can be detected easily.
    internal long CurrentStateToken => ComputeDeepHash();

    /// <summary>
    /// Calculates a hash code that represents the current state of all parameters in the dictionary, including their keys and associated values.
    /// </summary>
    /// <remarks>This method performs a deep hash computation by combining the hash codes of each key and its corresponding value list.
    /// The resulting hash reflects changes to any key or value within the dictionary, making it suitable for scenarios where a comprehensive state comparison is required.</remarks>
    /// <returns>A 64-bit integer hash code that uniquely identifies the contents of the parameters dictionary.</returns>
    private long ComputeDeepHash()
    {
        var hash = 0L;
        foreach (var kvp in this.parametersDict)
        {
            var keyHash = kvp.Key.GetHashCode();
            var listHash = ComputeListHash(kvp.Value);
            // 397 is a prime number used to reduce hash collisions.
            var entryHash = (keyHash * 397) ^ listHash;
            hash ^= entryHash;
        }
        return hash;
    } // private long ComputeDeepHash ()

    /// <summary>
    /// Computes a hash code for the contents of the specified list of double-precision floating-point values.
    /// </summary>
    /// <remarks>The hash code is based on the binary representation of the double values in the list.
    /// For best performance, use a <see cref="List{double}"/> or <see cref="double[]"/> as the input, as these types provide efficient access to the underlying data.</remarks>
    /// <param name="list">The list of double values to compute the hash code for. If the list is <see langword="null"/> or empty, the method returns 0.</param>
    /// <returns>A 64-bit integer representing the hash code of the list contents. Returns 0 if the list is <see langword="null"/> or empty.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long ComputeListHash(IReadOnlyList<double> list)
    {
        if (list is null || list.Count == 0) return 0;

        // `list` is expected to be List<double> or double[], which provide Span<T>.
        // Other types fall back to cold path.
        if (!list.TryGetSpan(out var span))
            return ComputeListHashLoop(list);

        var byteSpan = MemoryMarshal.AsBytes(span);
        var hash = new HashCode();
        hash.AddBytes(byteSpan);
        return hash.ToHashCode();
    } // private static long ComputeListHash (IReadOnlyList<double> list)

    // Do not inline cold path to keep hot path small, improving overall performance.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long ComputeListHashLoop(IReadOnlyList<double> list)
    {
        var hash = new HashCode();
        foreach (var item in list)
            hash.Add(item);
        return hash.ToHashCode();
    } // private static long ComputeListHashLoop (IReadOnlyList<double> list)

    #region IDictionary<double, IReadOnlyList<double>>

    public IReadOnlyList<double> this[double key]
    {
        get => this.parametersDict[key];
        set => this.parametersDict[key] = value;
    }

    public ICollection<double> Keys
        => this.parametersDict.Keys;

    IEnumerable<double> IReadOnlyDictionary<double, IReadOnlyList<double>>.Keys
        => this.parametersDict.Keys;

    public ICollection<IReadOnlyList<double>> Values
        => this.parametersDict.Values;

    IEnumerable<IReadOnlyList<double>> IReadOnlyDictionary<double, IReadOnlyList<double>>.Values
        => this.parametersDict.Values;

    public int Count
        => this.parametersDict.Count;

    public bool IsReadOnly
        => ((ICollection<KeyValuePair<double, IReadOnlyList<double>>>)this.parametersDict).IsReadOnly;

    public void Add(double key, IReadOnlyList<double> value)
        => this.parametersDict.Add(key, value);

    public void Add(KeyValuePair<double, IReadOnlyList<double>> item)
        => ((ICollection<KeyValuePair<double, IReadOnlyList<double>>>)this.parametersDict).Add(item);

    public void Clear()
        => this.parametersDict.Clear();

    public bool Contains(KeyValuePair<double, IReadOnlyList<double>> item)
        => ((ICollection<KeyValuePair<double, IReadOnlyList<double>>>)this.parametersDict).Contains(item);

    public bool ContainsKey(double key)
        => this.parametersDict.ContainsKey(key);

    public void CopyTo(KeyValuePair<double, IReadOnlyList<double>>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<double, IReadOnlyList<double>>>)this.parametersDict).CopyTo(array, arrayIndex);

    public IEnumerator<KeyValuePair<double, IReadOnlyList<double>>> GetEnumerator()
        => this.parametersDict.GetEnumerator();

    public bool Remove(double key)
        => this.parametersDict.Remove(key);

    public bool Remove(KeyValuePair<double, IReadOnlyList<double>> item)
        => ((ICollection<KeyValuePair<double, IReadOnlyList<double>>>)this.parametersDict).Remove(item);

    public bool TryGetValue(double key, [MaybeNullWhen(false)] out IReadOnlyList<double> value)
        => this.parametersDict.TryGetValue(key, out value);

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)this.parametersDict).GetEnumerator();

    #endregion IDictionary<double, IReadOnlyList<double>>
} // internal sealed class ParametersList : IDictionary<double, IReadOnlyList<double>>

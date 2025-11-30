
// (c) 2025 Kazuki Kohzuki

using System.Collections;
using System.Diagnostics.CodeAnalysis;

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
    // NOTE: Change implementation to versioning if changed can be detected easily.
    internal long CurrentStateToken => ComputeDeepHash();

    private long ComputeDeepHash()
    {
        var hash = 0L;
        foreach (var kvp in this.parametersDict)
        {
            var keyHash = kvp.Key.GetHashCode();
            var listHash = ComputeListHash(kvp.Value);
            var entryHash = (keyHash * 397) ^ listHash;
            hash ^= entryHash;
        }
        return hash;
    } // private long ComputeDeepHash ()

    private static long ComputeListHash(IReadOnlyList<double> list)
    {
        if (list == null || list.Count == 0) return 0;

        if (list.TryGetSpan(out var span))
        {
            var byteSpan = MemoryMarshal.AsBytes(span);
            var hash = new HashCode();
            hash.AddBytes(byteSpan);
            return hash.ToHashCode();
        }

        else
        {
            var hash = new HashCode();
            foreach (var item in list)
                hash.Add(item);
            return hash.ToHashCode();
        }
    } // private static long ComputeListHash (IReadOnlyList<double> list)

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

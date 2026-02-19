
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting.Collections;

/// <summary>
/// Provides extension methods for the <see cref="List{T}"/> class.
/// </summary>
internal static class ListExtension
{
    /// <summary>
    /// Adds the specified number of default-initialized elements to the end of the list.
    /// </summary>
    /// <typeparam name="T">The type of elements contained in the list.</typeparam>
    /// <param name="list">The list to which default elements will be added.</param>
    /// <param name="count">The number of default elements to add.</param>
    internal static void AddDefaults<T>(this List<T> list, int count)
    {
        if (count <= 0) return;

        var oldCount = list.Count;
        list.EnsureCapacity(oldCount + count);
        CollectionsMarshal.SetCount(list, oldCount + count);
        CollectionsMarshal.AsSpan(list).Slice(oldCount, count).Clear();
    } // internal static void AddDefaults<T> (this List<T>, int)

    /// <summary>
    /// Adds the specified item to the end of the list multiple times.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to which the item will be added repeatedly.</param>
    /// <param name="item">The item to add to the list.</param>
    /// <param name="count">The number of times to add the item.</param>
    /// <remarks>
    /// Use <see cref="AddDefaults{T}(List{T}, int)"/> for <paramref name="item"/> is <c>default(T)</c> for better performance.
    /// </remarks>
    internal static void AddRepeated<T>(this List<T> list, T item, int count)
    {
        if (count <= 0) return;

        var oldCount = list.Count;
        list.EnsureCapacity(oldCount + count);
        CollectionsMarshal.SetCount(list, oldCount + count);
        CollectionsMarshal.AsSpan(list).Slice(oldCount, count).Fill(item);
    } // internal static void AddRepeated<T> (this List<T>, T, int)
} // internal static class ListExtension

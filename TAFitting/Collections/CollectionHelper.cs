
// (c) 2025 Kazuki Kohzuki

/*
 * Collection<T> has an internal items field of type IList<T>, which is actually a List<T> if initialized without specified IList<T>.
 * By default, the internal items field is assumed to be List<T> and directly casted using Unsafe.As for performance reasons.
 * If there is a possibility that the internal items field is not List<T>, define SAFE_COLLECTION to use a safe but slower implementation.
 * Computational overhead of the safe implementation comes from type checking and method call overhead, which is not critical in most cases.
 */
//#define SAFE_COLLECTION

using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace TAFitting.Collections;

/// <summary>
/// Provides helper methods for collections. 
/// </summary>
internal static class CollectionHelper
{
    /// <summary>
    /// Adds the elements of the specified span to the end of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to which the elements should be added.</param>
    /// <param name="span">The span whose elements should be added to the end of the <see cref="Collection{T}"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddRange<T>(this Collection<T> collection, ReadOnlySpan<T> span)
    {
        var items = UnsafeAccessHelper<T>.GetItems(collection);
#if SAFE_COLLECTION
        if (items is List<T> list) list.AddRange(span);
        // The fallback method should not be included here to avoid performance degradation due to inlining prevention.
        else AddRangeForEach(items, span);
#else
        var list = Unsafe.As<List<T>>(items);
        list.AddRange(span);
#endif
    } // internal static void AddRange<T> (this Collection<T>, ReadOnlySpan<T>)

#if SAFE_COLLECTION

    // No inlining to keep the hot path small
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AddRangeForEach<T>(IList<T> items, ReadOnlySpan<T> span)
    {
        for (var i = 0; i < span.Length; i++)
            items.Add(span[i]);
    } // private static void AddRangeForEach<T> (IList<T>, ReadOnlySpan<T>)

#endif

    private static class UnsafeAccessHelper<T>
    {
        // The name of the filed will never be changed for binary serialization compatibility, so it is safe to hardcode the name here.
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "items")]
        extern internal static ref IList<T> GetItems(Collection<T> c);
    } // private static class UnsafeAccessHelper<T>
} // internal static class CollectionHelper

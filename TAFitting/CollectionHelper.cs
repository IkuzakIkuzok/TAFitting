
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

namespace TAFitting;

/// <summary>
/// Provides helper methods for collections. 
/// </summary>
internal static class CollectionHelper
{
    /// <summary>
    /// Adds the elements of the specified collection to the end of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="this">The collection to which the elements should be added.</param>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of the <see cref="Collection{T}"/>.
    /// The collection itself cannot be <see langword="null"/>,
    /// but it can contain elements that are <see langword="null"/>,
    /// if type <typeparamref name="T"/> is a reference type.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void AddRange<T>(this Collection<T> @this, IEnumerable<T> collection)
    {
        /*
         * Collection<T> does not have `AddRange` method, but its internal `items` field is `List<T>`, which has `AddRange` method.
         * So we can use `Unsafe.As` to cast `Collection<T>` to a wrapper class that exposes the `AddRange` method.
         * Getting the internal field via reflection is avoided for performance reasons.
         */

        var wrapper = Unsafe.As<CollectionWrapper<T>>(@this);
        wrapper.AddRange(collection);
    } // internal static void AddRange<T> (Collection<T>, IEnumerable<T>)

    /// <summary>
    /// Wrapper class to expose the internal items of <see cref="Collection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    private class CollectionWrapper<T>
    {
        private readonly IList<T> items = null!;

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="Collection{T}"/>.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="Collection{T}"/>.
        /// The collection itself cannot be <see langword="null"/>,
        /// but it can contain elements that are <see langword="null"/>,
        /// if type <typeparamref name="T"/> is a reference type.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddRange(IEnumerable<T> collection)
        {
#if SAFE_COLLECTION
            if (this.items is List<T> list) list.AddRange(collection);
            // The fallback method shuld not be included here to avoid performance degradation due to inlining prevention.
            else AddRangeForEach(collection);
#else
            var list = Unsafe.As<List<T>>(this.items);
            list.AddRange(collection);
#endif
        } // internal void AddRange (IEnumerable<T>)

#if SAFE_COLLECTION

        private void AddRangeForEach(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                this.items.Add(item);
        } // private void AddRangeForEach (IEnumerable<T>)

#endif
    } // internal class CollectionWrapper<T>
} // internal static class CollectionHelper

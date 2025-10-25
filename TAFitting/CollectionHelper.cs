
// (c) 2025 Kazuki Kohzuki

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
            var list = Unsafe.As<List<T>>(this.items);
            list.AddRange(collection);
        } // internal void AddRange (IEnumerable<T>)
    } // internal class CollectionWrapper<T>
} // internal static class CollectionHelper

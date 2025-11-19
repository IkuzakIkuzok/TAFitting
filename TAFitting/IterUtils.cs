
// (c) 2024 Kazuki KOHZUKI

using System.Numerics;
using System.Runtime.CompilerServices;

namespace TAFitting;

internal delegate bool ConditionalSelector<TSource, TResult>(TSource source, out TResult result);

/// <summary>
/// Utility methods for iteration.
/// </summary>
internal static class IterUtils
{
    /// <summary>
    /// Projects each element of a sequence into a new form by using a conditional selector function that determines whether to include the transformed element.
    /// </summary>
    /// <remarks>The selector function is invoked for each element in the source sequence.
    /// Only elements for which the selector returns true are included in the result sequence.
    /// Enumeration is deferred and performed lazily as the returned sequence is iterated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements returned by the selector function.</typeparam>
    /// <param name="source">The sequence of elements to project.</param>
    /// <param name="selector">A function that, given a source element, determines whether to select it and provides the transformed result if selected.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that contains the elements from the input sequence that satisfy the selector's condition, each transformed by the selector function.</returns>
    internal static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, ConditionalSelector<TSource, TResult> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var element in source)
        {
            if (selector(element, out var result))
                yield return result;
        }
    } // internal static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource>, ConditionalSelector<TSource, TResult>)

    /// <summary>
    /// Projects each element of a sequence into a new form by applying a selector function and yielding results of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <remarks>This method skips elements for which the selector function returns a value that cannot be cast to <typeparamref name="TResult"/>.
    /// The projection is performed lazily; the selector function is invoked only when the resulting sequence is enumerated.</remarks>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements returned by the selector function and yielded in the resulting sequence.</typeparam>
    /// <param name="source">The sequence of elements to project.</param>
    /// <param name="selector">A transform function to apply to each element of the source sequence.
    /// The function must return an object that can be cast to <typeparamref name="TResult"/>; elements for which the result cannot be cast are skipped.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> whose elements are the result of invoking the selector function on each element of the source sequence
    /// and successfully casting the result to <typeparamref name="TResult"/>.
    /// Elements for which the selector result cannot be cast to <typeparamref name="TResult"/> are omitted.</returns>
    internal static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, object?> selector)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (var element in source)
        {
            var result = selector(element);
            if (result is TResult typedResult)
                yield return typedResult;
        }
    } // internal static IEnumerable<TResult> Select<TSource, TResult> (this IEnumerable<TSource>, Func<TSource, object?>)

    /// <summary>
    /// Enumerates the elements of the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="offset">The offset.</param>
    /// <returns>The enumeration of the elements with their index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    internal static IEnumerable<(int index, T element)> Enumerate<T>(this IEnumerable<T> source, int offset = 0)
    {
        ArgumentNullException.ThrowIfNull(source);

        var index = offset;
        foreach (var element in source)
            yield return (index++, element);
    } // internal static IEnumerable<(int index, T element)> Enumerate<T> (this IEnumerable<T>, [int])

    /// <summary>
    /// Computes the average of the elements in the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The average of the elements in the source.</returns>
    /// <remarks>
    /// This method uses SIMD instructions if available.
    /// This causes changes in the result due to the order of the elements.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double FastAverage(this IEnumerable<double> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (!source.TryGetSpan(out var span))
            return source.Average();

        if (!Vector.IsHardwareAccelerated) return source.Average();
        if (span.Length < Vector<double>.Count) return source.Average();

        var sums = Vector<double>.Zero;
        var i = 0;
        do
        {
            sums += new Vector<double>(span[i..]);
            i += Vector<double>.Count;
        } while (i <= span.Length - Vector<int>.Count);

        var sum = Vector.Sum(sums);
        for (; i < span.Length; i++)
            sum += span[i];
        return sum / span.Length;
    } // internal static double FastAverage (this IEnumerable<double>)
} // internal static class IterUtils
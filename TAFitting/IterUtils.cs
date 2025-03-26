
// (c) 2024 Kazuki KOHZUKI

using System.Numerics;
using System.Runtime.CompilerServices;

namespace TAFitting;

/// <summary>
/// Utility methods for iteration.
/// </summary>
internal static class IterUtils
{
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
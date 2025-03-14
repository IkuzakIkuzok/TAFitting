
// (c) 2024 Kazuki KOHZUKI

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

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

    /// <summary>
    /// Validates that source is not null and then tries to extract a span from the source.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="span">The span.</param>
    /// <returns><see langword="true"/> if the span is extracted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetSpan<TSource>(this IEnumerable<TSource> source, out ReadOnlySpan<TSource> span) where TSource : struct
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.GetType() == typeof(TSource[]))
        {
            span = Unsafe.As<TSource[]>(source);
            return true;
        }

        if (source.GetType() == typeof(List<TSource>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<TSource>>(source));
            return true;
        }

        span = default;
        return false;
    } // internal static bool TryGetSpan<TSource> (this IEnumerable<TSource>, out ReadOnlySpan<TSource>)

    #region SIMD Linq for Span<T>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Min<T>(this Span<T> source) where T : struct, INumber<T>
        => ((ReadOnlySpan<T>)source).Min();

    /// <summary>
    /// Returns the minimum value in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The minimum value in the source.</returns>
    internal static T Min<T>(this ReadOnlySpan<T> source) where T : struct, INumber<T>
    {
        if (!Vector256.IsHardwareAccelerated)
        {
            var min = source[0];
            for (var i = 1; i < source.Length; i++)
            { 
                if (source[i] < min)
                    min = source[i];
            }
            return min;
        }
        else
        {
            ref var current = ref MemoryMarshal.GetReference(source);
            ref var to = ref Unsafe.Add(ref current, source.Length - Vector256<T>.Count);

            var v_min = Vector256.LoadUnsafe(ref current);
            current = ref Unsafe.Add(ref current, Vector256<T>.Count);
            while (Unsafe.IsAddressLessThan(ref current, ref to))
            {
                v_min = Vector256.Min(v_min, Vector256.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, Vector256<T>.Count);
            }
            v_min = Vector256.Min(v_min, Vector256.LoadUnsafe(ref to));

            var min = v_min[0];
            for (int i = 1; i < Vector256<T>.Count; i++)
            {
                if (v_min[i] < min)
                {
                    min = v_min[i];
                }
            }
            return min;
        }
    } // internal static T Min<T> (this ReadOnlySpan<T>) where T : struct, INumber<T>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Max<T>(this Span<T> source) where T : struct, INumber<T>
        => ((ReadOnlySpan<T>)source).Max();

    /// <summary>
    /// Returns the maximum value in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The maximum value in the source.</returns>
    internal static T Max<T>(this ReadOnlySpan<T> source) where T : struct, INumber<T>
    {
        if (!Vector256.IsHardwareAccelerated || source.Length < Vector256<T>.Count)
        {
            var max = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                if (source[i] > max)
                    max = source[i];
            }
            return max;
        }
        else
        {
            ref var current = ref MemoryMarshal.GetReference(source);
            ref var to = ref Unsafe.Add(ref current, source.Length - Vector256<T>.Count);

            var v_max = Vector256.LoadUnsafe(ref current);
            current = ref Unsafe.Add(ref current, Vector256<T>.Count);
            while (Unsafe.IsAddressLessThan(ref current, ref to))
            {
                v_max = Vector256.Max(v_max, Vector256.LoadUnsafe(ref current));
                current = ref Unsafe.Add(ref current, Vector256<T>.Count);
            }
            v_max = Vector256.Max(v_max, Vector256.LoadUnsafe(ref to));

            var max = v_max[0];
            for (int i = 1; i < Vector256<T>.Count; i++)
            {
                if (v_max[i] > max)
                {
                    max = v_max[i];
                }
            }
            return max;
        }
    } // internal static T Max<T>(this ReadOnlySpan<T> source) where T : struct, INumber<T>

    #endregion
} // internal static class IterUtils
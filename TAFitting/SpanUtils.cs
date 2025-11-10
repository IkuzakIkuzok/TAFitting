
// (c) 2025 Kazuki Kohzuki

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Intrinsics;

namespace TAFitting;

/// <summary>
/// Provides extension methods for <see cref="Span{T}"/> and <see cref="ReadOnlySpan{T}"/>.
/// </summary>
internal static class SpanUtils
{
    /// <summary>
    /// Tries to extract the <see cref="Span{T}"/> from the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="span">The extracted span.</param>
    /// <returns><see langword="true"/> if the span is extracted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryGetSpan<T>(this IEnumerable<T> source, out Span<T> span)
    {
        if (source.GetType() == typeof(T[]))
        {
            span = Unsafe.As<T[]>(source);
            return true;
        }

        if (source.GetType() == typeof(List<T>))
        {
            span = CollectionsMarshal.AsSpan(Unsafe.As<List<T>>(source));
            return true;
        }

        span = default;
        return false;
    } // internal static bool TryGetSpan<T> (this IEnumerable<T>, out Span<T>)

    /// <summary>
    /// Computes the sum of the elements in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The sum of the elements in the source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Sum<T>(this Span<T> source) where T : struct, INumber<T>
        => ((ReadOnlySpan<T>)source).Sum();

    /// <summary>
    /// Computes the sum of the elements in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The sum of the elements in the source.</returns>
    internal static T Sum<T>(this ReadOnlySpan<T> source) where T : struct, INumber<T>
    {
        if (!Vector256.IsHardwareAccelerated)
        {
            var sum = source[0];
            for (var i = 1; i < source.Length; i++)
            {
                sum += source[i];
            }
            return sum;
        }
        else
        {
            var sum = T.Zero;

            ref var begin = ref MemoryMarshal.GetReference(source);
            ref var last = ref Unsafe.Add(ref begin, source.Length);
            ref var current = ref begin;
            var vectorSum = Vector256<T>.Zero;

            ref var to = ref Unsafe.Add(ref begin, source.Length - Vector256<T>.Count);
            while (Unsafe.IsAddressLessThan(ref current, ref to))
            {
                vectorSum += Vector256.LoadUnsafe(ref current);
                current = ref Unsafe.Add(ref current, Vector256<T>.Count);
            }
            while (Unsafe.IsAddressLessThan(ref current, ref last))
            {
                unchecked // SIMD operation is unchecked so keep same behaviour
                {
                    sum += current;
                }
                current = ref Unsafe.Add(ref current, 1);
            }

            sum += Vector256.Sum(vectorSum);
            return sum;
        }
    } // internal static T Sum<T> (this ReadOnlySpan<T>) where T : struct, INumber<T>

    /// <summary>
    /// Computes the average of the elements in the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The average of the elements in the source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Average(this Span<double> source)
        => ((ReadOnlySpan<double>)source).Average();

    /// <summary>
    /// Computes the average of the elements in the source.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <returns>The average of the elements in the source.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Average(this ReadOnlySpan<double> source)
    {
        var sum = source.Sum();
        return sum / source.Length;
    } // internal static double Average (this ReadOnlySpan<double>)

    /// <summary>
    /// Returns the minimum value in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The minimum value in the source.</returns>
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
            for (var i = 1; i < Vector256<T>.Count; i++)
            {
                if (v_min[i] < min)
                {
                    min = v_min[i];
                }
            }
            return min;
        }
    } // internal static T Min<T> (this ReadOnlySpan<T>) where T : struct, INumber<T>

    /// <summary>
    /// Returns the maximum value in the source.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The source.</param>
    /// <returns>The maximum value in the source.</returns>
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
            for (var i = 1; i < Vector256<T>.Count; i++)
            {
                if (v_max[i] > max)
                {
                    max = v_max[i];
                }
            }
            return max;
        }
    } // internal static T Max<T>(this ReadOnlySpan<T> source) where T : struct, INumber<T>

    #region parse

    /// <summary>
    /// Parses the <see cref="ReadOnlySpan{Char}"/> to an integer using invariant culture.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>The parsed integer.</returns>
    internal static int ParseIntInvariant(this ReadOnlySpan<char> span)
        => int.Parse(span, CultureInfo.InvariantCulture);

    /// <summary>
    /// Parses the <see cref="ReadOnlySpan{Char}"/> to a double using invariant culture.
    /// </summary>
    /// <param name="span">The span to parse.</param>
    /// <returns>The parsed double.</returns>
    internal static double ParseDoubleInvariant(this ReadOnlySpan<char> span)
        => double.Parse(span, CultureInfo.InvariantCulture);

    #endregion parse
} // internal static class SpanUtils

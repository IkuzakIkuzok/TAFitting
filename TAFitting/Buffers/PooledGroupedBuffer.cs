
// (c) 2025 Kazuki Kohzuki

using System.Buffers;
using System.Runtime.CompilerServices;
using TAFitting;

namespace TAFitting.Buffers;

/// <summary>
/// Provides a pooled buffer that manages multiple contiguous groups of elements of type T, enabling efficient allocation and reuse of memory for grouped data operations.
/// </summary>
/// <remarks>This ref struct is intended for high-performance scenarios where multiple groups of elements need to be managed together using a single pooled buffer.
/// The buffer is allocated from the shared array pool on demand and should be released by calling Dispose when no longer needed.
/// Consifer using <see cref="PooledBuffer{T}"/> instead if only a single buffer is required, as it may be simpler and more efficient in such cases.
/// </remarks>
/// <typeparam name="T">The type of elements stored in each group within the buffer.</typeparam>
internal ref struct PooledGroupedBuffer<T>
{
    private readonly int _length;
    private readonly int _count;
    private T[]? _buffer = null;

    /// <summary>
    /// Initializes a new instance of the PooledGroupedBuffer class with the specified buffer length and group count.
    /// </summary>
    /// <param name="length">The length of each buffer in the group.</param>
    /// <param name="count">The number of buffers to include in the group.</param>
    internal PooledGroupedBuffer(int length, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(count));
        this._length = length;
        this._count = count;
    } // ctor (int, int)

    /// <summary>
    /// Returns a span representing the element at the specified index within the underlying buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the element to retrieve.</param>
    /// <returns>A span of type T that provides access to the data at the specified index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<T> GetSpan(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._count, nameof(index));
        this._buffer ??= RentArray();
        var offset = index * this._length;
        return this._buffer.AsSpan(offset, this._length);
    } // GetSpan (int)

    // No inlining to reduce the size of the hot path
    [MethodImpl(MethodImplOptions.NoInlining)]
    private readonly T[] RentArray()
    {
        var length = this._length * this._count;
        InvalidOperationException.ThrowIf(length > Array.MaxLength, "The requested buffer size exceeds the maximum allowable array length.");
        return ArrayPool<T>.Shared.Rent(length);
    } // private readonly T[] Rent ()

    /// <summary>
    /// Releases resources used by the instance and returns the underlying buffer to the shared array pool.
    /// </summary>
    public void Dispose()
    {
        var buffer = this._buffer;
        if (buffer is null) return;
        this._buffer = null;
        ArrayPool<T>.Shared.Return(buffer);
    } // public void Dispose ()
} // internal ref struct PooledGroupedBuffer<T>

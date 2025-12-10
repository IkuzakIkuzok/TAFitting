
// (c) 2025 Kazuki Kohzuki

using System.Buffers;
using System.Runtime.CompilerServices;

namespace TAFitting;

/// <summary>
/// Provides a buffer of a specified length that is rented from a shared array pool and released when disposed.
/// </summary>
/// <remarks>A <see cref="PooledBuffer{T}"/> enables efficient temporary allocation of arrays by renting from the shared array pool.
/// The buffer is returned to the pool when the instance is disposed.
/// It is strongly recommended to use a <see langword="using"/> statement or a <see langword="using"/> declaration to ensure proper disposal of the instance.
/// This type is intended for internal use to minimize memory allocations in performance-critical scenarios.</remarks>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
internal ref struct PooledBuffer<T>
{
    private readonly int _length;
    private T[]? _buffer = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="PooledBuffer{T}"/> struct with the specified length.
    /// </summary>
    /// <param name="length">The size of the buffer, in bytes.</param>
    /// <remarks>
    /// The buffer is not allocated (or rented) at this point.
    /// It is allocated (or rented) when <see cref="GetSpan"/> is called for the first time.
    /// </remarks>
    internal PooledBuffer(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        this._length = length;
    } // ctor (int)

    /// <summary>
    /// Returns a span representing the valid portion of the underlying buffer.
    /// </summary>
    /// <returns>A <see cref="Span{T}"/> containing the elements in the buffer up to the current length.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<T> GetSpan()
    {
        this._buffer ??= ArrayPool<T>.Shared.Rent(this._length);
        return this._buffer.AsSpan()[..this._length];
    } // internal Span<T> GetSpan (

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
} // internal ref struct PooledBuffer<T>

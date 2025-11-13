
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Data;

/// <summary>
/// Represents a cache for file data.
/// </summary>
internal sealed class FileCache
{
    /*
     * A buffer for entire data (107457 bytes) is too large and allocated on Large Object Heap (LOH).
     * This is unfavorable for performance.
     * Splitting the buffer into two (53793 bytes each) avoids LOH allocation and improves performance.
     * 53750 bytes = 43 bytes/line * 1250 lines
     * 1250 lines = Ceil(2499 lines / 2)
     */

    internal const int LINE_LENGTH = 43;
    internal const int LINE_COUNT = 2499;

    private const int BUFFER_LENGTH = 53750;

    private readonly byte[] _buffer1, _buffer2;

    /// <summary>
    /// Gets the length of the data.
    /// </summary>
    internal int Length { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCache"/> class.
    /// </summary>
    internal FileCache(bool half = false)
    {
        this._buffer1 = new byte[BUFFER_LENGTH];
        this._buffer2 = half ? [] : new byte[BUFFER_LENGTH];
    } // ctor ()

    /// <summary>
    /// Gets the buffers for file data.
    /// </summary>
    /// <returns>The list of buffers.</returns>
    /// <remarks>
    /// The returned list can be used directly in <see cref="RandomAccess.Read"/> method.
    /// </remarks>
    internal IReadOnlyList<Memory<byte>> GetBuffers()
        => [this._buffer1.AsMemory(), this._buffer2.AsMemory()];

    /// <summary>
    /// Reads a line from the buffer at the specified line index.
    /// </summary>
    /// <param name="lineIndex">The index of the line to read.</param>
    /// <returns>The span of bytes representing the line.</returns>
    internal Span<byte> ReadLine(int lineIndex)
    {
        var start = lineIndex * LINE_LENGTH;
        //var bufferIndex = start % BUFFER_LENGTH;
        //return (start < BUFFER_LENGTH ? this._buffer1 : this._buffer2).AsSpan(bufferIndex, LINE_LENGTH);
        if (start < BUFFER_LENGTH)
            return this._buffer1.AsSpan(start, LINE_LENGTH);
        else
            return this._buffer2.AsSpan(start - BUFFER_LENGTH, LINE_LENGTH);
    } // internal Span<byte> ReadLine (int)
} // internal sealed class FileCache

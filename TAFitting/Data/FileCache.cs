
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
     * 53793 bytes = 43 bytes/line * 1251 lines
     */

    internal const int LINE_LENGTH = 43;
    internal const int LINE_COUNT = 2499;

    private const int BUFFER_LENGTH = 53793;

    private int length = 0;
    private readonly byte[] _buffer1, _buffer2;

    /// <summary>
    /// Gets the length of the data.
    /// </summary>
    internal int Length => this.length;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCache"/> class.
    /// </summary>
    internal FileCache(bool half = false)
    {
        this._buffer1 = new byte[BUFFER_LENGTH];
        if (half)
            this._buffer2 = null!;
        else
            this._buffer2 = new byte[BUFFER_LENGTH];
    } // ctor ()

    /// <summary>
    /// Appends the specified data to the buffer.
    /// </summary>
    /// <param name="data">The data to append.</param>
    /// <remarks>
    /// This method is NOT thread-safe.
    /// Ensure that calls to this method are synchronized if accessed from multiple threads.
    /// </remarks>
    internal void Append(ReadOnlySpan<byte> data)
    {
        var i = this.length % BUFFER_LENGTH;
        data.CopyTo((this.length < BUFFER_LENGTH ? this._buffer1 : this._buffer2).AsSpan(i));
        this.length += data.Length;
    } // internal void Append (ReadOnlySpan<byte>)

    /// <summary>
    /// Reads a line from the buffer at the specified line index.
    /// </summary>
    /// <param name="lineIndex">The index of the line to read.</param>
    /// <returns>The span of bytes representing the line.</returns>
    internal Span<byte> ReadLine(int lineIndex)
    {
        var start = lineIndex * LINE_LENGTH;
        var bufferIndex = start % BUFFER_LENGTH;
        return (start < BUFFER_LENGTH ? this._buffer1 : this._buffer2).AsSpan(bufferIndex, LINE_LENGTH);
    } // internal Span<byte> ReadLine (int)
} // internal sealed class FileCache

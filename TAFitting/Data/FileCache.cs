
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Data;

/// <summary>
/// Represents a cache for file data.
/// </summary>
internal sealed class FileCache
{
    private readonly byte[] _buffer;

    /// <summary>
    /// Gets the length of the data.
    /// </summary>
    internal int Length { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCache"/> class.
    /// </summary>
    internal FileCache() : this(43 * 2499) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileCache"/> class with the specified length.
    /// </summary>
    /// <param name="length">The length of the buffer.</param>
    internal FileCache(int length)
    {
        this._buffer = new byte[length];
    } // ctor (int)

    /// <summary>
    /// Gets the buffer as a span.
    /// </summary>
    /// <returns>The buffer as a span.</returns>
    internal Span<byte> AsSpan() => this._buffer.AsSpan(0, this.Length);

    /// <summary>
    /// Appends the specified data to the buffer.
    /// </summary>
    /// <param name="data">The data to append.</param>
    internal void Append(Span<byte> data)
    {
        data.CopyTo(this._buffer.AsSpan(this.Length));
        this.Length += data.Length;
    } // internal void Append (Span<byte>)
} // internal sealed class FileCache

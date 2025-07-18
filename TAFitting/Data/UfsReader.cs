﻿
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Data;

/// <summary>
/// Provides methods to read data from a stream.
/// </summary>
internal sealed class UfsReader : UfsIOHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UfsReader"/> class with the specified stream.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    /// <param name="leaveOpen">If <see langword="true"/>, the stream will not be closed when this instance is disposed.</param>
    internal UfsReader(Stream stream, bool leaveOpen) : base(stream, leaveOpen) { }

    /// <summary>
    /// Reads a 32-bit signed integer from the stream.
    /// </summary>
    /// <returns>The 32-bit signed integer read from the stream.</returns>
    internal int ReadInt32()
    {
        var bytes = new byte[sizeof(int)];
        this._stream.Read(bytes, 0, bytes.Length);
        return BytesConverter.ToInt32(bytes);
    } // internal int ReadInt32 ()

    /// <summary>
    /// Reads a 64-bit floating-point number from the stream.
    /// </summary>
    /// <returns>The 64-bit floating-point number read from the stream.</returns>
    internal double ReadDouble()
    {
        var bytes = new byte[sizeof(double)];
        this._stream.Read(bytes, 0, bytes.Length);
        return BytesConverter.ToDouble(bytes);
    } // internal double ReadDouble ()

    /// <summary>
    /// Reads a specified number of 64-bit floating-point numbers from the stream.
    /// </summary>
    /// <param name="count">The number of 64-bit floating-point numbers to read.</param>
    /// <returns>An array of 64-bit floating-point numbers read from the stream.</returns>
    internal double[] ReadDoubles(int count)
    {
        var values = new double[count];
        for (var i = 0; i < count; i++)
            values[i] = ReadDouble();
        return values;
    } // internal double[] ReadDoubles (int)

    /// <summary>
    /// Reads a string from the stream.
    /// </summary>
    /// <returns>The string read from the stream.</returns>
    internal string ReadString()
    {
        var length = ReadInt32();
        if (length == 0) return string.Empty;

        var bytes = new byte[length];
        this._stream.Read(bytes, 0, bytes.Length);
        return NormNewLineInput(BytesConverter.ToString(bytes));
    } // internal string ReadString ()

    /// <summary>
    /// Skips a string in the stream.
    /// </summary>
    /// <remarks>
    /// This method reads the length of the string and skips the corresponding number of bytes in the stream without unnecessary allocations.
    /// </remarks>
    internal void SkipString()
    {
        var length = ReadInt32();
        if (length == 0) return;
        this._stream.Seek(length, SeekOrigin.Current);
    } // internal void SkipString ()
} // internal sealed class UfsReader : UfsIOHelper

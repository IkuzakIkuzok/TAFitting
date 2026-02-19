
// (c) 2026 Kazuki KOHZUKI

namespace TAFitting;

/// <summary>
/// Provides extension methods for stream-based text reading.
/// </summary>
internal static class TextStreamExtension
{
    /// <summary>
    /// Reads a line of characters from the current stream into the specified buffer.
    /// </summary>
    /// <remarks>A line is considered to be terminated by a carriage return ('\r'), a line feed ('\n'), or a carriage return immediately followed by a line feed.
    /// If the buffer is not large enough to contain the entire line, the method returns <see langword="false"/> and the buffer contains as many characters as fit.
    /// The method does not allocate additional memory and does not return the line as a string.</remarks>
    /// <param name="reader">The <see cref="StreamReader"/> instance to read from.</param>
    /// <param name="buffer">The buffer to receive the characters read from the stream. The method attempts to fill this buffer with the contents of a single line.</param>
    /// <param name="read">When this method returns, contains the number of characters read into <paramref name="buffer"/>.</param>
    /// <returns><see langword="true"/> if the entire line was read and fits within the buffer;
    /// otherwise, <see langword="false"/> if the buffer was not large enough to hold the entire line.</returns>
    internal static bool ReadLine(this StreamReader reader, Span<char> buffer, out int read)
    {
        read = 0;
        while (true)
        {
            var ch = reader.Read();
            if (ch == -1)
            {
                // End of stream
                break;
            }
            if (ch == '\n')
            {
                // Newline character
                break;
            }
            if (ch == '\r')
            {
                // Carriage return, check for following newline
                if (reader.Peek() == '\n')
                {
                    reader.Read(); // Consume the newline
                }
                break;
            }
            buffer[read++] = (char)ch;

            if (read == buffer.Length)
            {
                // Buffer overflow
                return false;
            }
        }
        return true;
    } // internal static bool ReadLine (this StreamReader, Span<char> buffer, out int)

    /// <summary>
    /// Parses a single line of CSV-formatted text from the current position of the stream and converts each cell to a value of type <typeparamref name="T"/>, writing the results into the specified output buffer.
    /// </summary>
    /// <typeparam name="T">The type of value to parse from each CSV cell.</typeparam>
    /// <param name="reader">The <see cref="StreamReader"/> from which to read the CSV line.</param>
    /// <param name="output">The buffer that receives the parsed values.</param>
    /// <param name="separator">The character used to separate cells in the CSV line. Defaults to ','. Cannot be a newline character.</param>
    /// <param name="provider">An optional format provider to use when parsing each cell. If null, the current culture is used.</param>
    /// <returns>The number of values successfully parsed and written to the output buffer. This will be less than or equal to the length of the output buffer.</returns>
    /// <exception cref="ArgumentException">Thrown if output is empty or if separator is a newline character.</exception>
    internal static int ParseCsvLine<T>(this StreamReader reader, Span<T> output, char separator = ',', IFormatProvider? provider = null) where T : ISpanParsable<T>
    {
        if (output.IsEmpty)
            throw new ArgumentException("Output buffer cannot be empty.", nameof(output));
        if (separator is '\r' or '\n')
            throw new ArgumentException("Separator cannot be a newline character.", nameof(separator));

        // Temporary buffer for reading each cell. Adjust size as needed, but keep in mind that it should be large enough to hold the longest expected cell value.
        var cellBuffer = (stackalloc char[256]); 
        var cellLen = 0;
        var parsed = 0;
        int c;

        while ((c = reader.Read()) != -1)
        {
            var ch = (char)c;
            if (ch == separator || ch is '\r' or '\n')
            {
                if (ch == '\r')
                {
                    if (reader.Peek() == '\n') reader.Read();
                    ch = '\n';
                }

                var cell = cellBuffer[..cellLen];
                output[parsed++] = T.Parse(cell, provider);

                if (ch is '\n') return parsed;

                cellLen = 0;

                if (parsed == output.Length)
                {
                    // Output buffer overflow
                    SkipToEndOfLine(reader);
                    return parsed;
                }
            }
            else
            {
                if (cellLen < cellBuffer.Length)
                    cellBuffer[cellLen++] = ch;
            }
        }

        return parsed;
    } // internal static int ParseCsvLine<T> (this StreamReader, Span<T>, [char], [IFormatProvider]) where T : ISpanParsable<T>

    private static void SkipToEndOfLine(StreamReader reader)
    {
        int c;
        while ((c = reader.Read()) != -1)
        {
            var ch = (char)c;

            if (ch == '\n') return;
            if (ch == '\r')
            {
                if (reader.Peek() == '\n') reader.Read();
                return;
            }
        }
    } // private static void SkipToEndOfLine (this StreamReader)
} // internal class TextStreamExtension


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

    

    
} // internal class TextStreamExtension

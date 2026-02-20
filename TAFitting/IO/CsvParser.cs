
// (c) 2026 Kazuki KOHZUKI

using DisposalGenerator;
using System.Text;

namespace TAFitting.IO;

/// <summary>
/// Provides methods for parsing and validating CSV-formatted data from a text stream using a specified separator character.
/// </summary>
[AutoDisposal]
internal partial class CsvParser : IDisposable
{
    protected readonly StreamReader _reader;

    /// <summary>
    /// Initializes a new instance of the CsvParser class using the specified StreamReader as the input source.
    /// </summary>
    /// <param name="reader">The <see cref="StreamReader"/> that provides the CSV data to parse.</param>
    internal CsvParser(StreamReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader, nameof(reader));
        this._reader = reader;
    } // ctor (StreamReader)

    /// <summary>
    /// Initializes a new instance of the CsvParser class for reading CSV data from the specified file using the given text encoding.
    /// </summary>
    /// <param name="path">The path to the CSV file to be read.</param>
    /// <param name="encoding">The character encoding to use when reading the file.</param>
    internal CsvParser(string path, Encoding encoding)
    {
        this._reader = new(path, encoding);
    } // ctor (string, Encoding)

    /// <summary>
    /// Verifies that the current line of the input matches the expected header columns,
    /// using the specified separator character to delimit columns.
    /// </summary>
    /// <param name="expected">The expected header column names, in order. If empty, the method returns <see langword="true"/> without reading from the input.</param>
    /// <param name="separator">The character used to separate columns. Cannot be a newline character. The default is ','.</param>
    /// <returns><see langword="true"/> if the current line matches the expected header columns; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method advances the reader to the end of the current line regardless of whether the header verification succeeds or fails, ensuring that subsequent reads start from the next line.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <paramref name="separator"/> is a newline character (either '\r' or '\n').</exception>
    internal bool VerifyHeader(ReadOnlySpan<string> expected, char separator = ',')
    {
        if (expected.IsEmpty)
        {
            SkipToEndOfLine();
            return true;
        }

        if (separator is '\r' or '\n')
            throw new ArgumentException("Separator cannot be a newline character.", nameof(separator));

        int c;
        var ch = '\0';
        var eolReached = false;
        foreach (var e in expected)
        {
            for (var charIdx = 0; charIdx < e.Length; charIdx++)
            {
                if ((c = this._reader.Read()) == -1) goto Failed;
                ch = (char)c;
                if (ch != e[charIdx]) goto Failed;
            }

            // Expect end of column (i.e., separator or EOL) here.

            if ((c = this._reader.Read()) == -1) goto Failed;
            ch = (char)c;
            eolReached = ch is '\r' or '\n';
            if (ch != separator && !eolReached) goto Failed;
        }

        if (eolReached)
        {
            if (ch == '\r' && this._reader.Peek() == '\n')
                this._reader.Read();
        }
        else
        {
            SkipToEndOfLine();
        }
            
        return true;

    Failed:
        SkipToEndOfLine();
        return false;
    } // internal bool VerifyHeader (ReadOnlySpan<string>, [char])

    /// <summary>
    /// Skips the specified number of columns in the current line of the input, using the given separator character.
    /// </summary>
    /// <param name="count">The number of columns to skip.</param>
    /// <param name="separator">The character used to separate columns. Cannot be a newline character. The default is ','.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="separator"/> is a newline character (either '\r' or '\n').</exception>
    /// <exception cref="FormatException">Thrown if the end of the line is reached before the specified number of columns have been skipped.</exception>
    internal void SkipColumns(int count, char separator = ',')
    {
        if (count <= 0) return;

        if (separator is '\r' or '\n')
            throw new ArgumentException("Separator cannot be a newline character.", nameof(separator));

        int c;
        while ((c = this._reader.Read()) != -1)
        {
            var ch = (char)c;
            if (ch == separator)
            {
                if (--count == 0) return;
            }
            else if (ch == '\r')
            {
                if (this._reader.Peek() == '\n') this._reader.Read();
                break;
            }
            else if (ch == '\n')
            {
                break;
            }
        }

        // Reached end of line or end of file before skipping the required number of columns
        throw new FormatException("Unexpected end of line while skipping columns.");
    } // internal void SkipColumns (int, [char])

    /// <summary>
    /// Parses a single line of CSV-formatted text from the current position of the stream and converts each cell to a value of type <typeparamref name="T"/>, writing the results into the specified output buffer.
    /// </summary>
    /// <typeparam name="T">The type of value to parse from each CSV cell.</typeparam>
    /// <param name="output">The buffer that receives the parsed values.</param>
    /// <param name="separator">The character used to separate cells in the CSV line. Defaults to ','. Cannot be a newline character.</param>
    /// <param name="provider">An optional format provider to use when parsing each cell. If null, the current culture is used.</param>
    /// <returns>The number of values successfully parsed and written to the output buffer. This will be less than or equal to the length of the output buffer.</returns>
    /// <exception cref="ArgumentException">Thrown if output is empty or if separator is a newline character.</exception>
    internal int ParseLine<T>(Span<T> output, char separator = ',', IFormatProvider? provider = null) where T : ISpanParsable<T>
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

        while ((c = this._reader.Read()) != -1)
        {
            var ch = (char)c;
            if (ch == separator || ch is '\r' or '\n')
            {
                if (ch == '\r')
                {
                    if (this._reader.Peek() == '\n') this._reader.Read();
                    ch = '\n';
                }

                var cell = cellBuffer[..cellLen];
                output[parsed++] = T.Parse(cell, provider);

                if (ch is '\n') return parsed;

                cellLen = 0;

                if (parsed == output.Length)
                {
                    // Output buffer overflow
                    SkipToEndOfLine();
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
    } // internal int ParseLine<T> (Span<T>, [char], [IFormatProvider]) where T : ISpanParsable<T>

    private void SkipToEndOfLine()
    {
        int c;
        while ((c = this._reader.Read()) != -1)
        {
            var ch = (char)c;

            if (ch == '\n') return;
            if (ch == '\r')
            {
                if (this._reader.Peek() == '\n') this._reader.Read();
                return;
            }
        }
    } // private void SkipToEndOfLine ()
} // internal partial class CsvParser : IDisposable

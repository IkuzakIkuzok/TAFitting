
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace TAFitting;

/// <summary>
/// Provides utility methods for text processing.
/// </summary>
internal static class TextUtils
{
    private static readonly Encoding DefaultEncoding;

    internal static Encoding CP932;

    static TextUtils()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var codePage = Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage;
        DefaultEncoding = Encoding.GetEncoding(codePage);

        CP932 = Encoding.GetEncoding(932);
    } // cctor ()

    
    /// <summary>
    /// Decodes the specified sequence of bytes into a string using the detected or default encoding.
    /// </summary>
    /// <remarks>If the encoding cannot be determined from the byte sequence, a default encoding is used.
    /// The method does not throw if the byte sequence is empty; it returns an empty string.</remarks>
    /// <param name="bytes">The read-only span of bytes to decode into a string.</param>
    /// <returns>A string representation of the decoded bytes.</returns>
    internal static string GetText(this ReadOnlySpan<byte> bytes)
    {
        var encoding = bytes.GetEncoding() ?? DefaultEncoding;
        return encoding.GetString(bytes);
    } // internal static string GetText (this ReadOnlySpan<byte>)


    /// <summary>
    /// Attempts to detect the text encoding of the specified byte sequence, with a focus on common Japanese encodings.
    /// </summary>
    /// <remarks>This method heuristically distinguishes between several encodings, including ASCII, UTF-8, Shift_JIS, EUC-JP, JIS, and Unicode.
    /// Detection is not guaranteed to be accurate for all inputs, especially for short or ambiguous byte sequences.
    /// The method is primarily intended for Japanese text and may not reliably detect encodings for other languages.</remarks>
    /// <param name="bytes">The byte sequence to analyze for encoding detection.</param>
    /// <returns>An Encoding instance representing the detected encoding, or <see langword="null"/> if the encoding cannot be determined.</returns>
    internal static Encoding? GetEncoding(this ReadOnlySpan<byte> bytes)
    {
        /*
         * https://dobon.net/vb/dotnet/string/detectcode.html
         */

        const byte bEscape = 0x1B;
        const byte bAt = 0x40;
        const byte bDollar = 0x24;
        const byte bAnd = 0x26;
        const byte bOpen = 0x28;    //'('
        const byte bB = 0x42;
        const byte bD = 0x44;
        const byte bJ = 0x4A;
        const byte bI = 0x49;

        var len = bytes.Length;
        byte b1, b2, b3, b4;

        var isBinary = false;
        for (var i = 0; i < len; i++)
        {
            b1 = bytes[i];
            if (b1 is <= 0x06 or 0x7F or 0xFF)
            {
                //'binary'
                isBinary = true;
                if (b1 == 0x00 && i < len - 1 && bytes[i + 1] <= 0x7F)
                {
                    //smells like raw unicode
                    return Encoding.Unicode;
                }
            }
        }
        if (isBinary) return null;

        //not Japanese
        var notJapanese = true;
        for (var i = 0; i < len; i++)
        {
            b1 = bytes[i];
            if (b1 is bEscape or >= 0x80)
            {
                notJapanese = false;
                break;
            }
        }
        if (notJapanese) return Encoding.ASCII;

        for (var i = 0; i < len - 2; i++)
        {
            b1 = bytes[i];
            b2 = bytes[i + 1];
            b3 = bytes[i + 2];

            if (b1 == bEscape)
            {
                if (b2 == bDollar && b3 == bAt)
                {
                    //JIS_0208 1978
                    //JIS
                    return Encoding.GetEncoding(50220);
                }
                else if (b2 == bDollar && b3 == bB)
                {
                    //JIS_0208 1983
                    //JIS
                    return Encoding.GetEncoding(50220);
                }
                else if (b2 == bOpen && (b3 == bB || b3 == bJ))
                {
                    //JIS_ASC
                    //JIS
                    return Encoding.GetEncoding(50220);
                }
                else if (b2 == bOpen && b3 == bI)
                {
                    //JIS_KANA
                    //JIS
                    return Encoding.GetEncoding(50220);
                }
                if (i < len - 3)
                {
                    b4 = bytes[i + 3];
                    if (b2 == bDollar && b3 == bOpen && b4 == bD)
                    {
                        //JIS_0212
                        //JIS
                        return Encoding.GetEncoding(50220);
                    }
                    if (i < len - 5 &&
                        b2 == bAnd && b3 == bAt && b4 == bEscape &&
                        bytes[i + 4] == bDollar && bytes[i + 5] == bB)
                    {
                        //JIS_0208 1990
                        //JIS
                        return Encoding.GetEncoding(50220);
                    }
                }
            }
        }

        //should be euc|sjis|utf8
        //use of (?:) by Hiroki Ohzaki <ohzaki@iod.ricoh.co.jp>
        var sjis = 0;
        var euc = 0;
        var utf8 = 0;
        for (var i = 0; i < len - 1; i++)
        {
            b1 = bytes[i];
            b2 = bytes[i + 1];
            if (((0x81 <= b1 && b1 <= 0x9F) || (0xE0 <= b1 && b1 <= 0xFC)) &&
                ((0x40 <= b2 && b2 <= 0x7E) || (0x80 <= b2 && b2 <= 0xFC)))
            {
                //SJIS_C
                sjis += 2;
                i++;
            }
        }
        for (var i = 0; i < len - 1; i++)
        {
            b1 = bytes[i];
            b2 = bytes[i + 1];
            if (((0xA1 <= b1 && b1 <= 0xFE) && (0xA1 <= b2 && b2 <= 0xFE)) ||
                (b1 == 0x8E && (0xA1 <= b2 && b2 <= 0xDF)))
            {
                //EUC_C
                //EUC_KANA
                euc += 2;
                i++;
            }
            else if (i < len - 2)
            {
                b3 = bytes[i + 2];
                if (b1 == 0x8F && (0xA1 <= b2 && b2 <= 0xFE) &&
                    (0xA1 <= b3 && b3 <= 0xFE))
                {
                    //EUC_0212
                    euc += 3;
                    i += 2;
                }
            }
        }
        for (var i = 0; i < len - 1; i++)
        {
            b1 = bytes[i];
            b2 = bytes[i + 1];
            if ((0xC0 <= b1 && b1 <= 0xDF) && (0x80 <= b2 && b2 <= 0xBF))
            {
                //UTF8
                utf8 += 2;
                i++;
            }
            else if (i < len - 2)
            {
                b3 = bytes[i + 2];
                if ((0xE0 <= b1 && b1 <= 0xEF) && (0x80 <= b2 && b2 <= 0xBF) &&
                    (0x80 <= b3 && b3 <= 0xBF))
                {
                    //UTF8
                    utf8 += 3;
                    i += 2;
                }
            }
        }
        //M. Takahashi's suggestion
        //utf8 += utf8 / 2;

        if (euc > sjis && euc > utf8)
        {
            //EUC
            return Encoding.GetEncoding(51932);
        }
        else if (sjis > euc && sjis > utf8)
        {
            //SJIS
            return Encoding.GetEncoding(932);
        }
        else if (utf8 > euc && utf8 > sjis)
        {
            //UTF8
            return Encoding.UTF8;
        }

        return null;
    } // internal static Encoding? GetEncoding (this ReadOnlySpan<byte>)

    #region Parse

    internal static int ParseIntInvariant(this string s)
        => int.Parse(s, CultureInfo.InvariantCulture);

    internal static double ParseDoubleInvariant(this string s)
        => double.Parse(s, CultureInfo.InvariantCulture);

    #endregion Parse

    #region ToString

    /// <summary>
    /// Converts the specified integer to its string representation using the invariant culture.
    /// </summary>
    /// <param name="value">The integer value to convert to a string.</param>
    /// <param name="format">A standard or custom numeric format string that defines the format of the returned string. If <see langword="null"/> or empty, the default format is used.</param>
    /// <returns>A string representation of the integer value, formatted using the specified format string and the invariant culture.</returns>
    internal static string ToInvariantString(this int value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts the specified decimal value to its string representation using the invariant culture.
    /// </summary>
    /// <param name="value">The decimal value to convert to a string.</param>
    /// <param name="format">A standard or custom numeric format string that defines the format of the returned string. If <see langword="null"/> or empty, the default format is used.</param>
    /// <returns>A string representation of the decimal value, formatted using the specified format string and the invariant culture.</returns>
    internal static string ToInvariantString(this decimal value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    /// <summary>
    /// Converts the specified double-precision floating-point number to its string representation using the invariant
    /// culture.
    /// </summary>
    /// <param name="value">The double-precision floating-point number to convert.</param>
    /// <param name="format">A standard or custom numeric format string that defines the format of the returned string. If <see langword="null"/> or empty, the default format is used.</param>
    /// <returns>A string representation of the value of the double-precision floating-point number, formatted using the specified format string and the invariant culture.</returns>
    internal static string ToInvariantString(this double value, [StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    #endregion ToString

    /// <summary>
    /// Gets the last partition of the string separated by the specified separator character.
    /// </summary>
    /// <param name="s">The string to get the last partition from.</param>
    /// <param name="separator">The separator character.</param>
    /// <returns>
    /// The last partition of the string.
    /// If the separator character is not found in the string, the entire string is returned.
    /// </returns>
    internal static string GetLastPartition(this string s, char separator)
    {
        var index = s.LastIndexOf(separator);
        return index >= 0 ? s[(index + 1)..] : s;
    } // internal static string GetLastPartition (this string, char)

    /// <summary>
    /// Reads a line of characters from the current stream and writes the data to the specified buffer.
    /// </summary>
    /// <remarks>A line is considered to be terminated by a carriage return (<c>'\r'</c>), a line feed (<c>'\n'</c>),
    /// or a carriage return immediately followed by a line feed.
    /// The terminating line break characters are not included in the buffer.
    /// If the buffer is not large enough to hold the entire line, the method returns -1 and does not write a partial line.</remarks>
    /// <param name="reader">The <see cref="StreamReader"/> instance to read from.</param>
    /// <param name="buffer">The buffer that receives the characters read from the stream.</param>
    /// <returns>The number of characters written to the buffer, or -1 if the buffer is too small to hold the entire line.
    /// Returns 0 if the end of the stream is reached before any characters are read.</returns>
    internal static int ReadLine(this StreamReader reader, Span<char> buffer)
    {
        var count = 0;
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
            buffer[count++] = (char)ch;

            if (count == buffer.Length)
            {
                // Buffer overflow
                return -1;
            }
        }
        return count;
    } // internal static int ReadLine (this StreamReader, Span<char>)
} // internal static class TextUtils

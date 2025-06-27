
// (c) 2024 Kazuki KOHZUKI

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
    /// Gets the text from the specified byte array.
    /// </summary>
    /// <param name="bytes">The byte array to get the text from.</param>
    /// <returns>The text decoded from the byte array.</returns>
    internal static string GetText(this byte[] bytes)
    {
        var encoding = bytes.GetEncoding() ?? DefaultEncoding;
        return encoding.GetString(bytes);
    } // internal static string GetText (this byte[])

    /// <summary>
    /// Gets the encoding of the specified byte array.
    /// </summary>
    /// <param name="bytes">The byte array to get the encoding of.</param>
    /// <returns>The encoding of the byte array if it can be determined; otherwise, <see langword="null"/>.</returns>
    internal static Encoding? GetEncoding(this byte[] bytes)
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
    } // internal static Encoding? GetEncoding (this byte[])

    #region Parse

    internal static int ParseIntInvariant(this string s)
        => int.Parse(s, CultureInfo.InvariantCulture);

    internal static double ParseDoubleInvariant(this string s)
        => double.Parse(s, CultureInfo.InvariantCulture);

    #endregion Parse

    #region ToString

    internal static string ToInvariantString(this int value, string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    internal static string ToInvariantString(this decimal value, string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    internal static string ToInvariantString(this double value, string? format = null)
        => value.ToString(format, CultureInfo.InvariantCulture);

    #endregion ToString
} // internal static class TextUtils


// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace TAFitting.Controls;

/// <summary>
/// Represents a handler for the negative sign.
/// </summary>
/// <remarks>
/// The constructor of this class changes the negative sign to the specified sign,
/// or the hyphen-minus sign if no sign is specified.
/// This instance will restore the original negative sign when it is disposed.
/// </remarks>
/// <example>
/// This example shows how to temporarily change the negative sign to the hyphen-minus sign.
/// <code>
///     using var _ = new NegativeSignHandler();
///     // Some code that uses the hyphen-minus sign as the negative sign.
///     
///     // The negative sign is restored to the original sign when the using block is exited.
/// </code>
/// </example>
internal sealed partial class NegativeSignHandler : IDisposable
{
    /// <summary>
    /// Hyphen-minus sign (U+002D).
    /// </summary>
    internal const string HyphenMinus = "-";

    /// <summary>
    /// Minus sign (U+2212).
    /// </summary>
    internal const string MinusSign = "\u2212";

    private static readonly FieldInfo? negativeSign
        = typeof(NumberFormatInfo).GetField("_negativeSign", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly string originalSign;

    /// <summary>
    /// Gets the negative sign.
    /// </summary>
    internal static string NegativeSign => NumberFormatInfo.CurrentInfo.NegativeSign;

    /// <summary>
    /// Initializes a new instance of the <see cref="NegativeSignHandler"/> class.
    /// </summary>
    internal NegativeSignHandler() : this(HyphenMinus) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NegativeSignHandler"/> class.
    /// </summary>
    /// <param name="sign"></param>
    internal NegativeSignHandler(string sign)
    {
        this.originalSign = NumberFormatInfo.CurrentInfo.NegativeSign;
        ChangeNegativeSign(sign);
    } // internal NegativeSignHandler(string sign)

    public void Dispose()
        => ChangeNegativeSign(this.originalSign);

    /// <summary>
    /// Changes the negative sign.
    /// </summary>
    /// <param name="sign">The new negative sign.</param>
    internal static void ChangeNegativeSign(string sign)
    {
        if (negativeSign is null) return;
        negativeSign?.SetValue(NumberFormatInfo.CurrentInfo, sign);
    } // internal static void ChangeNegativeSign(string sign)

    /// <summary>
    /// Sets the negative sign to the hyphen-minus sign.
    /// </summary>
    internal static void SetHyphenMinus()
        => ChangeNegativeSign(HyphenMinus);

    /// <summary>
    /// Sets the negative sign to the minus sign.
    /// </summary>
    internal static void SetMinusSign()
        => ChangeNegativeSign(MinusSign);

    /// <summary>
    /// Converts the text to use the hyphen-minus sign.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>The converted text.</returns>
    [return: NotNullIfNotNull(nameof(text))]
    internal static string? ToHyphenMinus(string? text)
        => text?.Replace(MinusSign, HyphenMinus, StringComparison.Ordinal);

    /// <summary>
    /// Converts the text to use the minus sign.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>The converted text.</returns>
    [return: NotNullIfNotNull(nameof(text))]
    internal static string? ToMinusSign(string? text)
        => text?.Replace(HyphenMinus, MinusSign, StringComparison.Ordinal);

    /// <summary>
    /// Tries to parse the string as a double, considering both minus sign variants.
    /// </summary>
    /// <param name="s">The string to parse.</param>
    /// <param name="value">The parsed double value.</param>
    /// <returns><see langword="true"/> if parsing succeeded; otherwise, <see langword="false"/>.</returns>
    internal static bool TryParseDouble(string? s, out double value)
    {
        if (string.IsNullOrEmpty(s))
        {
            value = 0;
            return false;
        }

        if (double.TryParse(ToMinusSign(s), out value)) return true;
        if (double.TryParse(ToHyphenMinus(s), out value)) return true;
        return false;
    } // internal static bool TryParseDouble (string?, out double)

    internal static bool TryParseDouble(ReadOnlySpan<char> s, out double value)
    {
        if (s.IsEmpty)
        {
            value = 0;
            return false;
        }

        if (s[0] == '-')
        {
            if (double.TryParse(s, out value)) return true;
            var longMinus = (stackalloc char[s.Length]);
            longMinus[0] = '\u2212';
            s[1..].CopyTo(longMinus[1..]);
            return double.TryParse(longMinus, out value);
        }
        else if (s[0] == '\u2212')
        {
            if (double.TryParse(s, out value)) return true;
            var shortMinus = (stackalloc char[s.Length]);
            shortMinus[0] = '-';
            s[1..].CopyTo(shortMinus[1..]);
            return double.TryParse(shortMinus, out value);
        }
        else
        {
            return double.TryParse(s, out value);
        }
    } // internal static bool TryParseDouble (ReadOnlySpan<char>, out double)

    /// <summary>
    /// Tries to parse multiple strings as doubles, considering both minus sign variants.
    /// </summary>
    /// <param name="strings">The strings to parse.</param>
    /// <param name="values">The span to store the parsed double values.</param>
    /// <retursns><see langword="true"/> if all parsing succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException">The length of the <paramref name="values"/> span is less than that of the <paramref name="strings"/> span.</exception>
    internal static bool TryParseDoubles(ReadOnlySpan<string> strings, Span<double> values)
    {
        if (values.Length < strings.Length)
            throw new ArgumentException("The length of the values span is less than that of the strings span.");

        for (var i = 0; i < strings.Length; i++)
        {
            if (!TryParseDouble(strings[i], out var value))
                return false;
            values[i] = value;
        }
        return true;
    } // internal static bool TryParseDoubles (ReadOnlySpan<string>, Span<double>)

    /// <summary>
    /// Parses doubles from a delimited string.
    /// </summary>
    /// <param name="s">The input string.</param>
    /// <param name="separator">The character that separates the values.</param>
    /// <param name="values">A span to store the parsed double values.</param>
    /// <returns>The number of successfully parsed double values.</returns>
    internal static int ParseDoubles(ReadOnlySpan<char> s, char separator, Span<double> values)
    {
        var count = 0;
        var start = 0;
        for (var i = 0; i <= s.Length; i++)
        {
            if (i != s.Length && s[i] != separator)
                continue;

            var length = i - start;
            if (length > 0)
            {
                var segment = s.Slice(start, length);
                if (TryParseDouble(segment, out var value))
                {
                    if (count < values.Length)
                        values[count++] = value;
                    else
                        break;
                }
            }
            start = i + 1;
        }
        return count;
    } // internal static int ParseDoubles (ReadOnlySpan<char>, char, Span<double>)
} // internal sealed partial class NegativeSignHandler : IDisposable

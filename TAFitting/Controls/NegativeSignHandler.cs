
// (c) 2024 Kazuki KOHZUKI

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
    internal static string ToHyphenMinus(string text)
        => text.Replace(MinusSign, HyphenMinus, StringComparison.Ordinal);

    /// <summary>
    /// Converts the text to use the minus sign.
    /// </summary>
    /// <param name="text">The text to be converted.</param>
    /// <returns>The converted text.</returns>
    internal static string ToMinusSign(string text)
        => text.Replace(HyphenMinus, MinusSign, StringComparison.Ordinal);
} // internal sealed partial class NegativeSignHandler : IDisposable

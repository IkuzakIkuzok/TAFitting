
// (c) 2024 Kazuki KOHZUKI

using System.Globalization;
using System.Reflection;

namespace TAFitting.Controls;

internal sealed class NegativeSignHandler : IDisposable
{
    internal const string HyphenMinus = "-";
    internal const string MinusSign = "\u2212";

    private static readonly FieldInfo? negativeSign
        = typeof(NumberFormatInfo).GetField("_negativeSign", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly string originalSign;

    internal static string NegativeSign => NumberFormatInfo.CurrentInfo.NegativeSign;

    internal NegativeSignHandler() : this(HyphenMinus) { }

    internal NegativeSignHandler(string sign)
    {
        this.originalSign = NumberFormatInfo.CurrentInfo.NegativeSign;
        ChangeNegativeSign(sign);
    } // internal NegativeSignHandler(string sign)

    public void Dispose()
        => ChangeNegativeSign(this.originalSign);

    internal static void ChangeNegativeSign(string sign)
    {
        if (negativeSign is null) return;
        negativeSign?.SetValue(NumberFormatInfo.CurrentInfo, sign);
    } // internal static void ChangeNegativeSign(string sign)

    internal static void SetHyphenMinus()
        => ChangeNegativeSign(HyphenMinus);

    internal static void SetMinusSign()
        => ChangeNegativeSign(MinusSign);

    internal static string ToHyphenMinus(string text)
        => text.Replace(MinusSign, HyphenMinus);

    internal static string ToMinusSign(string text)
        => text.Replace(HyphenMinus, MinusSign);
} // internal sealed class NegativeSignHandler : IDisposable

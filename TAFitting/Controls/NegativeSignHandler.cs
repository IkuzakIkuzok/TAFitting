
// (c) 2024 Kazuki KOHZUKI

using System.Globalization;
using System.Reflection;

namespace TAFitting.Controls;

internal sealed class NegativeSignHandler : IDisposable
{
    private static readonly FieldInfo? negativeSign
        = typeof(NumberFormatInfo).GetField("_negativeSign", BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly string originalSign;

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
} // internal sealed class NegativeSignHandler : IDisposable

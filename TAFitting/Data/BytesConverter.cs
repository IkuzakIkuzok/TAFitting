
// (c) 2025 Kazuki Kohzuki

using System.Text;

namespace TAFitting.Data;

/// <summary>
/// Converts bytes to various types and vice versa.
/// The endian is automatically corrected to big-endian.
/// </summary>
internal static partial class BytesConverter
{
    private static readonly Encoding SystemEncoding;

    /// <summary>
    /// Retrieves the current Windows ANSI code page identifier for the operating system.
    /// </summary>
    /// <returns>The current Windows ANSI code page (ACP) identifier for the operating system.</returns>
    [LibraryImport("kernel32.dll")]
    private static partial int GetACP();

    static BytesConverter()
    {
        /*
         * It seems that the SurfaceXplorer app uses CP932 (Shift_JIS) as long as on my computer,
         * but it is hard to think that the code page is fixed.
         * Rather, it is better to assume that the code page is same as the system default encoding.
         * System.Text.Encoding.Default returns the system default encoding on .NET Framework,
         * but it always returns Encoding.UTF8 on .NET Core and .NET 5+.
         * Therefore, we need to use the Windows API to get the system default code page.
         */

        var provider = CodePagesEncodingProvider.Instance;
        Encoding.RegisterProvider(provider);

        var codePage = GetACP();
        SystemEncoding = Encoding.GetEncoding(codePage);
    } // cctor ()

    private static byte[] CorrectEndian(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return bytes;
    } // private static byte[] CorrectEndian (byte[])

    /// <summary>
    /// Converts the specified byte array to a 32-bit signed integer.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The 32-bit signed integer converted from the byte array.</returns>
    internal static int ToInt32(byte[] bytes)
        => BitConverter.ToInt32(CorrectEndian(bytes), 0);

    /// <summary>
    /// Converts the specified byte array to a 64-bit floating-point number.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The 64-bit floating-point number converted from the byte array.</returns>
    internal static double ToDouble(byte[] bytes)
        => BitConverter.ToDouble(CorrectEndian(bytes), 0);

    /// <summary>
    /// Converts the specified byte array to a string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The string converted from the byte array.</returns>
    internal static string ToString(byte[] bytes)
        => SystemEncoding.GetString(bytes).TrimEnd('\0');

    /// <summary>
    /// Converts the specified 32-bit signed integer to a byte array.
    /// </summary>
    /// <param name="value">The 32-bit signed integer to convert.</param>
    /// <returns>The byte array converted from the 32-bit signed integer.</returns>
    internal static byte[] ToBytes(int value)
        => CorrectEndian(BitConverter.GetBytes(value));

    /// <summary>
    /// Converts the specified 64-bit floating-point number to a byte array.
    /// </summary>
    /// <param name="value">The 64-bit floating-point number to convert.</param>
    /// <returns>The byte array converted from the 64-bit floating-point number.</returns>
    internal static byte[] ToBytes(double value)
        => CorrectEndian(BitConverter.GetBytes(value));

    /// <summary>
    /// Converts the specified string to a byte array.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The byte array converted from the string.</returns>
    internal static byte[] ToBytes(string value)
        => SystemEncoding.GetBytes(value);
} // internal static partial class BytesConverter
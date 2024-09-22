
// (c) 2024 Kazuki KOHZUKI

using System.Text;
using WinClipboard = System.Windows.Forms.Clipboard;

namespace TAFitting.Clipboard;

internal static class ClipboardHandler
{
    internal static IEnumerable<ClipboardRow> GetRowsFromClipboard(IEnumerable<string> parameters)
    {
        if (!WinClipboard.ContainsData(DataFormats.CommaSeparatedValue)) yield break;
        if (WinClipboard.GetData(DataFormats.CommaSeparatedValue) is not MemoryStream stream) yield break;

        var csv = stream.ToArray().ToText();
        var rows = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var header = rows.First().Split(',');
        var contents = rows.Skip(1).Select(row => row.Trim('\0').Split(','));
        
        var parameterIndices = parameters.Select(p => Array.IndexOf(header, p)).ToArray();
        if (parameterIndices.Any(i => i < 0)) yield break;

        foreach (var content in contents)
        {
            if (string.IsNullOrWhiteSpace(content[0])) continue;
            var wavelength = double.Parse(content[0]);
            var values = parameterIndices.Select(i => double.Parse(content[i])).ToArray();
            yield return new ClipboardRow(wavelength, values);
        }
    } // internal static IEnumerable<ClipboardRow> GetRowsFromClipboard (IEnumerable<string>)

    /// <summary>
    /// Converts the specified text to a byte array.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>UTF-8 encoded byte array.</returns>
    private static byte[] ToBytes(this string text)
        => Encoding.UTF8.GetBytes(text);

    /// <summary>
    /// Converts the specified byte array to a text.
    /// </summary>
    /// <param name="bytes">The byte array.</param>
    /// <returns>UTF-8 decoded text.</returns>
    private static string ToText(this byte[] bytes)
        => Encoding.UTF8.GetString(bytes);
} // internal static class ClipboardHandler


// (c) 2024 Kazuki KOHZUKI

using TAFitting.Controls;
using WinClipboard = System.Windows.Forms.Clipboard;

namespace TAFitting.Clipboard;

internal static class ClipboardHandler
{
    internal static IEnumerable<ClipboardRow> GetRowsFromClipboard(IEnumerable<string> parameters)
    {
        if (!WinClipboard.ContainsData(DataFormats.CommaSeparatedValue)) yield break;
        if (WinClipboard.GetData(DataFormats.CommaSeparatedValue) is not MemoryStream stream) yield break;

        var csv = stream.ToArray().GetText();
        var rows = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var header = rows.First().Split(',');
        var contents = rows.Skip(1).Select(row => row.Trim('\0').Split(','));
        
        var parameterIndices = parameters.Select(p => Array.IndexOf(header, p)).ToArray();
        if (parameterIndices.Any(i => i < 0)) yield break;

        using var _ = new NegativeSignHandler("-");

        foreach (var content in contents)
        {
            if (string.IsNullOrWhiteSpace(content[0])) continue;
            var wavelength = double.Parse(content[0]);
            var values = parameterIndices.Select(i => double.Parse(content[i])).ToArray();
            yield return new ClipboardRow(wavelength, values);
        }
    } // internal static IEnumerable<ClipboardRow> GetRowsFromClipboard (IEnumerable<string>)
} // internal static class ClipboardHandler


// (c) 2024 Kazuki KOHZUKI

using TAFitting.Controls;
using TAFitting.Data;
using WinClipboard = System.Windows.Forms.Clipboard;

namespace TAFitting.Clipboard;

internal static class ClipboardHandler
{
    internal static IEnumerable<ParameterValues> GetParameterValuesFromClipboard(IEnumerable<string> parameters)
    {
        if (!WinClipboard.TryGetData(DataFormats.UnicodeText, out string? csv)) yield break;

        var rows = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var header = rows.First().Split('\t');
        var contents = rows.Skip(1).Select(row => row.Trim('\0').Split('\t'));
        
        var parameterIndices = parameters.Select(p => Array.IndexOf(header, p)).ToArray();
        if (parameterIndices.Any(i => i < 0)) yield break;

        using var _ = new NegativeSignHandler();

        foreach (var content in contents)
        {
            if (string.IsNullOrWhiteSpace(content[0])) continue;
            var wavelength = content[0].ParseDoubleInvariant();
            var values = parameterIndices.Select(i => content[i].ParseDoubleInvariant()).ToArray();
            yield return new(wavelength, values);
        }
    } // internal static IEnumerable<ParameterValues> GetParameterValuesFromClipboard (IEnumerable<string>)
} // internal static class ClipboardHandler

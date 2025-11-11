
// (c) 2024 Kazuki KOHZUKI

using System.Text.RegularExpressions;

namespace TAFitting.Data;

/*
 * Gets filename based on the parent directory name and the format.
 *
 * The format can contain the following patterns:
 *   - <BASENAME>: The parent directory name.
 *   - <BASENAME|...>: The parent directory name with replacements.
 * 
 * Replacement format:
 *   - <pattern>/<replacement>: Replace the matched pattern with the replacement.
 *   - r:<pattern>/<replacement>: Replace the matched pattern with the replacement using regular expression.
 * 
 * Example:
 *   Format: "<BASENAME|ms/000us|r:[um]s/>"
 *   Basename: "1ms"
 *   Output: "1000"
 *   Note that the replacement patters can be chained and applied in order.
 *   <replacement> can be an empty string.
 */

/// <summary>
/// Handles the filename of data files.
/// </summary>
internal static partial class FileNameHandler
{
    /// <summary>
    /// Gets or sets the timeout for regular expression.
    /// </summary>
    internal static int RegexTimeoutMilliseconds { get; set; } = 500;

    private static readonly Regex re_basename = BasenamePattern();

    /// <summary>
    /// Gets the filename from the basename and the format.
    /// </summary>
    /// <param name="basename">The basename.</param>
    /// <param name="format">The format.</param>
    /// <returns>The filename from the basename and the format.</returns>
    internal static string GetFileName(string basename, string format)
    {
        var filename = format;
        try
        {
            var ms = re_basename.Matches(format);
            foreach (Match m in ms)
            {
                var pattern = m.Value;
                var s_basename = basename;

                if (pattern == "<BASENAME>")  // No need to modify the placeholder
                    goto ReplacePlaceholder;

                var replaces = pattern[10..^1].Split('|');
                foreach (var replace in replaces)
                    ModifyBasename(ref s_basename, replace);

                ReplacePlaceholder:
                filename = filename.Replace(pattern, s_basename, StringComparison.Ordinal);
            }  // foreach (Match m in ms)

            return filename;
        }
        catch
        {
            return basename;
        }
    } // internal static string GetFileName (string, string)

    private static void ModifyBasename(ref string basename, string replace)
    {
        if (!replace.StartsWith("r:", StringComparison.Ordinal))
        {
            // Simple string replacement
            var kv = replace.Split('/');
            basename = basename.Replace(kv[0], kv[1], StringComparison.Ordinal);
            return;
        }

        // Regular expression replacement
        var kv_r = replace[2..].Split('/');
        var timeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        try
        {
            var re = new Regex(kv_r[0], RegexOptions.None, timeout);
            basename = re.Replace(basename, kv_r[1]);
        }
        catch (RegexMatchTimeoutException)
        {
            basename = basename.Replace(kv_r[0], kv_r[1], StringComparison.Ordinal);
        }
    } // private static void ModifyBasename (ref string, string)

    [GeneratedRegex(@"<BASENAME(\|[^|/]+/[^|/]*)*>")]
    private static partial Regex BasenamePattern();
} // internal static class FileNameHandler

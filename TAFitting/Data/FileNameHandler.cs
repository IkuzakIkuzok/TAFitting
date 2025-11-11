
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
    /// <remarks>
    /// If the <paramref name="format"/> does not contain any sequential replacements or regular expression replacements,
    /// consider using <see cref="GetFileNameFastMode(string, string)"/> for better performance.
    /// </remarks>
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
        var span_r = replace.AsSpan()[2..];
        var sep = span_r.IndexOf('/');
        var oldPattern = span_r[..sep].ToString();
        var newPattern = span_r[(sep + 1)..].ToString();
        var timeout = TimeSpan.FromMilliseconds(RegexTimeoutMilliseconds);
        try
        {
            var re = new Regex(oldPattern, RegexOptions.None, timeout);
            basename = re.Replace(basename, newPattern);
        }
        catch (RegexMatchTimeoutException)
        {
            basename = basename.Replace(oldPattern, newPattern, StringComparison.Ordinal);
        }
    } // private static void ModifyBasename (ref string, string)

    /// <summary>
    /// Gets the filename from the basename and the format using a fast method.
    /// </summary>
    /// <param name="basename">The basename.</param>
    /// <param name="format">The format.</param>
    /// <returns>The filename from the basename and the format.</returns>
    /// <remarks>
    /// This method only supports a simple string replacements in the format '<BASENAME|old/new>'.
    /// Sequential replacements and regular expression replacements are not supported.
    /// </remarks>
    internal static string GetFileNameFastMode(string basename, string format)
    {
        var format_span = format.AsSpan();
        var idx_placeholder_begin = format_span.IndexOf("<BASENAME", StringComparison.Ordinal);
        if (idx_placeholder_begin < 0)
            return format;  // No placeholder found, return the format as is.
        var idx_placeholder_end = format_span.IndexOf('>');

        var basename_span = basename.AsSpan();

        var replace_begin = idx_placeholder_begin + 10;
        var replace_end = idx_placeholder_end;
        if (replace_begin >= replace_end)
            goto SimpleBasename;

        var replace = format_span[replace_begin..replace_end];
        var sep = replace.IndexOf('/');
        if (sep < 0)
            goto SimpleBasename;

        var oldSpan = replace[..sep];
        if (oldSpan.IsEmpty)
            goto SimpleBasename;
        var newSpan = replace[(sep + 1)..];
        sep = basename_span.IndexOf(oldSpan, StringComparison.Ordinal);
        if (sep >= 0)
            goto ReplaceBasename;

    SimpleBasename:
        // Simply replace the placeholder ('<BASENAME>') with the basename
        var newBasenameLen = basename_span.Length;
        var newBasename = (stackalloc char[newBasenameLen]);
        basename_span.CopyTo(newBasename);
        goto ReplacePlaceholder;

    ReplaceBasename:
        // Replace the placeholder ('<BASENAME|...>') with the modified basename
        newBasenameLen = basename_span.Length - oldSpan.Length + newSpan.Length;
        newBasename = (stackalloc char[newBasenameLen]);
        basename_span[..sep].CopyTo(newBasename);
        newSpan.CopyTo(newBasename[sep..]);
        basename_span[(sep + oldSpan.Length)..].CopyTo(newBasename[(sep + newSpan.Length)..]);

    ReplacePlaceholder:
        var newLen = format_span.Length - (idx_placeholder_end - idx_placeholder_begin + 1) + newBasenameLen;
        var dst = (stackalloc char[newLen]);
        format_span[..idx_placeholder_begin].CopyTo(dst);
        newBasename.CopyTo(dst[idx_placeholder_begin..]);
        format_span[(idx_placeholder_end + 1)..].CopyTo(dst[(idx_placeholder_begin + newBasenameLen)..]);
        return new(dst);
    } // internal static string GetFileNameFastMode (string, string)

    internal static bool IsSimpleFormat(string format)
    {
        var span = format.AsSpan();
        var placeholders = span.Count("<BASENAME");
        if (placeholders == 0) return true;  // No placeholders, simple.
        if (placeholders > 1) return false;  // Multiple placeholders, complex.
        
        var idx_placeholder_begin = span.IndexOf("<BASENAME", StringComparison.Ordinal);
        var idx_placeholder_end = span.IndexOf('>');

        var replace_begin = idx_placeholder_begin + 10;
        var replace_end = idx_placeholder_end;

        if (replace_begin >= replace_end)
            return true;  // No replacements, simple.

        var replace = span[replace_begin..replace_end];
        var separators = replace.Count('|');
        if (separators > 1)
            return false;  // Multiple replacements, complex.

        if (replace.StartsWith("r:", StringComparison.Ordinal))
            return false;  // Regular expression replacement, complex.

        return true;  // Simple string replacement, simple.
    } // internal static bool IsSimpleFormat (string)

    [GeneratedRegex(@"<BASENAME(\|[^|/]+/[^|/]*)*>")]
    private static partial Regex BasenamePattern();
} // internal static class FileNameHandler

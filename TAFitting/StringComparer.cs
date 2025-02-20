﻿
// (c) 2024 Kazuki KOHZUKI

using System.Text.RegularExpressions;

namespace TAFitting;

/// <summary>
/// Provides a string comparer for natural sorting.
/// </summary>
internal sealed partial class StringComparer : IComparer<string>
{
    private static readonly Regex re_textNum = NamePartsPattern();

    private static readonly StringComparer _instance = new();

    /// <summary>
    /// Gets the instance of the <see cref="StringComparer"/> class.
    /// </summary>
    internal static StringComparer Instance => _instance;

    /// <inheritdoc/>
    // Only use supported StringComparison values.
    // ExceptionAdjustment: M:System.String.Compare(System.String,System.String,System.StringComparison) -T:System.NotSupportedException
    public int Compare(string? s1, string? s2)
    {
        if (s1 == s2) return 0;
        if (s1 == null) return -1;
        if (s2 == null) return 1;

        var parts1 = re_textNum.Matches(s1).Select(m => m.Value);
        var parts2 = re_textNum.Matches(s2).Select(m => m.Value);

        var sr = 0;
        foreach ((var part1, var part2) in parts1.Zip(parts2))
        {
            if (part1 == part2) continue;
            if (double.TryParse(part1, out var num1) && double.TryParse(part2, out var num2))
                sr = num1.CompareTo(num2);
            else
                sr = string.Compare(part1, part2, StringComparison.InvariantCulture);

            if (sr != 0) return sr;
        }

        return 0;
    } // public int Compare (s1, s2)

    private StringComparer() { }

    [GeneratedRegex(@"(\D+|\d+(\.\d+)?)")]
    private static partial Regex NamePartsPattern();
} // internal sealed partial class StringComparer : IComparer<string>

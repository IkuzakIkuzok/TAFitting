
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics;

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a range of masking.
/// </summary>
/// <param name="Start">The start of the masking range.</param>
/// <param name="End">The end of the masking range.</param>
[DebuggerDisplay("{Start}-{End}")]
internal readonly record struct MaskingRange(double Start, double End)
{
    /// <summary>
    /// Gets an empty masking range.
    /// </summary>
    internal static MaskingRange Empty { get; } = new(double.NaN, double.NaN);

    /// <summary>
    /// Gets the start of the masking range.
    /// </summary>
    internal double Start { get; } = Start;

    /// <summary>
    /// Gets the end of the masking range.
    /// </summary>
    internal double End { get; } = End;
    
    /// <summary>
    /// Gets a value indicating whether this masking range is empty.
    /// </summary>
    internal bool IsEmpty
        => double.IsNaN(this.Start) || double.IsNaN(this.End);

    /// <summary>
    /// Checks whether the specified value is included in this masking range.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns><see langword="true"/> if the <paramref name="value"/> is included in this masking range; otherwise, <see langword="false"/>.</returns>
    internal bool Includes(double value)
        => this.Start <= value && value <= this.End;

    /// <summary>
    /// Parses a character span representing a numeric value or a range and returns the corresponding <see cref="MaskingRange"/> instance.
    /// </summary>
    /// <remarks>If the input contains a hyphen ('-'), it is interpreted as a range with start and end values.
    /// Otherwise, the input is treated as a single value, and the range will have identical start and end values.</remarks>
    /// <param name="value">A read-only span of characters containing either a single numeric value or a range in the format "start-end". 
    /// Leading and trailing whitespace is ignored for each value.</param>
    /// <returns>A <see cref="MaskingRange"/> representing the parsed value or range.
    /// Returns <see cref="MaskingRange.Empty"/> if the input cannot be parsed as a valid number or range.</returns>
    internal static MaskingRange FromSpan(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return Empty;

        var hyphenIndex = value.IndexOf('-');
        if (hyphenIndex < 0) // single value
        {
            if (!double.TryParse(value, out var time)) return Empty;
            return new(time, time);
        }

        // range value
        var startSpan = value[..hyphenIndex].Trim();
        var endSpan = value[(hyphenIndex + 1)..].Trim();
        if (!double.TryParse(startSpan, out var start)) return Empty;
        if (!double.TryParse(endSpan, out var end)) return Empty;
        return new(start, end);
    } // internal static MaskingRange FromSpan (ReadOnlySpan<char>)
} // internal readonly record struct MaskingRange (double, double)

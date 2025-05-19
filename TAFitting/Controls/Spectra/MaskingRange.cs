
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
    internal static MaskingRange Empty { get; } = new MaskingRange(double.NaN, double.NaN);

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
    /// Converts the string representation of a masking range to its equivalent masking range.
    /// </summary>
    /// <param name="value">The string representation of the masking range.</param>
    /// <returns>The equivalent masking range.</returns>
    internal static MaskingRange FromString(string value)
    {
        var values = value.Trim().Split('-');

        if (values.Length == 1)
        {
            if (!double.TryParse(values[0], out var time)) return Empty;
            return new(time, time);
        }
        if (values.Length != 2) return Empty;

        if (!double.TryParse(values[0], out var start)) return Empty;
        if (!double.TryParse(values[1], out var end)) return Empty;
        if (start > end) return new(end, start);
        return new(start, end);
    } // internal static MaskingRange FromString (string)
} // internal readonly record struct MaskingRange (double, double)

﻿
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics;

namespace TAFitting.Controls.Spectra;

[DebuggerDisplay("{Start}-{End}")]
internal readonly record struct MaskingRange(double Start, double End)
{
    internal static MaskingRange Empty { get; } = new MaskingRange(double.NaN, double.NaN);

    internal double Start { get; } = Start;

    internal double End { get; } = End;
    
    internal bool IsEmpty
        => double.IsNaN(this.Start) || double.IsNaN(this.End);

    internal bool Includes(double value)
        => this.Start <= value && value <= this.End;

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

﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a text box for masking ranges.
/// </summary>
internal partial class MaskingRangeBox : DelayedTextBox
{
    /// <summary>
    /// Gets the masking ranges.
    /// </summary>
    internal MaskingRanges MaskingRanges
        => new(this.Text);
} // internal partial class MaskingRangeBox : DelayedTextBox

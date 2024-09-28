
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls.Spectra;
internal class MaskingRangeBox : DelayedTextBox
{
    internal MaskingRanges MaskingRanges
        => new(this.Text);
} // internal class MaskingRangeBox : DelayedTextBox

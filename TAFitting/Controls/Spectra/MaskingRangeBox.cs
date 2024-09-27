
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls.Spectra;
internal class MaskingRangeBox : DelayedTextBox
{
    internal IEnumerable<MaskingRange> MaskingRanges
        => this.Text.Split(',')
                    .Select(MaskingRange.FromString)
                    .Where(r => !r.IsEmpty);
} // internal class MaskingRangeBox : DelayedTextBox

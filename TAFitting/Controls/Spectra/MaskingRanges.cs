
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Controls.Spectra;

internal sealed class MaskingRanges : IEnumerable<MaskingRange>
{
    private readonly HashSet<MaskingRange> _maskingRanges;

    internal MaskingRanges(string ranges)
    {
        this._maskingRanges = new(
            ranges.Split(',')
                  .Select(MaskingRange.FromString)
                  .Where(r => !r.IsEmpty)
        );
    } // ctor (string)

    public IEnumerator<MaskingRange> GetEnumerator()
        => ((IEnumerable<MaskingRange>)this._maskingRanges).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)this._maskingRanges).GetEnumerator();

    internal IEnumerable<double> GetMaskedPoints(IEnumerable<double> points)
        => points.Where(p => this.Any(r => r.Includes(p))).ToArray();

    internal IEnumerable<double> GetNextOfMaskedPoints(IEnumerable<double> points)
        => this.Select(r => r.End)
               .Select(p => points.SkipWhile(w => w < p).FirstOrDefault(double.NaN))
               .Where(p => !double.IsNaN(p))
               .ToArray();
} // internal sealed class MaskingRanges : IEnumerable<MaskingRange>

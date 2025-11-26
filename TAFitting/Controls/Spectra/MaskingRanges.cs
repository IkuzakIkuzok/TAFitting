
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a collection of masking ranges.
/// </summary>
internal sealed partial class MaskingRanges : IEnumerable<MaskingRange>
{
    private readonly HashSet<MaskingRange> _maskingRanges;

    /// <summary>
    /// Gets the source string representation of the masking ranges.
    /// </summary>
    internal string SourceString { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaskingRanges"/> class.
    /// </summary>
    /// <param name="ranges">The string representation of masking ranges.</param>
    internal MaskingRanges(string ranges)
    {
        this.SourceString = ranges;
        var span = ranges.AsSpan();
        var count = span.Count(',') + 1;
        this._maskingRanges = new(count);

        var start = 0;
        var end = 0;
        while (end < span.Length)
        {
            if (span[end] == ',')
            {
                var rangeSpan = span[start..end].Trim();
                var range = MaskingRange.FromSpan(rangeSpan);
                if (!range.IsEmpty)
                    this._maskingRanges.Add(range);
                start = end + 1;
            }
            end++;
        }
        // Last range
        var lastRangeSpan = span[start..end].Trim();
        var lastRange = MaskingRange.FromSpan(lastRangeSpan);
        if (!lastRange.IsEmpty)
            this._maskingRanges.Add(lastRange);
    } // ctor (string)

    /// <inheritdoc/>
    public IEnumerator<MaskingRange> GetEnumerator()
        => ((IEnumerable<MaskingRange>)this._maskingRanges).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)this._maskingRanges).GetEnumerator();

    private static bool CheckIncluded(MaskingRange range, double point)
        => range.Includes(point);

    /// <summary>
    /// Determines whether the specified point is contained within any of the masking ranges in the collection.
    /// </summary>
    /// <param name="point">The value to test for inclusion within the masking ranges.</param>
    /// <returns><see langword="true"/> if the point is included in at least one masking range; otherwise, <see langword="false"/>.</returns>
    internal bool Include(double point)
        => this._maskingRanges.Any(CheckIncluded, point);

    /// <summary>
    /// Gets the masked points.
    /// </summary>
    /// <param name="points">The points to be masked.</param>
    /// <returns>The masked points.</returns>
    internal IEnumerable<double> GetMaskedPoints(IEnumerable<double> points)
    {
        if (!this.Any()) return [];
        return points.Where(Include);
    } // internal IEnumerable<double> GetMaskedPoints (IEnumerable<double>)

    /// <summary>
    /// Gets the next points of the masked points.
    /// </summary>
    /// <param name="points">The points to be masked.</param>
    /// <returns>The next points of the masked points.</returns>
    internal IEnumerable<double> GetNextOfMaskedPoints(IEnumerable<double> points)
        => this.Select(r => r.End)
               .Select(p => points.SkipWhile(w => w < p).FirstOrDefault(double.NaN))
               .Where(p => !double.IsNaN(p));
} // internal sealed partial class MaskingRanges : IEnumerable<MaskingRange>

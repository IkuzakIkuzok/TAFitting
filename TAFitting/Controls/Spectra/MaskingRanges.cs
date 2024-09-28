
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a collection of masking ranges.
/// </summary>
internal sealed class MaskingRanges : IEnumerable<MaskingRange>
{
    private readonly HashSet<MaskingRange> _maskingRanges;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaskingRanges"/> class.
    /// </summary>
    /// <param name="ranges">The string representation of masking ranges.</param>
    internal MaskingRanges(string ranges)
    {
        this._maskingRanges = new(
            ranges.Split(',')
                  .Select(MaskingRange.FromString)
                  .Where(r => !r.IsEmpty)
        );
    } // ctor (string)

    /// <inheritdoc/>
    public IEnumerator<MaskingRange> GetEnumerator()
        => ((IEnumerable<MaskingRange>)this._maskingRanges).GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable)this._maskingRanges).GetEnumerator();

    /// <summary>
    /// Gets the masked points.
    /// </summary>
    /// <param name="points">The points to be masked.</param>
    /// <returns>The masked points.</returns>
    internal IEnumerable<double> GetMaskedPoints(IEnumerable<double> points)
        => points.Where(p => this.Any(r => r.Includes(p))).ToArray();

    /// <summary>
    /// Gets the next points of the masked points.
    /// </summary>
    /// <param name="points">The points to be masked.</param>
    /// <returns>The next points of the masked points.</returns>
    internal IEnumerable<double> GetNextOfMaskedPoints(IEnumerable<double> points)
        => this.Select(r => r.End)
               .Select(p => points.SkipWhile(w => w < p).FirstOrDefault(double.NaN))
               .Where(p => !double.IsNaN(p))
               .ToArray();
} // internal sealed class MaskingRanges : IEnumerable<MaskingRange>

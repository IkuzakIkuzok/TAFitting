
// (c) 2026 Kazuki KOHZUKI

using System.Buffers;
using System.Runtime.CompilerServices;

namespace TAFitting.Excel.Formulas;

/// <summary>
/// Represents a mutable, stack-allocated collection of Excel formula segments used to efficiently build or process formula template.
/// </summary>
internal ref struct TemplateSegmentList
{
    private Span<TemplateSegment> _segments;
    private TemplateSegment[]? _pooled;
    private int _position;

    private int _constLength;
    private int _paramPlaceholderCount;
    private int _timePlaceholderCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSegmentList"/> class using the specified buffer of formula segments.
    /// </summary>
    /// <param name="initialBuffer">A span containing the initial segments to be used for the list.
    /// The span must remain valid for the lifetime of the <see cref="TemplateSegmentList"/> instance.</param>
    internal TemplateSegmentList(Span<TemplateSegment> initialBuffer)
    {
        this._segments = initialBuffer;
        this._pooled = null;
        this._position = 0;

        this._constLength = 0;
        this._paramPlaceholderCount = 0;
        this._timePlaceholderCount = 0;
    } // ctor (Span<TemplateSegment>)

    /// <summary>
    /// Gets the number of items currently contained in the collection.
    /// </summary>
    internal readonly int Count => this._position;

    /// <summary>
    /// Gets the total length of constant segments in the collection.
    /// </summary>
    internal readonly int ConstantLength => this._constLength;

    /// <summary>
    /// Gets the number of parameter placeholders present in the format string.
    /// </summary>
    internal readonly int ParameterPlaceholderCount => this._paramPlaceholderCount;

    /// <summary>
    /// Gets the number of time placeholders present in the format string.
    /// </summary>
    internal readonly int TimePlaceholderCount => this._timePlaceholderCount;

    /// <summary>
    /// Adds the specified formula segment to the collection if it is not empty.
    /// </summary>
    /// <param name="segment">The formula segment to add.</param>
    internal void Add(TemplateSegment segment)
    {
        if (segment.IsEmpty) return;

        if (this._position >= this._segments.Length)
            Grow();

        this._segments[this._position++] = segment;

        this._constLength += segment.GetConstLength();
        switch (segment.Type)
        {
            case TemplateSegmentType.ParameterPlaceholder:
                this._paramPlaceholderCount++;
                break;
            case TemplateSegmentType.TimePlaceholder:
                this._timePlaceholderCount++;
                break;
        }
    } // internal void Add (TemplateSegment)

    // Do not inline to keep the hot path small
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        var newSize = this._segments.Length == 0 ? 32 : this._segments.Length << 1;

        var newArray = ArrayPool<TemplateSegment>.Shared.Rent(newSize);
        this._segments.CopyTo(newArray);

        if (this._pooled is not null)
            ArrayPool<TemplateSegment>.Shared.Return(this._pooled);

        this._segments = newArray;
        this._pooled = newArray;
    } // private void Grow ()

    /// <summary>
    /// Returns an array containing all segments currently stored in the buffer.
    /// </summary>
    /// <returns>An array of <see cref="TemplateSegment"/> objects representing the segments in the buffer.
    /// The array will be empty if no segments are present.</returns>
    internal readonly TemplateSegment[] ToArray()
    {
        if (this._position == 0) return [];
        var arr = new TemplateSegment[this._position];
        this._segments[..this._position].CopyTo(arr);
        return arr;
    } // internal readonly TemplateSegment[] ToArray ()

    public void Dispose()
    {
        if (this._pooled is not null)
            ArrayPool<TemplateSegment>.Shared.Return(this._pooled);

        this._pooled = null;
        this._segments = default;
    } // public void Dispose ()
} // internal ref struct TemplateSegmentList

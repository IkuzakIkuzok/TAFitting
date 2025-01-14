
// (c) 2025 Kazuki Kohzuki

using System.Collections;
using System.Runtime.CompilerServices;

namespace TAFitting.Print;

/// <summary>
/// Represents a collection of additional contents in a <see cref="SpectraSummaryDocument"/>.
/// </summary>
[CollectionBuilder(typeof(AdditionalContentCollection), nameof(Create))]
internal class AdditionalContentCollection : IEnumerable<AdditionalContent>
{
    private readonly List<AdditionalContent> _contents = [];

    /// <summary>
    /// Gets or sets the mode for handling the overlap of additional contents.
    /// </summary>
    internal AdditionalContentOverlapMode OverlapMode { get; set; } = AdditionalContentOverlapMode.Overwrite;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalContentCollection"/> class.
    /// </summary>
    internal AdditionalContentCollection() : this([]) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdditionalContentCollection"/> class
    /// with the specified additional contents.
    /// </summary>
    /// <param name="contents">The additional contents to include in the collection.</param>
    internal AdditionalContentCollection(ReadOnlySpan<AdditionalContent> contents)
    {
        this._contents.AddRange(contents);
    } // ctor (ReadOnlySpan<AdditionalContent>)

    /// <summary>
    /// Adds the specified additional content to the collection.
    /// </summary>
    /// <param name="content">The additional content to add.</param>
    /// <exception cref="InvalidOperationException">
    /// The content overlaps with an existing content and the <see cref="OverlapMode"/> is set to <see cref="AdditionalContentOverlapMode.Error"/>.
    /// </exception>
    internal void Add(AdditionalContent content)
    {
        switch (this.OverlapMode)
        {
            case AdditionalContentOverlapMode.AllowOverlap:
                break;
            case AdditionalContentOverlapMode.Error:
                if (this._contents.Any(c => c.Position == content.Position))
                    throw new InvalidOperationException("Content overlaps.");
                break;
            case AdditionalContentOverlapMode.Overwrite:
                this._contents.RemoveAll(c => c.Position == content.Position);
                break;
            case AdditionalContentOverlapMode.Ignore:
                if (this._contents.Any(c => c.Position == content.Position))
                    return;
                break;
        }
        this._contents.Add(content);
    } // Add (AdditionalContent)

    /// <summary>
    /// Creates a new instance of the <see cref="AdditionalContentCollection"/> class
    /// with the specified additional contents.
    /// </summary>
    /// <param name="contents">The additional contents to include in the collection.</param>
    /// <returns>
    /// A new instance of the <see cref="AdditionalContentCollection"/> class with the specified additional contents.
    /// </returns>
    internal static AdditionalContentCollection Create(ReadOnlySpan<AdditionalContent> contents)
        => new(contents);

    public IEnumerator<AdditionalContent> GetEnumerator()
        => this._contents.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this._contents.GetEnumerator();
} // internal class AdditionalContentCollection : IEnumerable<AdditionalContent>

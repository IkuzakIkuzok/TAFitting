
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Origin;

/// <summary>
/// Wraps a worksheet collection.
/// </summary>
internal partial class Worksheets : IReadOnlyList<Worksheet>
{
    private readonly dynamic _worksheets;
    private readonly List<Worksheet> worksheets;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worksheets"/> class with the specified worksheets.
    /// </summary>
    /// <param name="worksheets">The Origin worksheets object.</param>
    internal Worksheets(dynamic worksheets)
    {
        this._worksheets = worksheets;
        this.worksheets = [];
        for (var i = 0; i < worksheets.Count; i++)
            this.worksheets.Add(new(worksheets[i]));
    } // ctor (dynamic)

    public Worksheet this[int index] => this.worksheets[index];

    public int Count => this.worksheets.Count;

    public IEnumerator<Worksheet> GetEnumerator()
        => this.worksheets.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.worksheets.GetEnumerator();
} // internal partial class Worksheets : IReadOnlyList<Worksheet>

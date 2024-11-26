
// (c) 2024 Kazuki KOHZUKI

using System.Collections;

namespace TAFitting.Origin;

/// <summary>
/// A collection of <see cref="Layer"/> objects.
/// </summary>
internal partial class Layers : IReadOnlyList<Layer>
{
    private readonly dynamic _layers;
    private readonly List<Layer> layers;

    /// <summary>
    /// Initializes a new instance of the <see cref="Layers"/> class with the specified layers.
    /// </summary>
    /// <param name="layers">The Origin layers object.</param>
    internal Layers(dynamic layers)
    {
        this._layers = layers;
        this.layers = [];
        for (var i = 0; i < layers.Count; i++)
            this.layers.Add(new(layers[i]));
    } // ctor (dynamic)

    public Layer this[int index] => this.layers[index];

    public int Count => this.layers.Count;

    public IEnumerator<Layer> GetEnumerator()
        => this.layers.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => this.layers.GetEnumerator();
} // internal partial class Layers : IReadOnlyList<Layer>

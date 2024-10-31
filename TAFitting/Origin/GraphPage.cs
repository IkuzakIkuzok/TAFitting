
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents a graph page in Origin.
/// </summary>
internal class GraphPage
{
    private readonly dynamic graph;
    private readonly Layers layers;

    /// <summary>
    /// Gets or sets the name of the graph page.
    /// </summary>
    internal string Name
    {
        get => this.graph.Name;
        set => this.graph.Name = value;
    }

    /// <summary>
    /// Gets the layers of the graph page.
    /// </summary>
    internal Layers Layers => this.layers;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphPage"/> class with the specified graph page.
    /// </summary>
    /// <param name="graph">The Origin graph page object.</param>
    internal GraphPage(dynamic graph)
    {
        this.graph = graph;
        this.layers = new(this.graph.Layers);
    } // ctor (dynamic)
} // internal class GraphPage

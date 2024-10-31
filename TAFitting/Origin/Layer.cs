
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Origin;

/// <summary>
/// Represents a layer in Origin.
/// </summary>
internal class Layer
{
    private readonly dynamic layer;
    private readonly DataPlots dataPlots;

    /// <summary>
    /// Gets or sets the name of the layer.
    /// </summary>
    internal string Name
    {
        get => this.layer.Name;
        set => this.layer.Name = value;
    }

    /// <summary>
    /// Gets the data plots of the layer.
    /// </summary>
    internal DataPlots DataPlots => this.dataPlots;

    /// <summary>
    /// Initializes a new instance of the <see cref="Layer"/> class with the specified layer.
    /// </summary>
    /// <param name="layer">The Origin layer object.</param>
    internal Layer(dynamic layer)
    {
        this.layer = layer;
        this.dataPlots = new(this.layer.DataPlots);
    } // ctor (dynamic)
} // internal class Layer

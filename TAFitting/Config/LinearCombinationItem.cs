
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;
using TAFitting.Model;

namespace TAFitting.Config;

/// <summary>
/// Represents a linear combination item.
/// </summary>
[Serializable]
public sealed class LinearCombinationItem
{
    /// <summary>
    /// Gets or sets the GUID.
    /// </summary>
    [XmlAttribute("guid")]
    public Guid Guid { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    [XmlElement("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    [XmlElement("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the components.
    /// </summary>
    [XmlArray("components")]
    [XmlArrayItem("model")]
    public Guid[] Components { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearCombinationItem"/> class.
    /// </summary>
    public LinearCombinationItem() { }

    /// <summary>
    /// Registers the linear combination model.
    /// </summary>
    internal void Register()
    {
        var model = new LinearCombinationModel(this.Name, this.Components);
        ModelManager.AddModel(this.Guid, model, this.Category);
    } // internal void Register()
} // public sealed class LinearCombinationItem

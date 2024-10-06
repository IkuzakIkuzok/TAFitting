
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the model configuration.
/// </summary>
[Serializable]
public sealed class ModelConfig
{
    /// <summary>
    /// Gets or sets the default model.
    /// </summary>
    [XmlElement("default")]
    public Guid DefaultModel { get; set; } = Guid.Empty;

    /// <summary>
    /// Gets or sets the linear combination items.
    /// </summary>
    [XmlArray("linear-combinations")]
    [XmlArrayItem("linear-combination")]
    public List<LinearCombinationItem> LinearCombinations { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelConfig"/> class.
    /// </summary>
    public ModelConfig() { }
} // public sealed class ModelConfig

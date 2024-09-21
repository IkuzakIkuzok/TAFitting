
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the color gradient configuration.
/// </summary>
[Serializable]
public sealed class ColorGradientConfig
{
    /// <summary>
    /// Gets or sets the start color of the gradient.
    /// </summary>
    [XmlElement("start")]
    public SerializableColor StartColor { get; set; } = Color.Red;

    /// <summary>
    /// Gets or sets the end color of the gradient.
    /// </summary>
    [XmlElement("end")]
    public SerializableColor EndColor { get; set; } = Color.Blue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorGradientConfig"/> class.
    /// </summary>
    public ColorGradientConfig() { }
} // public sealed class ColorGradientConfig

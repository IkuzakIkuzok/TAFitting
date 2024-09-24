
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the appearance configuration of the spectra.
/// </summary>
[Serializable]
public sealed class SpectraAppearanceConfig
{
    /// <summary>
    /// Gets or sets the color gradient configuration.
    /// </summary>
    [XmlElement("color-gradient")]
    public ColorGradientConfig ColorGradientConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the color of the observed data.
    /// </summary>
    [XmlElement("line-width")]
    public int LineWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the size of the spectra marker.
    /// </summary>
    [XmlElement("marker-size")]
    public int MarkerSize { get; set; } = 7;
} // public sealed class Spectra


// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the appearance configuration.
/// </summary>
[Serializable]
public sealed class AppearanceConfig
{
    /// <summary>
    /// Gets or sets the color of the observed data.
    /// </summary>
    [XmlElement("observed-color")]
    public SerializableColor ObservedColor { get; set; } = Color.Gray;

    /// <summary>
    /// Gets or sets the color of the filtered data.
    /// </summary>
    [XmlElement("filtered-color")]
    public SerializableColor FilteredColor { get; set; } = Color.Blue;

    /// <summary>
    /// Gets or sets the color of the fit lines.
    /// </summary>
    [XmlElement("fit-color")]
    public SerializableColor FitColor { get; set; } = Color.Red;

    /// <summary>
    /// Gets or sets the line width of the fit lines.
    /// </summary>
    [XmlElement("fit-width")]
    public int FitWidth { get; set; } = 3;

    /// <summary>
    /// Gets or sets the font of the axis labels.
    /// </summary>
    [XmlElement("axis-label")]
    public FontConfig AxisLabelFont { get; set; } = new();

    /// <summary>
    /// Gets or sets the font of the axis title.
    /// </summary>
    [XmlElement("axis-title")]
    public FontConfig AxisTitleFont { get; set; } = new();

    /// <summary>
    /// Gets or sets the appearance configuration of the spectra.
    /// </summary>
    [XmlElement("spectra")]
    public SpectraAppearanceConfig Spectra { get; set; } = new();

    /// <summary>
    /// Gets or sets the R-squared thresholds.
    /// </summary>
    [XmlArray("r-squared")]
    [XmlArrayItem("threshold")]
    public RSquaredThresholdItem[] RSquaredThresholds { get; set; } = [
        new(0.5, Color.LightGreen),
        new(0.0, Color.LightYellow),
        new(double.NegativeInfinity, Color.LightPink)
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="AppearanceConfig"/> class.
    /// </summary>
    public AppearanceConfig() { }
} // public sealed class AppearanceConfig

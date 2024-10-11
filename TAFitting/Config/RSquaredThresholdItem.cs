
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;
using DColor = System.Drawing.Color;

namespace TAFitting.Config;

/// <summary>
/// Represents the R-squared threshold item.
/// </summary>
[Serializable]
public sealed class RSquaredThresholdItem
{
    /// <summary>
    /// Gets or sets the threshold value.
    /// </summary>
    [XmlAttribute("value")]
    public double Threshold { get; set; }

    /// <summary>
    /// Gets or sets the color of the threshold line.
    /// </summary>
    [XmlElement("color")]
    public SerializableColor Color { get; set; } = DColor.White;

    /// <summary>
    /// Initializes a new instance of the <see cref="RSquaredThresholdItem"/> class.
    /// </summary>
    public RSquaredThresholdItem() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RSquaredThresholdItem"/> class
    /// with the specified threshold and color.
    /// </summary>
    /// <param name="threshold">The threshold value.</param>
    /// <param name="color">The color of the threshold line.</param>
    internal RSquaredThresholdItem(double threshold, DColor color)
    {
        this.Threshold = threshold;
        this.Color = color;
    } // ctor (double, DColor)
} // public sealed class RSquaredThresholdItem

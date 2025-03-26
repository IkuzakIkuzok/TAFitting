
// (c) 2025 Kazuki Kohzuki

using System.Xml.Serialization;
using TAFitting.Controls.Analyzers;

namespace TAFitting.Config;

/// <summary>
/// Represents the analyzer configuration.
/// </summary>
[Serializable]
public sealed class AnalyzerConfig
{
    /// <summary>
    /// Gets or sets the default Fourier spectrum type.
    /// </summary>
    [XmlElement("default-fourier-spectrum")]
    public FourierSpectrumType DefaultFourierSpectrum { get; set; } = FourierSpectrumType.AmplitudeSpectrum;

    /// <summary>
    /// Gets or sets the color of the line in the analyzer.
    /// </summary>
    public SerializableColor LineColor { get; set; } = Color.Blue;

    /// <summary>
    /// Gets or sets the line width of the spectra.
    /// </summary>
    [XmlElement("line-width")]
    public int LineWidth { get; set; } = 2;

    /// <summary>
    /// Gets or sets the size of the spectra marker.
    /// </summary>
    [XmlElement("marker-size")]
    public int MarkerSize { get; set; } = 7;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyzerConfig"/> class.
    /// </summary>
    public AnalyzerConfig() { }
} // public sealed class AnalyzerConfig

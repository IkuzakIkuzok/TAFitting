
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
    /// Initializes a new instance of the <see cref="AnalyzerConfig"/> class.
    /// </summary>
    public AnalyzerConfig() { }
} // public sealed class AnalyzerConfig

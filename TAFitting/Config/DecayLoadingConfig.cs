
// (c) 2024 Kazuki Kohzuki

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the decay loading configuration.
/// </summary>
[Serializable]
public sealed class DecayLoadingConfig
{
    /// <summary>
    /// Gets or sets the filename format of the A-B signal.
    /// </summary>
    [XmlElement("format-a-b")]
    public string AMinusBSignalFormat { get; set; } = "<BASENAME|nm/>-a-b-tdm.csv";

    /// <summary>
    /// Gets or sets the filename format of the B signal.
    /// </summary>
    [XmlElement("format-b")]
    public string BSignalFormat { get; set; } = "<BASENAME|nm/>-b.csv";

    /// <summary>
    /// Gets or sets the signal-to-noise ratio threshold.
    /// </summary>
    [XmlElement("snr-threshold")]
    public double SignalToNoiseRatioThreshold { get; set; } = 2.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecayLoadingConfig"/> class.
    /// </summary>
    public DecayLoadingConfig() { }
} // public sealed class DecayLoadingConfig

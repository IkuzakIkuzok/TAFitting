
// (c) 2024 Kazuki KOHZUKI

using System.Xml.Serialization;

namespace TAFitting.Config;

/// <summary>
/// Represents the solver configuration.
/// </summary>
[Serializable]
public sealed class SolverConfig
{
    /// <summary>
    /// Gets or sets the auto-fit flag.
    /// </summary>
    [XmlElement("auto-fit")]
    public bool AutoFit { get; set; } = true;

    /// <summary>
    /// Gets or sets the threshold data count for parallel processing.
    /// </summary>
    [XmlElement("parallel-threshold")]
    public int ParallelThreshold { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum number of iterations.
    /// </summary>
    [XmlElement("max-iterations")]
    public int MaxIterations { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether to use SIMD.
    /// </summary>
    /// <value><see langword="true"/> if SIMD is used; otherwise, <see langword="false"/>.</value>
    [XmlElement("use-simd")]
    public bool UseSIMD { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum ratio of data points to truncate when using SIMD.
    /// </summary>
    /// <remarks>
    /// The SIMD implementations can handle the specified number of data points, e.g., 2048.
    /// In actual data, the last data in a time series is almost meaningless, so truncation often has no effect on the analysis.
    /// This property specifies how much percentage truncation is allowed.
    /// </remarks>
    [XmlElement("max-truncate-ratio")]
    public double MaxTruncateRatio { get; set; } = 0.1;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolverConfig"/> class.
    /// </summary>
    public SolverConfig() { }
} // public sealed class SolverConfig

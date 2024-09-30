
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
    /// Initializes a new instance of the <see cref="SolverConfig"/> class.
    /// </summary>
    public SolverConfig() { }
} // public sealed class SolverConfig

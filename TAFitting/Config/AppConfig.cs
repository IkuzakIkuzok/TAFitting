
// (c) 2024 Kazuki KOHZUKI

using System.Text;
using System.Xml.Serialization;

namespace TAFitting.Config;

[Serializable]
[XmlRoot("appSettings")]
public sealed class AppConfig
{
    private const string FILENAME = "TAFitting.config";

    private static readonly string FullPath;

    static AppConfig()
    {
        try
        {
            FullPath = Path.Combine(Program.AppLocation, FILENAME);
        }
        catch
        {
            FullPath = FILENAME;
        }
    } // cctor ()

    /// <summary>
    /// Gets or sets the appearance configuration.
    /// </summary>
    [XmlElement("appearance")]
    public AppearanceConfig AppearanceConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the filter configuration.
    /// </summary>
    [XmlElement("filter")]
    public FilterConfig FilterConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the model configuration.
    /// </summary>
    [XmlElement("model")]
    public ModelConfig ModelConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the solver configuration.
    /// </summary>
    [XmlElement("solver")]
    public SolverConfig SolverConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the analyzer configuration.
    /// 
    /// 
    /// </summary>
    [XmlElement("analyzer")]
    public AnalyzerConfig AnalyzerConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets the decay loading configuration.
    /// </summary>
    [XmlElement("decay-loading")]
    public DecayLoadingConfig DecayLoadingConfig { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to use different state for file dialog.
    /// </summary>
    [XmlElement("separate-file-dialog-state")]
    public bool SeparateFileDialogState { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppConfig"/> class.
    /// </summary>
    public AppConfig() { }

    /// <summary>
    /// Loads the application configuration.
    /// </summary>
    /// <returns>Load the configuration from file.
    /// If the file does not exists, returns default configuration.</returns>
    internal static AppConfig Load()
    {
        if (!File.Exists(FullPath)) return new();
        try
        {
            using var reader = new StreamReader(FullPath, Encoding.UTF8);
            return (AppConfig)new XmlSerializer(typeof(AppConfig)).Deserialize(reader)!;
        }
        catch
        {
            return new();
        }
    } // internal static AppConfig Load ()

    /// <summary>
    /// Saves the application configuration.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">The user does not have permission to save the file.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory does not exist.</exception>
    /// <exception cref="IOException">An I/O error occurred.</exception>
    /// <exception cref="System.Security.SecurityException">The user does not have permission to save the file.</exception>
    internal void Save()
    {
        using var writer = new StreamWriter(FullPath, false, Encoding.UTF8);
        new XmlSerializer(typeof(AppConfig)).Serialize(writer, this);
    } // internal void Save ()
} // public sealed class AppConfig

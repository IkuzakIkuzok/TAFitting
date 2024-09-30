
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Config;
using TAFitting.Controls;

namespace TAFitting;

internal static class Program
{
    internal static readonly string AppLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;

    /// <summary>
    /// Gets the main window.
    /// </summary>
    internal static MainWindow MainWindow { get; }

    #region config

    /// <summary>
    /// Gets or sets the gradient start color.
    /// </summary>
    internal static Color GradientStart
    {
        get => Config.AppearanceConfig.Spectra.ColorGradientConfig.StartColor;
        set
        {
            Config.AppearanceConfig.Spectra.ColorGradientConfig.StartColor = value;
            SaveConfig();
            GradientChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the gradient end color.
    /// </summary>
    internal static Color GradientEnd
    {
        get => Config.AppearanceConfig.Spectra.ColorGradientConfig.EndColor;
        set
        {
            Config.AppearanceConfig.Spectra.ColorGradientConfig.EndColor = value;
            SaveConfig();
            GradientChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    internal static Color ObservedColor
    {
        get => Config.AppearanceConfig.ObservedColor;
        set
        {
            Config.AppearanceConfig.ObservedColor = value;
            SaveConfig();
        }
    }

    internal static Color FitColor
    {
        get => Config.AppearanceConfig.FitColor;
        set
        {
            Config.AppearanceConfig.FitColor = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line widdh of the fit lines.
    /// </summary>
    internal static int FitWidth
    {
        get => Config.AppearanceConfig.FitWidth;
        set
        {
            Config.AppearanceConfig.FitWidth = value;
            SaveConfig();
        }
    }

    internal static int SpectraLineWidth
    {
        get => Config.AppearanceConfig.Spectra.LineWidth;
        set
        {
            Config.AppearanceConfig.Spectra.LineWidth = value;
            SaveConfig();
            SpectraLineWidthChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    internal static int SpectraMarkerSize
    {
        get => Config.AppearanceConfig.Spectra.MarkerSize;
        set
        {
            Config.AppearanceConfig.Spectra.MarkerSize = value;
            SaveConfig();
            SpectraMarkerSizeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Get or set the font of the axis labels.
    /// </summary>
    internal static Font AxisLabelFont
    {
        get => Config.AppearanceConfig.AxisLabelFont.Font;
        set
        {
            Config.AppearanceConfig.AxisLabelFont.Font = value;
            SaveConfig();
            AxisLabelFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Get or set the font of the axis titles.
    /// </summary>
    internal static Font AxisTitleFont
    {
        get => Config.AppearanceConfig.AxisTitleFont.Font;
        set
        {
            Config.AppearanceConfig.AxisTitleFont.Font = value;
            SaveConfig();
            AxisTitleFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    internal static Guid DefaultModel
    {
        get => Config.ModelConfig.DefaultModel;
        set
        {
            Config.ModelConfig.DefaultModel = value;
            SaveConfig();
        }
    }

    internal static bool AutoFit
    {
        get => Config.SolverConfig.AutoFit;
        set
        {
            Config.SolverConfig.AutoFit = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the threshold data count for parallel processing.
    /// </summary>
    internal static int ParallelThreshold
    {
        get => Config.SolverConfig.ParallelThreshold;
        set
        {
            Config.SolverConfig.ParallelThreshold = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of iterations.
    /// </summary>
    internal static int MaxIterations
    {
        get => Config.SolverConfig.MaxIterations;
        set
        {
            Config.SolverConfig.MaxIterations = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the filename format of the A-B signal.
    /// </summary>
    internal static string AMinusBSignalFormat
    {
        get => Config.DecayLoadingConfig.AMinusBSignalFormat;
        set
        {
            Config.DecayLoadingConfig.AMinusBSignalFormat = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the filename format of the B signal.
    /// </summary>
    internal static string BSignalFormat
    {
        get => Config.DecayLoadingConfig.BSignalFormat;
        set
        {
            Config.DecayLoadingConfig.BSignalFormat = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Occurs when the color gradient is changed.
    /// </summary>
    internal static event EventHandler? GradientChanged;
    
    /// <summary>
    /// Occurs when the spectra marker size is changed.
    /// </summary>
    internal static event EventHandler? SpectraMarkerSizeChanged;

    /// <summary>
    /// Occurs when the spectra line width is changed.
    /// </summary>
    internal static event EventHandler? SpectraLineWidthChanged;

    /// <summary>
    /// Occurs when the axis label font is changed.
    /// </summary>
    internal static event EventHandler? AxisLabelFontChanged;

    /// <summary>
    /// Occurs when the axis title font is changed.
    /// </summary>
    internal static event EventHandler? AxisTitleFontChanged;

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    internal static AppConfig Config { get; }

    #endregion config

    static Program()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Config = AppConfig.Load();
        MainWindow = new();
    } // cctor ()

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        NegativeSignHandler.SetMinusSign();
        Application.Run(MainWindow);
    } // private static void Main ()

    private static void SaveConfig()
    {
        try
        {
            Config.Save();
        }
        catch
        {
            FadingMessageBox.Show("Failed to save the app configuration.", 0.8, 1000, 75, 0.1);
        }
    } // private static void SaveConfig ()
} // internal static class Program
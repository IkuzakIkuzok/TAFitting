
// (c) 2024-2025 Kazuki Kohzuki

using TAFitting.Controls.Analyzers;

namespace TAFitting;

internal static partial class Program
{
    #region properties

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

    /// <summary>
    /// Gets or sets the line color of the observed data.
    /// </summary>
    internal static Color ObservedColor
    {
        get => Config.AppearanceConfig.ObservedColor;
        set
        {
            Config.AppearanceConfig.ObservedColor = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line color of the filtered data.
    /// </summary>
    internal static Color FilteredColor
    {
        get => Config.AppearanceConfig.FilteredColor;
        set
        {
            Config.AppearanceConfig.FilteredColor = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line color of the fit lines.
    /// </summary>
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
    /// Gets or sets the marker size of the observed data.
    /// </summary>
    internal static int ObservedSize
    {
        get => Config.AppearanceConfig.ObservedSize;
        set
        {
            Config.AppearanceConfig.ObservedSize = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line width of the filtered data.
    /// </summary>
    internal static int FilteredWidth
    {
        get => Config.AppearanceConfig.FilteredWidth;
        set
        {
            Config.AppearanceConfig.FilteredWidth = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line width of the fit lines.
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

    /// <summary>
    /// Gets or sets the marker size of the spectra lines.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the marker size of the spectra.
    /// </summary>
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
    /// Gets or sets the template name in Origin.
    /// </summary>
    internal static string OriginTemplateName
    {
        get => Config.AppearanceConfig.Spectra.OriginTemplate;
        set
        {
            Config.AppearanceConfig.Spectra.OriginTemplate = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Get or set the font of the axis labels.
    /// </summary>
    internal static Font AxisLabelFont
    {
        get => Config.AppearanceConfig.AxisLabelFont;
        set
        {
            Config.AppearanceConfig.AxisLabelFont = value;
            SaveConfig();
            AxisLabelFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Get or set the font of the axis titles.
    /// </summary>
    internal static Font AxisTitleFont
    {
        get => Config.AppearanceConfig.AxisTitleFont;
        set
        {
            Config.AppearanceConfig.AxisTitleFont = value;
            SaveConfig();
            AxisTitleFontChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the default filter.
    /// </summary>
    internal static Guid DefaultFilter
    {
        get => Config.FilterConfig.DefaultFilter;
        set
        {
            Config.FilterConfig.DefaultFilter = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to auto-apply the filter.
    /// </summary>
    internal static bool AutoApplyFilter
    {
        get => Config.FilterConfig.AutoApply;
        set
        {
            Config.FilterConfig.AutoApply = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to hide the original data.
    /// </summary>
    internal static bool HideOriginalData
    {
        get => Config.FilterConfig.HideOriginal;
        set
        {
            Config.FilterConfig.HideOriginal = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the default model.
    /// </summary>
    internal static Guid DefaultModel
    {
        get => Config.ModelConfig.DefaultModel;
        set
        {
            Config.ModelConfig.DefaultModel = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use the default model.
    /// </summary>
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
    /// Gets or sets a value indicating whether to use SIMD.
    /// </summary>
    internal static bool UseSIMD
    {
        get => Config.SolverConfig.UseSIMD;
        set
        {
            Config.SolverConfig.UseSIMD = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the Fourier spectrum type.
    /// </summary>
    internal static FourierSpectrumType FourierSpectrumType
    {
        get => Config.AnalyzerConfig.DefaultFourierSpectrum;
        set
        {
            Config.AnalyzerConfig.DefaultFourierSpectrum = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line color of the analyzer.
    /// </summary>
    internal static Color AnalyzerLineColor
    {
        get => Config.AnalyzerConfig.LineColor;
        set
        {
            Config.AnalyzerConfig.LineColor = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the line width of the analyzer.
    /// </summary>
    internal static int AnalyzerLineWidth
    {
        get => Config.AnalyzerConfig.LineWidth;
        set
        {
            Config.AnalyzerConfig.LineWidth = value;
            SaveConfig();
        }
    }

    /// <summary>
    /// Gets or sets the marker size of the analyzer.
    /// </summary>
    internal static int AnalyzerMarkerSize
    {
        get => Config.AnalyzerConfig.MarkerSize;
        set
        {
            Config.AnalyzerConfig.MarkerSize = value;
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

    #endregion properties

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
} // internal static partial class Program


// (c) 2025 Kazuki KOHZUKI

using DisposalGenerator;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Data;
using TAFitting.Filter.Fourier;

namespace TAFitting.Controls.Analyzers;

/// <summary>
/// Provides a form for Fourier analysis.
/// </summary>
[DesignerCategory("Code")]
[AutoDisposal]
internal sealed partial class FourierAnalyzer : Form, IAnalyzer
{
    private readonly Chart chart;
    private readonly Axis axisX_freq, axisX_time, axisY;

    private Decay? decay;
    private FourierSpectrumType spectrumType = FourierSpectrumType.AmplitudeSpectrum;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourierAnalyzer"/> class.
    /// </summary>
    internal FourierAnalyzer()
    {
        this.Text = "Fourier Analysis";
        this.Size = new(800, 600);

        this.chart = new()
        {
            Dock = DockStyle.Fill,
            Parent = this,
        };

        this.axisX_freq = new Axis()
        {
            Title = "Frequency (Hz)",
            Minimum = 1e3,
            Maximum = 1e6,
            LogarithmBase = 10,
            LabelStyle = new() { Format = "#.0e+0" },
        };
        this.axisY = new Axis()
        {
            Title = "Amplitude",
            Minimum = 1e-6,
            Maximum = 1e-3,
            LogarithmBase = 10,
            LabelStyle = new() { Format = "#.0e+0" },
        };

        // Axis for time domain
        // The axis must be reversed, but here is not in order to customize the labels.
        // The labels are set in `SetTimeLabels`.
        this.axisX_time = new Axis()
        {
            Title = "Period (s)",
            Minimum = 1e3,
            Maximum = 1e6,
            // LabelStyle = new() { Format = "#.0e+0" },
            // IsReversed = true,
            Enabled = AxisEnabled.True,
        };
        this.axisX_time.LabelStyle.Enabled = true;
        this.axisX_time.MajorGrid.Enabled = true;
        this.axisX_time.MajorTickMark.Enabled = true;

        this.axisX_freq.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        this.axisX_freq.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;
        this.axisX_freq.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;

        this.axisX_freq.TitleFont = this.axisX_time.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;
        this.axisX_freq.LabelStyle.Font = this.axisX_time.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;
        Program.AxisTitleFontChanged += SetAxisTitleFont;
        Program.AxisLabelFontChanged += SetAxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX_freq,
            AxisX2 = this.axisX_time,
            AxisY = this.axisY,
        });

        AddDummySeries();
        this.axisX_freq.IsLogarithmic = this.axisX_time.IsLogarithmic = this.axisY.IsLogarithmic = true;
        SetTimeLabels(3, 6, 1.0);  // as minimum and maximum values are set to 1e3 and 1e6, respectively

        #region menu

        this.MainMenuStrip = new()
        {
            Parent = this,
        };

        #region menu.axis

        var menu_axis = new ToolStripMenuItem("&Axis");
        this.MainMenuStrip.Items.Add(menu_axis);

        foreach (var spectrumType in Enum.GetValues<FourierSpectrumType>())
        {
            var item = new ToolStripMenuItem(GetYAxisLabel(spectrumType))
            {
                Tag = spectrumType,
            };
            item.Click += ChangeSpectrumType;
            menu_axis.DropDownOpening += (sender, e) => item.Checked = this.spectrumType == spectrumType;
            menu_axis.DropDownItems.Add(item);
        }

        #endregion menu.axis

        #endregion menu
    } // ctor ()

    override protected void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        Program.AxisTitleFontChanged -= SetAxisTitleFont;
        Program.AxisLabelFontChanged -= SetAxisLabelFont;
    } // override protected void OnClosing (CancelEventArgs)

    /// <summary>
    /// Sets the spectrum of the decay.
    /// </summary>
    private void SetSpectrum()
    {
        if (this.decay is null) return;

        this.chart.Series.Clear();

        var time = this.decay.RawTimes;
        var signal = this.decay.Filtered.RawSignals;

        var n = time.Count;
        var sampleRate = (time.Count - 1) / (time[^1] - time[0]);

        /*
         * The stack size is 1 MiB by default.
         * A complex number consists of two double values, which requires 16 bytes.
         * 16384 elements require 256 KiB, which is far less than 1 MiB.
         */
        var buffer = n <= 0x4000 ? stackalloc Complex[n] : new Complex[n];
        for (var i = 0; i < n; ++i) buffer[i] = new(signal[i], 0);

        FastFourierTransform.Forward(buffer);
        var freq = FastFourierTransform.FrequencyScale(n, sampleRate, false);

        var series = new Series()
        {
            ChartType = SeriesChartType.Line,
            BorderWidth = 2,
            ChartArea = this.chart.ChartAreas[0].Name,
            XAxisType = AxisType.Primary,
        };

        n >>= 1;  // Only positive frequencies, i.e., the half of the spectrum, have physical meanings.
        freq = freq[..n];
        buffer = buffer[..n];
        var amp_min = double.PositiveInfinity;
        var amp_max = double.NegativeInfinity;
        for (var i = 0; i < n; ++i)
        {
            var f = freq[i];
            if (f <= 0) continue;
            var a = CalcYValue(this.spectrumType, buffer[i], f);
            if (this.spectrumType != FourierSpectrumType.PowerSpectralDensityDecibel && a <= 0) continue;
            if (double.IsNaN(a) || double.IsInfinity(a)) continue;
            a = Math.Clamp(a, -1e28, 1e28);
            series.Points.AddXY(f, a);
            amp_min = Math.Min(amp_min, a);
            amp_max = Math.Max(amp_max, a);
        }
        this.chart.Series.Add(series);

        this.axisY.Title = GetYAxisLabel(this.spectrumType);

        var freq_min = freq[1];  // exclude DC component
        var freq_max = freq[^1];
        var x_min = Math.Floor(Math.Log10(freq_min));
        var x_max = Math.Ceiling(Math.Log10(freq_max));
        var f_min = Math.Pow(10, x_min);
        var f_max = Math.Pow(10, x_max);
        this.axisX_freq.Minimum = f_min;
        this.axisX_freq.Maximum = f_max;

        // Set the same range for the time domain axis as the frequency domain axis.
        this.axisX_time.Minimum = f_min;
        this.axisX_time.Maximum = f_max;

        this.axisX_time.Title = $"Period ({this.decay.TimeUnit})";

        if (this.spectrumType == FourierSpectrumType.PowerSpectralDensityDecibel)
        {
            this.axisY.IsLogarithmic = false;
            {
                var log = Math.Log10(Math.Abs(amp_min));
                var order = log == 0 ? 0 : Math.Floor(log);
                var decrement = log % 1 == 0 ? Math.Pow(10, order - 1) : Math.Pow(10, order);
                this.axisY.Minimum = Math.Ceiling(amp_min / decrement) * decrement - decrement;
            }
            {
                var log = Math.Log10(Math.Abs(amp_max));
                var order = log == 0 ? 0 : Math.Floor(log);
                var increment = Math.Pow(10, order);
                this.axisY.Maximum = Math.Floor(amp_max / increment) * increment + increment;
            }
        }
        else
        {
            this.axisY.IsLogarithmic = true;
            var y_min = Math.Floor(Math.Log10(amp_min));
            var y_max = Math.Ceiling(Math.Log10(amp_max));
            this.axisY.Minimum = Math.Pow(10, y_min);
            this.axisY.Maximum = Math.Pow(10, y_max);
        }

        this.chart.ChartAreas[0].RecalculateAxesScale();
        this.axisX_freq.AdjustAxisIntervalLogarithmic();
        this.axisX_time.AdjustAxisIntervalLogarithmic();
        this.axisY.AdjustAxisInterval();

        SetTimeLabels((int)x_min, (int)x_max, this.decay.TimeUnit);
    } // private void SetSpectrum ()

    private static double CalcYValue(FourierSpectrumType spectrumType, Complex y, double f)
    {
        var amp = y.Magnitude;
        if (spectrumType == FourierSpectrumType.AmplitudeSpectrum) return amp;

        var power = amp * amp;
        if (spectrumType == FourierSpectrumType.PowerSpectrum) return power;

        var psd = power / f;
        if (spectrumType == FourierSpectrumType.PowerSpectralDensity) return psd;
        if (spectrumType == FourierSpectrumType.AmplitudeSpectralDensity) return Math.Sqrt(psd);

        Debug.Assert(spectrumType == FourierSpectrumType.PowerSpectralDensityDecibel);
        return 10 * Math.Log10(psd);
    } // private static double CalcYValue (FourierSpectrumType, Complex, double)

    private static string GetYAxisLabel(FourierSpectrumType spectrumType)
        => spectrumType switch
        {
            FourierSpectrumType.AmplitudeSpectrum           => "Amplitude",
            FourierSpectrumType.AmplitudeSpectralDensity    => "Amplitude spectral density",
            FourierSpectrumType.PowerSpectrum               => "Power",
            FourierSpectrumType.PowerSpectralDensity        => "Power spectral density",
            FourierSpectrumType.PowerSpectralDensityDecibel => "Power spectral density (dB)",
            _ => throw new ArgumentOutOfRangeException(nameof(spectrumType)),
        };

    /// <summary>
    /// Sets the time labels.
    /// </summary>
    /// <param name="freq_min_log">The minimum value of the frequency in logarithm.</param>
    /// <param name="freq_max_log">The maximum value of the frequency in logarithm.</param>
    /// <param name="timeUnit">The time unit for scaling.</param>
    private void SetTimeLabels(int freq_min_log, int freq_max_log, double timeUnit)
    {
        var d = Math.Max((int)this.axisX_freq.Interval, 1);
        var n = (freq_max_log - freq_min_log + 1) / d;
        this.axisX_time.CustomLabels.Clear();
        for (var i = 0; i < n; i += d)
        {
            var f = Math.Pow(10, freq_min_log + i);
            var t = 1 / f / timeUnit;
            this.axisX_time.CustomLabels.Add(freq_min_log + i - 1, freq_min_log + i + 1, $"{t:#.0e+0}");
        }
    } // private void SetTimeLabels (int, int)

    /// <inheritdoc/>
    public void SetDecay(Decay decay, double wavelength)
    {
        this.Text = $"Fourier Analysis: {wavelength} nm";
        this.decay = decay;
        SetSpectrum();
    } // public void SetDecay (Decay, double)

    private void ChangeSpectrumType(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not FourierSpectrumType type) return;
        this.spectrumType = type;
        SetSpectrum();
    } // private void ChangeSpectrumType (object, EventArgs)

    private void AddDummySeries()
    {
        var dummy = new Series()
        {
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
            XAxisType = AxisType.Primary,
        };
        dummy.Points.AddXY(1e4, 1e-10);
        this.chart.Series.Add(dummy);
    } // private void AddDummySeries ()

    private void SetAxisTitleFont(object? sender, EventArgs e)
        => this.axisX_freq.TitleFont = this.axisX_time.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;

    private void SetAxisLabelFont(object? sender, EventArgs e)
        => this.axisX_freq.LabelStyle.Font = this.axisX_time.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;
} // internal sealed partial class FourierAnalyzer : Form

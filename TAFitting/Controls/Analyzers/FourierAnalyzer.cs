
// (c) 2025 Kazuki KOHZUKI

using DisposalGenerator;
using System.Numerics;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Data;
using TAFitting.Filter.Fourier;

namespace TAFitting.Controls.Analyzers;

[DesignerCategory("Code")]
[AutoDisposal]
internal sealed partial class FourierAnalyzer : Form, IAnalyzer
{
    private readonly Chart chart;
    private readonly Axis axisX_freq, axisX_time, axisY;

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
    } // ctor ()

    override protected void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        Program.AxisTitleFontChanged -= SetAxisTitleFont;
        Program.AxisLabelFontChanged -= SetAxisLabelFont;
    } // override protected void OnClosing (CancelEventArgs)

    private void SetAmplitude(Decay decay)
    {
        this.chart.Series.Clear();

        var time = decay.RawTimes;
        var signal = decay.Filtered.RawSignals;

        var n = (int)Math.Pow(2, Math.Ceiling(Math.Log2(time.Count)));
        var sampleRate = (time.Count - 1) / (time[^1] - time[0]);

        var offset = (n - time.Count) >> 1;
        var buffer = new Complex[n];
        for (var i = 0; i < time.Count; ++i)
            buffer[i + offset] = new(signal[i], 0);

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
            var a = buffer[i].Magnitude;
            if (f <= 0 || a <= 0) continue;
            series.Points.AddXY(f, a);
            amp_min = Math.Min(amp_min, a);
            amp_max = Math.Max(amp_max, a);
        }
        this.chart.Series.Add(series);

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

        this.axisX_time.Title = $"Period ({decay.TimeUnit})";

        var y_min = Math.Floor(Math.Log10(amp_min));
        var y_max = Math.Ceiling(Math.Log10(amp_max));
        this.axisY.Minimum = Math.Pow(10, y_min);
        this.axisY.Maximum = Math.Pow(10, y_max);

        this.chart.ChartAreas[0].RecalculateAxesScale();
        this.axisX_freq.AdjustAxisIntervalLogarithmic();
        this.axisX_time.AdjustAxisIntervalLogarithmic();
        this.axisY.AdjustAxisIntervalLogarithmic();

        SetTimeLabels((int)x_min, (int)x_max, decay.TimeUnit);
    } // private void SetAmplitude (Decay)

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
        SetAmplitude(decay);
    } // public void SetDecay (Decay, double)

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

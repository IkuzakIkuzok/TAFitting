
// (c) 2026 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Controls.Charting;
using TAFitting.Model;

namespace TAFitting.Controls;

/// <summary>
/// Provides helper methods and properties for configuring and managing chart plots, including axes and data series, within a charting component.
/// </summary>
/// <remarks>The <see cref="PlotHelper"/> class is intended for internal use to facilitate the display and customization of observed, filtered, fitted, and comparison data series on a chart.
/// It exposes properties for accessing the X and Y axes, and provides methods to update plot appearance, adjust axis intervals, and manage data series.
/// This class is not thread-safe and should be used only on the UI thread that owns the chart.</remarks>
internal sealed class PlotHelper
{
    private readonly Chart chart;
    private readonly Axis axisX, axisY;
    private readonly CacheSeries s_observed, s_filtered, s_fit, s_compare;

    /// <summary>
    /// Gets or sets a value indicating whether drawing operations should be stopped.
    /// </summary>
    internal bool StopDrawing { get; set; } = false;

    /// <summary>
    /// Gets the horizontal associated with the chart or plot area.
    /// </summary>
    internal Axis AxisX => this.axisX;

    /// <summary>
    /// Gets the vertical axis associated with the chart area.
    /// </summary>
    internal Axis AxisY => this.axisY;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlotHelper"/> class and configures the specified chart for plotting time versus ΔµOD data on logarithmic axes.
    /// </summary>
    /// <param name="chart">The Chart control to be configured for plotting.</param>
    internal PlotHelper(Chart chart)
    {
        this.chart = chart;

        this.axisX = new()
        {
            Title = "Time (µs)",
            Minimum = 0.05,
            Maximum = 1000,
            LogarithmBase = 10,
            //Interval = 1,
            LabelStyle = new() { Format = "#.0e+0" },
        };
        this.axisY = new()
        {
            Title = "ΔµOD",
            Minimum = 10,
            Maximum = 10000,
            LogarithmBase = 10,
            //Interval = 1,
            LabelStyle = new() { Format = "#.0e+0" },
        };

        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        //this.axisX_freq.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;

        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        this.s_observed = new()
        {
            Color = Program.ObservedColor,
            ChartType = SeriesChartType.Point,
            MarkerSize = Program.ObservedSize,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_observed);

        this.s_filtered = new()
        {
            Color = Program.FilteredColor,
            ChartType = SeriesChartType.Line,
            BorderWidth = Program.FilteredWidth,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_filtered);

        this.s_fit = new()
        {
            Color = Program.FitColor,
            ChartType = SeriesChartType.Line,
            BorderWidth = Program.FitWidth,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_fit);

        this.s_compare = new()
        {
            Color = Color.FromArgb(64, Program.FilteredColor),
            ChartType = SeriesChartType.Line,
            BorderWidth = Program.FilteredWidth,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_compare);
    } // ctor (Chart)

    /// <summary>
    /// Adds a dummy series to the chart.
    /// </summary>
    internal void AddDummySeries()
    {
        var dummy = new Series()
        {
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        dummy.Points.AddXY(1e-6, 1e-6);
        this.chart.Series.Add(dummy);
    } // internal void AddDummySeries ()

    /// <summary>
    /// Removes all observed points from the collection.
    /// </summary>
    internal void ClearObserved()
        => this.s_observed.Points.Clear();

    /// <summary>
    /// Removes all filtered points from the collection.
    /// </summary>
    internal void ClearFit()
        => this.s_fit.Points.Clear();

    /// <summary>
    /// Removes all comparison points from the collection.
    /// </summary>
    internal void ClearCompare()
        => this.s_compare.Points.Clear();

    /// <summary>
    /// Displays the observed and fitted plots for the specified parameter row and fitting model.
    /// </summary>
    /// <param name="row">The parameter row for which to display plots. If <see langword="null"/>, no observed or fitted plots are shown.</param>
    /// <param name="model">The fitting model used to generate the fitted plot. If <see langword="null"/>, only the observed plot is displayed.</param>
    /// <param name="hideOriginal"><see langword="true"/> to hide the original observed data in the plot; otherwise, <see langword="false"/>.</param>
    internal void ShowPlots(ParametersTableRow? row, IFittingModel? model, bool hideOriginal)
    {
        ShowObserved(row, hideOriginal);
        ShowFit(row, model);
    } // internal void ShowPlots (ParametersTableRow?, IFittingModel?, bool)

    /// <summary>
    /// Displays the observed and filtered signal data for the specified parameters row.
    /// </summary>
    /// <param name="row">The parameters row containing the signal data to display. If <see langword="null"/>, no action is taken.</param>
    /// <param name="hideOriginal"><see langword="true"/> to hide the original observed data; otherwise, <see langword="false"/> to display both.</param>
    internal void ShowObserved(ParametersTableRow? row, bool hideOriginal)
    {
        if (this.StopDrawing) return;
        if (row is null) return;

        var decay = row.Decay;
        var filtered = decay.FilteredSignals;

        var invert = row.Inverted;

        if (hideOriginal)
            this.s_observed.Points.Clear();
        else
            this.s_observed.AddDecay(decay, invert);

        this.s_filtered.AddPositivePoints(decay.GetTimesAsSpan(), filtered, invert);
    }// internal void ShowObserved (ParametersTableRow?, bool)

    /// <summary>
    /// Displays a comparison visualization for the specified parameters row.
    /// </summary>
    /// <param name="row">The parameters row containing decay to use for the comparison.</param>
    internal void ShowCompare(ParametersTableRow row)
    {
        var decay = row.Decay;
        var invert = row.Inverted;

        var filtered = decay.FilteredSignals;
        this.s_compare.AddPositivePoints(decay.GetTimesAsSpan(), filtered, invert);
    } // internal void ShowCompare (ParametersTableRow)

    /// <summary>
    /// Displays the fit curve for the specified parameters and fitting model on the current plot.
    /// </summary>
    /// <param name="row">The table row containing the decay data and fitting parameters to use for generating the fit curve.</param>
    /// <param name="model">The fitting model used to generate the fit function.</param>
    internal void ShowFit(ParametersTableRow? row, IFittingModel? model)
    {
        if (this.StopDrawing) return;
        if (row is null) return;
        if (model is null) return;

        var decay = row.Decay;

        var parameters = row.Parameters;
        var func = model.GetFunction(parameters);
        var invert = row.Inverted;
        this.s_fit.AddPositivePoints(decay.GetTimesAsSpan(), func, invert);
    } // internal void ShowFit ()

    #region Adjust axes

    internal void AdjustXAxisInterval(object? sender, EventArgs e)
        => AdjustXAxisInterval();

    internal void AdjustYAxisInterval(object? sender, EventArgs e)
        => AdjustYAxisInterval();

    private void AdjustXAxisInterval()
        => AdjustAxisInterval(this.axisX);

    private void AdjustYAxisInterval()
        => AdjustAxisInterval(this.axisY);

    private void AdjustAxisInterval(Axis axis)
    {
        this.chart.ChartAreas[0].RecalculateAxesScale();
        var pixelInterval = axis.IsLogarithmic ? 30 : 100;
        axis.AdjustAxisInterval(pixelInterval);
    } // private void AdjustAxisInterval (Axis)

    internal void AdjustAxesIntervals(object? sender, EventArgs e)
        => AdjustAxesIntervals();

    internal void AdjustAxesIntervals()
    {
        AdjustXAxisInterval();
        AdjustYAxisInterval();
    } // internal void AdjustAxesIntervals ()

    #endregion Adjust axes

    #region change appearance

    #region change color

    internal void SetObservedColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.ObservedColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_observed.Color = Program.ObservedColor = cd.Color;
    } // internal void SetObservedColor (object?, EventArgs)

    internal void SetFilteredColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.FilteredColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_filtered.Color = Program.FilteredColor = cd.Color;
        this.s_compare.Color = Color.FromArgb(64, this.s_filtered.Color);
    } // internal void SetFilteredColor (object?, EventArgs)

    internal void SetFitColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.FitColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_fit.Color = Program.FitColor = cd.Color;
    } // internal void SetFitColor (object?, EventArgs)

    #endregion change color

    #region change width/size

    internal void ChangeObservedSize(object? sender, ToolStripMenuItemGroupSelectionChangedEventArgs<int> e)
    {
        if (e.SelectedItem is null) return;
        var size = e.SelectedItem.Tag;
        this.s_observed.MarkerSize = Program.ObservedSize = size;
    } // internal void ChangeObservedSize (object?, ToolStripMenuItemGroupSelectionChangedEventArgs<int>)

    internal void ChangeFilteredWidth(object? sender, ToolStripMenuItemGroupSelectionChangedEventArgs<int> e)
    {
        if (e.SelectedItem is null) return;
        var width = e.SelectedItem.Tag;
        this.s_filtered.BorderWidth = this.s_compare.BorderWidth = Program.FilteredWidth = width;
    } // internal void ChangeFilteredWidth (object?, ToolStripMenuItemGroupSelectionChangedEventArgs<int>)

    internal void ChangeFitWidth(object? sender, ToolStripMenuItemGroupSelectionChangedEventArgs<int> e)
    {
        if (e.SelectedItem is null) return;
        var width = e.SelectedItem.Tag;
        this.s_fit.BorderWidth = Program.FitWidth = width;
    } // internal void ChangeFitWidth (object?, ToolStripMenuItemGroupSelectionChangedEventArgs<int>)

    #endregion change width/size

    #region change font

    internal void SelectAxisLabelFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisLabelFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont = fd.Font;
    } // internal void SelectAxisLabelFont (object?, EventArgs)

    internal void SelectAxisTitleFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisTitleFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont = fd.Font;
    } // internal void SelectAxisTitleFont (object?, EventArgs)

    #endregion change font

    #endregion change appearance
} // internal sealed class PlotHelper

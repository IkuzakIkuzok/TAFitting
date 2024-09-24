﻿
// (c) 2024 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Excel;
using TAFitting.Model;

namespace TAFitting.Controls.Spectra;

[DesignerCategory("Code")]
internal sealed class SpectraPreviewWindow : Form
{
    private readonly SplitContainer mainContainter;

    private readonly TimeTable timeTable;

    private readonly Chart chart;
    private readonly Axis axisX, axisY;

    private Guid modelId = Guid.Empty;
    private Dictionary<double, double[]> parameters = [];
    private double selectedWavelength = double.NaN;

    private int lineWidth = Program.SpectraLineWidth;
    private int markerSize = Program.SpectraMarkerSize;

    internal Guid ModelId
    {
        get => this.modelId;
        set
        {
            if (this.modelId == value) return;
            this.modelId = value;
            DrawSpectra();
        }
    }

    internal double SelectedWavelength
    {
        get => this.selectedWavelength;
        set
        {
            if (this.selectedWavelength == value) return;
            this.selectedWavelength = value;
            DrawSpectra();
        }
    }

    private IFittingModel Model => ModelManager.Models[this.modelId];

    internal SpectraPreviewWindow()
    {
        this.Text = "Spectra Preview";
        this.Size = new(900, 500);

        this.mainContainter = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BorderStyle = BorderStyle.Fixed3D,
            Parent = this,
        };
        
        this.chart = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.mainContainter.Panel1,
        };

        this.axisX = new Axis()
        {
            Title = "Wavelength (nm)",
            Minimum = 500,
            Maximum = 1000,
            LogarithmBase = 10,
            Interval = 100,
        };
        this.axisY = new Axis()
        {
            Title = "ΔµOD",
            Minimum = -1000,
            Maximum = 1000,
            Interval = 500,
        };

        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        this.axisX.MinorGrid.Interval = 20;
        this.axisY.MinorGrid.Interval = 100;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;
        Program.AxisTitleFontChanged += SetAxisTitleFont;
        Program.AxisLabelFontChanged += SetAxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        this.chart.Paint += AdjustAxisIntervalsOnFirstPaint;

        this.timeTable = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.mainContainter.Panel2,
        };
        this.timeTable.CellValueChanged += DrawSpectra;
        this.timeTable.RowsRemoved += DrawSpectra;
        this.timeTable.Rows.Add(0.5);
        this.timeTable.Rows.Add(1.0);
        this.timeTable.Rows.Add(2.0);
        this.timeTable.Rows.Add(4.0);
        this.timeTable.Rows.Add(8.0);

        #region menu

        this.MainMenuStrip = new()
        {
            Parent = this,
        };

        #region menu.file

        var menu_file = new ToolStripMenuItem("&File");
        this.MainMenuStrip.Items.Add(menu_file);

        var menu_fileSave = new ToolStripMenuItem("&Save")
        {
            ShortcutKeys = Keys.Control | Keys.S,
        };
        menu_fileSave.Click += SaveToFile;
        menu_file.DropDownItems.Add(menu_fileSave);

        menu_file.DropDownItems.Add(new ToolStripSeparator());

        var menu_fileClose = new ToolStripMenuItem("&Close");
        menu_fileClose.Click += (sender, e) => Close();
        menu_file.DropDownItems.Add(menu_fileClose);

        #endregion menu.file

        #region menu.view

        var menu_view = new ToolStripMenuItem("&View");
        this.MainMenuStrip.Items.Add(menu_view);

        var menu_viewColorGradient = new ToolStripMenuItem("&Color Gradient");
        menu_viewColorGradient.Click += SelectColorGradient;
        menu_view.DropDownItems.Add(menu_viewColorGradient);

        #region line width

        var menu_viewLineWidth = new ToolStripMenuItem("&Line Width");
        menu_view.DropDownItems.Add(menu_viewLineWidth);

        for (var i = 0; i <= 10; i++)
        {
            var item = new ToolStripMenuItem(i.ToString())
            {
                Tag = i,
            };
            item.Click += ChangeLineWidth;
            menu_viewLineWidth.DropDownOpening += (sender, e) => item.Checked = (int)item.Tag == this.lineWidth;
            menu_viewLineWidth.DropDownItems.Add(item);
        }

        #endregion line width

        #region marker size

        var menu_viewMarkerSize = new ToolStripMenuItem("&Marker Size");
        menu_view.DropDownItems.Add(menu_viewMarkerSize);

        for (var i = 0; i <= 10; i++)
        {
            var item = new ToolStripMenuItem(i.ToString())
            {
                Tag = i,
            };
            item.Click += ChangeMarkerSize;
            menu_viewMarkerSize.DropDownOpening += (sender, e) => item.Checked = (int)item.Tag == this.markerSize;
            menu_viewMarkerSize.DropDownItems.Add(item);
        }

        #endregion marker size

        #endregion menu.view

        #endregion menu

        this.mainContainter.SplitterDistance = 700;
        this.mainContainter.Panel2MinSize = 150;
    } // ctor ()

    internal SpectraPreviewWindow(IReadOnlyDictionary<double, double[]> parameters) : this()
    {
        SetParameters(parameters);
    } // ctor (IReadOnlyDictionary<double, double[]>)

    override protected void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);

        Program.AxisTitleFontChanged -= SetAxisTitleFont;
        Program.AxisLabelFontChanged -= SetAxisLabelFont;
    } // override protected void OnClosing (CancelEventArgs)

    override protected void OnShown(EventArgs e)
    {
        base.OnShown(e);

        DrawHorizontalLine(this.axisX.Minimum, this.axisX.Maximum);
    } // override protected void OnShown (EventArgs)

    private void AdjustAxisIntervals()
    {
        this.chart.ChartAreas[0].RecalculateAxesScale();
        this.axisX.AdjustAxisIntervalLinear(75);
        this.axisY.AdjustAxisIntervalLinear(50);
    } // private void AdjustAxisIntervals ()

    private void AdjustAxisIntervalsOnFirstPaint(object? sender, EventArgs e)
    {
        this.chart.Paint -= AdjustAxisIntervalsOnFirstPaint;
        AdjustAxisIntervals();
    } // private void AdjustAxisIntervalsOnFirstPaint (object?, EventArgs)

    private void DrawSpectra(object? sender, EventArgs e)
        => DrawSpectra();

    private void DrawSpectra()
    {
        if (this.modelId == Guid.Empty) return;
        var model = this.Model;

        this.chart.Series.Clear();

        if (this.parameters.Count == 0) return;

        var wlMin = this.parameters.Keys.Min();
        var wlMax = this.parameters.Keys.Max();

        DrawHorizontalLine(wlMin, wlMax);

        this.timeTable.SetColors();

        var times = this.timeTable.Times.ToArray();
        var funcs = this.parameters.ToDictionary(
            kv => kv.Key,
            kv => model.GetFunction(kv.Value)
        );
        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, times.Length);
        var index = 0;
        var sigMin = double.MaxValue;
        var sigMax = double.MinValue;
        foreach (var time in times)
        {
            (var min, var max) = DrawSpectrum(time, funcs, gradient[index++]);
            sigMin = Math.Min(sigMin, min);
            sigMax = Math.Max(sigMax, max);
        }

        this.axisX.Minimum = wlMin;
        this.axisX.Maximum = wlMax;
        this.axisY.Minimum = Math.Min(sigMin * 1.1, 0.0);
        this.axisY.Maximum = Math.Max(sigMax * 1.1, 0.0);

        AdjustAxisIntervals();
    } // private void DrawSpectra ()

    private (double Min, double Max) DrawSpectrum(double time, Dictionary<double, Func<double, double>> funcs, Color color)
    {
        var series = new Series
        {
            ChartType = SeriesChartType.Line,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = this.markerSize,
            Color = color,
            BorderWidth = this.lineWidth,
            LegendText = time.ToString("F2"),
        };

        var min = double.MaxValue;
        var max = double.MinValue;
        foreach (var wavelength in this.parameters.Keys.Order())
        {
            var func = funcs[wavelength];
            var signal = func(time);
            min = Math.Min(min, signal);
            max = Math.Max(max, signal);
            series.Points.AddXY(wavelength, signal);

            if (wavelength == this.selectedWavelength)
                HighlightWavelength(wavelength, signal, color);
        }
        this.chart.Series.Add(series);
        return (min, max);
    } // private (double, double) DrawSpectrum (double, Dictionary<double, Func<double, double>>, Color)

    private void DrawHorizontalLine(double wlMin, double wlMax)
    {
        var horizontal = new Series()
        {
            ChartType = SeriesChartType.Line,
            Color = Color.Black,
            BorderWidth = 2,
        };
        horizontal.Points.AddXY(wlMin, 0);
        horizontal.Points.AddXY(wlMax, 0);
        this.chart.Series.Add(horizontal);
    } // private void DrawHorizontalLine ()

    private void HighlightWavelength(double wavelength, double signal, Color color)
    {
        var series = new Series()
        {
            ChartType = SeriesChartType.Line,
            MarkerStyle = MarkerStyle.Cross,
            MarkerSize = this.markerSize + 10,
            Color = color,
            BorderWidth = 2,
        };
        series.Points.AddXY(wavelength, signal);
        this.chart.Series.Add(series);
    } // private void HighlightWavelength (double)

    private void SaveToFile(object? sender, EventArgs e)
        => SaveToFile();

    private void SaveToFile()
    {
        if (this.modelId == Guid.Empty) return;
        if (this.parameters.Count == 0) return;
        if (this.timeTable.Rows.Count == 0) return;

        using var sfd = new SaveFileDialog
        {
            Title = "Save Spectra",
            Filter = "Excel Workbook|*.xlsx|CSV files|*.csv|All files|*.*",
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var filename = sfd.FileName;
        var extension = Path.GetExtension(filename);
        var writer = GetSpreadSheetWriter(extension);

        try
        {
            writer.Parameters = this.Model.Parameters.Select(p => p.Name).ToArray();
            writer.Times = this.timeTable.Times.ToArray();

            foreach ((var wavelength, var parameters) in this.parameters)
                writer.AddRow(wavelength, parameters);

            writer.Write(filename);

            FadingMessageBox.Show(
                "Spectra saved successfully.",
                0.8, 1000, 75, 0.1, this
            );
        }
        catch (Exception e)
        {
            MessageBox.Show(
                e.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            if (writer is IDisposable disposable)
                disposable.Dispose();
        }
    } // private void SaveToFile ()

    private ISpreadSheetWriter GetSpreadSheetWriter(string extension)
        => extension.ToUpper() switch
        {
            ".CSV" => new CsvWriter(this.Model),
            ".XLSX" => new ExcelWriter(this.Model),
            _ => throw new NotSupportedException($"The extension '{extension}' is not supported."),
        }; // private static ISpreadSheetWriter GetSpreadSheetWriter (string

    internal void SetParameters(IReadOnlyDictionary<double, double[]> parameters)
    {
        this.parameters = parameters.ToDictionary();
        DrawSpectra();
    } // internal void SetParameters (IDictionary<double, double[]>)

    private void SelectColorGradient(object? sender, EventArgs e)
    {
        var picker = new ColorGradientPicker(Program.GradientStart, Program.GradientEnd)
        {
            StartPosition = FormStartPosition.CenterParent,
        };
        (Program.GradientStart, Program.GradientEnd) = picker.ShowDialog();

        DrawSpectra();
    } // private void SelectColorGradient (object?, EventArgs)

    private void ChangeLineWidth(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not int width) return;
        this.lineWidth = Program.SpectraLineWidth = width;
        DrawSpectra();
    } // private void ChangeLineWidth (object?, EventArgs)

    private void ChangeMarkerSize(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not int size) return;
        this.markerSize = Program.SpectraMarkerSize = size;
        DrawSpectra();
    } // private void ChangeMarkerSize (object?, EventArgs)

    private void SetAxisTitleFont(object? sender, EventArgs e)
        => this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;

    private void SetAxisLabelFont(object? sender, EventArgs e)
        => this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;
} // internal sealed class SpectraPreviewWindow : Form


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
            Minimum = 0.05,
            Maximum = 1000,
            LogarithmBase = 10,
            Interval = 100,
        };
        this.axisY = new Axis()
        {
            Title = "ΔµOD",
            Minimum = -1000,
            Maximum = 1000,
            Interval = 100,
        };

        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        this.axisX.MinorGrid.Interval = 50;
        this.axisY.MinorGrid.Interval = 50;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;
        Program.AxisTitleFontChanged += SetAxisTitleFont;
        Program.AxisLabelFontChanged += SetAxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        this.timeTable = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.mainContainter.Panel2,
        };
        this.timeTable.CellValueChanged += DrawSpectra;
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

        var horizontal = new Series()
        {
            ChartType = SeriesChartType.Line,
            Color = Color.Black,
            BorderWidth = 1,
        };
        horizontal.Points.AddXY(wlMin, 0);
        horizontal.Points.AddXY(wlMax, 0);
        this.chart.Series.Add(horizontal);

        this.timeTable.SetColors();

        var times = this.timeTable.Times.ToArray();
        var gradient = new ColorGradient(Program.GradientStart, Program.GradientEnd, times.Length);
        var index = 0;
        var sigMin = double.MaxValue;
        var sigMax = double.MinValue;
        foreach (var time in times)
        {
            (var min, var max) = DrawSpectrum(time, model, gradient[index++]);
            sigMin = Math.Min(sigMin, min);
            sigMax = Math.Max(sigMax, max);
        }

        this.axisX.Minimum = wlMin;
        this.axisX.Maximum = wlMax;
        this.axisY.Minimum = Math.Min(sigMin * 1.2, 0.0);
        this.axisY.Maximum = Math.Max(sigMax * 1.2, 0.0);
    } // private void DrawSpectra ()

    private (double Min, double Max) DrawSpectrum(double time, IFittingModel model, Color color)
    {
        var series = new Series
        {
            ChartType = SeriesChartType.Line,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = this.markerSize,
            Color = color,
            BorderWidth = 2,
            LegendText = time.ToString("F2"),
        };

        var min = double.MaxValue;
        var max = double.MinValue;
        foreach ((var wavelength, var parameters) in this.parameters.OrderBy(kv => kv.Key))
        {
            var func = model.GetFunction(parameters);
            var signal = func(time);
            min = Math.Min(min, signal);
            max = Math.Max(max, signal);
            series.Points.AddXY(wavelength, signal);
        }
        this.chart.Series.Add(series);
        return (min, max);
    } // private (double, double) DrawSpectrum (double, IFittingModel, Color)

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

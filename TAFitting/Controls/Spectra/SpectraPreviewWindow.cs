
// (c) 2024 Kazuki Kohzuki

using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Controls.Toast;
using TAFitting.Excel;
using TAFitting.Model;
using TAFitting.Origin;

namespace TAFitting.Controls.Spectra;

/// <summary>
/// Represents a window for previewing spectra.
/// </summary>
[DesignerCategory("Code")]
internal sealed class SpectraPreviewWindow : Form
{
    private readonly SplitContainer mainContainer, optionsContainer;

    private readonly TimeTable timeTable;
    private readonly MaskingRangeBox maskingRangeBox;

    private readonly Chart chart;
    private readonly Axis axisX, axisY;

    private Guid modelId = Guid.Empty;
    private Dictionary<double, double[]> parameters = [];
    private double selectedWavelength = double.NaN;

    /// <summary>
    /// Gets or sets the ID of the model.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the selected wavelength.
    /// </summary>
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

    internal string TimeUnit
    {
        get => this.timeTable.Unit;
        set => this.timeTable.Unit = value;
    }

    internal string SignalUnit
    {
        get => this.axisY.Title;
        set => this.axisY.Title = value;
    }

    /// <summary>
    /// Gets the model.
    /// </summary>
    private IFittingModel Model => ModelManager.Models[this.modelId].Model;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpectraPreviewWindow"/> class.
    /// </summary>
    internal SpectraPreviewWindow()
    {
        this.Text = "Spectra Preview";
        this.Size = new(900, 500);

        this.mainContainer = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BorderStyle = BorderStyle.Fixed3D,
            Parent = this,
        };
        
        this.chart = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.mainContainer.Panel1,
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

        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;
        Program.AxisTitleFontChanged += SetAxisTitleFont;
        Program.AxisLabelFontChanged += SetAxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });
        this.chart.MouseDoubleClick += SelectWavelength;
        this.chart.Paint += AdjustAxisIntervalsOnFirstPaint;

        this.optionsContainer = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BorderStyle = BorderStyle.FixedSingle,
            Parent = this.mainContainer.Panel2,
        };

        this.timeTable = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.optionsContainer.Panel1,
        };
        this.timeTable.CellValueChanged += DrawSpectra;
        this.timeTable.RowsRemoved += DrawSpectra;
        this.timeTable.Rows.Add(0.5);
        this.timeTable.Rows.Add(1.0);
        this.timeTable.Rows.Add(2.0);
        this.timeTable.Rows.Add(4.0);
        this.timeTable.Rows.Add(8.0);

        _ = new Label()
        {
            Text = "Masking",
            Location = new(10, 10),
            Width = 100,
            Parent = this.optionsContainer.Panel2,
        };

        this.maskingRangeBox = new()
        {
            Location = new(10, 35),
            Width = 140,
            Parent = this.optionsContainer.Panel2,
        };
        this.maskingRangeBox.DelayedTextChanged += DrawSpectra;

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

        if (OriginProject.IsAvailable)
        {
            var menu_fileExport = new ToolStripMenuItem("&Export to Origin")
            {
                ShortcutKeys = Keys.Control | Keys.E,
            };
            menu_fileExport.Click += ExportToOrigin;
            menu_file.DropDownItems.Add(menu_fileExport);
        }

        var menu_fileCopy = new ToolStripMenuItem("&Copy spectra")
        {
            ShortcutKeys = Keys.Control | Keys.C,
        };
        menu_fileCopy.Click += CopyPlotsToClipboard;
        menu_file.DropDownItems.Add(menu_fileCopy);

        menu_file.DropDownItems.Add(new ToolStripSeparator());

        var menu_fileClose = new ToolStripMenuItem("&Close")
        {
            ShortcutKeys = Keys.Control | Keys.W,
        };
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
            menu_viewLineWidth.DropDownOpening += (sender, e) => item.Checked = (int)item.Tag == Program.SpectraLineWidth;
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
            menu_viewMarkerSize.DropDownOpening += (sender, e) => item.Checked = (int)item.Tag == Program.SpectraMarkerSize;
            menu_viewMarkerSize.DropDownItems.Add(item);
        }

        #endregion marker size

        #endregion menu.view

        #endregion menu

        this.mainContainer.SplitterDistance = 700;
        this.mainContainer.Panel2MinSize = 150;

        Program.GradientChanged += DrawSpectra;
        Program.SpectraLineWidthChanged += DrawSpectra;
        Program.SpectraMarkerSizeChanged += DrawSpectra;
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

    private void AdjustAxesIntervals(object? sender, EventArgs e)
        => AdjustAxesIntervals();

    private void AdjustAxesIntervals()
    {
        this.chart.ChartAreas[0].RecalculateAxesScale();
        this.axisX.AdjustAxisIntervalLinear(75);
        this.axisY.AdjustAxisIntervalLinear(50);
    } // private void AdjustAxesIntervals ()

    private void AdjustAxisIntervalsOnFirstPaint(object? sender, EventArgs e)
    {
        this.chart.Paint -= AdjustAxisIntervalsOnFirstPaint;
        this.chart.SizeChanged += AdjustAxesIntervals;
        AdjustAxesIntervals();
    } // private void AdjustAxisIntervalsOnFirstPaint (object?, EventArgs)

    private void DrawSpectra(object? sender, EventArgs e)
        => DrawSpectra();

    private void DrawSpectra()
    {
        if (this.modelId == Guid.Empty) return;
        var model = this.Model;

        this.chart.Series.Clear();

        if (this.parameters.Count == 0) return;

        try
        {
            var wlMin = this.parameters.Keys.Min();
            var wlMax = this.parameters.Keys.Max();

            DrawHorizontalLine(wlMin, wlMax);

            this.timeTable.SetColors();

            var maskingRanges = this.maskingRangeBox.MaskingRanges;
            var wavelengths = this.parameters.Keys.Order().ToArray();
            var masked = maskingRanges.GetMaskedPoints(wavelengths);
            var nextOfMasked = maskingRanges.GetNextOfMaskedPoints(wavelengths);

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
                (var min, var max) = DrawSpectrum(time, funcs, gradient[index++], masked, nextOfMasked);
                sigMin = Math.Min(sigMin, min);
                sigMax = Math.Max(sigMax, max);
            }

            this.axisX.Minimum = wlMin;
            this.axisX.Maximum = wlMax;
            this.axisY.Minimum = Math.Min(sigMin * 1.1, 0.0);
            this.axisY.Maximum = Math.Max(sigMax * 1.1, 0.0);

            AdjustAxesIntervals();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    } // private void DrawSpectra ()

    private (double Min, double Max) DrawSpectrum(double time, Dictionary<double, Func<double, double>> funcs, Color color, IEnumerable<double> masked, IEnumerable<double> nextOfMasked)
    {
        Series MakeSeries() => new()
        {
            ChartType = SeriesChartType.Line,
            MarkerStyle = MarkerStyle.Circle,
            MarkerSize = Program.SpectraMarkerSize,
            Color = color,
            BorderWidth = Program.SpectraLineWidth,
            LegendText = time.ToString("F2"),
        };

        var series = MakeSeries();

        var min = double.MaxValue;
        var max = double.MinValue;
        foreach (var wavelength in this.parameters.Keys.Order())
        {
            if (nextOfMasked.Contains(wavelength) && series.Points.Count > 0)
            {
                this.chart.Series.Add(series);
                series = MakeSeries();
            }
            if (masked.Contains(wavelength)) continue;

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
    } // private (double, double) DrawSpectrum (double, Dictionary<double, Func<double, double>>, Color, IEnumerable<double>, IEnumerable<double>)

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
            MarkerSize = Program.SpectraMarkerSize + 10,
            Color = color,
            BorderWidth = 2,
        };
        series.Points.AddXY(wavelength, signal);
        this.chart.Series.Add(series);
    } // private void HighlightWavelength (double)

    private void SelectWavelength(object? sender, MouseEventArgs e)
    {
        var result = this.chart.HitTest(e.X, e.Y);
        if (result.ChartElementType != ChartElementType.DataPoint) return;

        var point = result.Series.Points[result.PointIndex];
        Program.MainWindow.SelectWavelength(point.XValue);
    } // private void SelectWavelength (object?, MouseEventArgs)

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
            FileName = $"{Program.MainWindow.SampleName}_spectra.xlsx",
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var filename = sfd.FileName;
        var extension = Path.GetExtension(filename);
        var writer = GetSpreadSheetWriter(extension);

        var maskingRanges = this.maskingRangeBox.MaskingRanges.ToArray();

        try
        {
            using var _ = new NegativeSignHandler();
            writer.Parameters = this.Model.Parameters.Select(p => p.Name).ToArray();
            writer.Times = [.. this.timeTable.Times];

            foreach ((var wavelength, var parameters) in this.parameters)
            {
                if (maskingRanges.Any(r => r.Includes(wavelength))) continue;
                writer.AddRow(wavelength, parameters);
            }

            writer.Write(filename);

            ShowSavedNotification(filename);
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

    private void ExportToOrigin(object? sender, EventArgs e)
        => ExportToOrigin();

    private void ExportToOrigin()
    {
        if (this.modelId == Guid.Empty) return;
        if (this.parameters.Count == 0) return;
        if (this.timeTable.Rows.Count == 0) return;

        using var sfd = new SaveFileDialog
        {
            Title = "Export to Origin",
            Filter = "Origin Project|*.opju",
            FileName = $"{Program.MainWindow.SampleName}_spectra.opju",
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;
        var filename = sfd.FileName;

        try
        {
            using var origin = new OriginProject();

            if (origin.IsModified)
            {
                MessageBox.Show(
                    "The current Origin project has unsaved changes and will be closed without saving after pressing OK button. " +
                    "Save the project before exporting to Origin.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                origin.NewProject();
            }

            var book = origin.AddWorkbook();
            book.Name = "Data";

            var sheet = book.Worksheets[0];
            sheet.Name = "Spectra";

            var col_wl = sheet.Columns[0];
            col_wl.LongName = "Wavelength";
            col_wl.Units = "nm";
            col_wl.SetData(this.parameters.Keys.Cast<object>().ToArray());

            var timeUnit = this.TimeUnit;
            var funcs = this.parameters.Values.Select(p => this.Model.GetFunction(p)).ToArray();
            foreach ((var i, var time) in this.timeTable.Times.Enumerate())
            {
                var col_index = i + 1;
                var col = col_index < sheet.ColumnsCount ? sheet.Columns[col_index] : sheet.Columns.Add();
                col.LongName = $"{time} {timeUnit}";

                var values = this.parameters.Keys.Select((wl, i) => funcs[i](time)).Cast<object>().ToArray();
                col.SetData(values);
            }

            var graph = origin.AddGraph("Spectra", Program.OriginTemplateName);
            var layer = graph.Layers[0];
            var range = sheet.DataRange;
            layer.DataPlots.Add(range, PlotTypes.PlotLine);

            origin.Save(filename);
            ShowSavedNotification(filename);
        }
        catch
        {
            MessageBox.Show(
                "Failed to export to Origin.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    } // private void ExportToOrigin ()

    private static void ShowSavedNotification(string path)
    {
        var ext = Path.GetExtension(path);
        var launcher = AppLauncher.GetInstance(ext);

        var toast =
            new ToastNotification("Spectra saved successfully:")
            .AddText(path)
            .AddButton("OK");

        if (launcher is not null)
            toast.AddButton("Open", (_) => launcher.OpenFile(path));

        toast.Show();
    } // private static void ShowSavedNotification (string)

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
        if (string.IsNullOrWhiteSpace(this.maskingRangeBox.Text))
            this.maskingRangeBox.Text = string.Join(",", DetermineMaskingPoints([.. this.parameters.Keys]).Select(wl => wl.ToString("F1")));
        DrawSpectra();
    } // internal void SetParameters (IDictionary<double, double[]>)

    private static IEnumerable<double> DetermineMaskingPoints(IReadOnlyList<double> wavelengths)
    {
        if (wavelengths.Count < 2) return [];
        var allSteps = new double[wavelengths.Count - 1];
        for (var i = 0; i < allSteps.Length; i++)
            allSteps[i] = wavelengths[i + 1] - wavelengths[i];
        var steps = allSteps.Order().Reverse().ToArray();
        if (steps.Length < 3) return [];
        if (steps[0] >= steps[1] * 2)
        {
            var step = steps[0];
            var index = Array.IndexOf(allSteps, step);
            return [wavelengths[index] + step / 2];
        }
        return [];
    } // private static IEnumerable<double> DetermineMaskingPoints (IReadOnlyList<double>)

    private void SelectColorGradient(object? sender, EventArgs e)
    {
        var picker = new ColorGradientPicker(Program.GradientStart, Program.GradientEnd)
        {
            StartPosition = FormStartPosition.CenterParent,
        };
        if (picker.ShowDialog() != DialogResult.OK) return;
        Program.GradientStart = picker.StartColor;
        Program.GradientEnd = picker.EndColor;
    } // private void SelectColorGradient (object?, EventArgs)

    private void ChangeLineWidth(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not int width) return;
        Program.SpectraLineWidth = width;
    } // private void ChangeLineWidth (object?, EventArgs)

    private void ChangeMarkerSize(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not int size) return;
        Program.SpectraMarkerSize = size;
    } // private void ChangeMarkerSize (object?, EventArgs)

    private void SetAxisTitleFont(object? sender, EventArgs e)
        => this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;

    private void SetAxisLabelFont(object? sender, EventArgs e)
        => this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;

    private void CopyPlotsToClipboard(object? sender, EventArgs e)
        => CopyPlotsToClipboard();

    private void CopyPlotsToClipboard()
    {
        using var bitmap = new Bitmap(this.chart.Width, this.chart.Height);
        this.chart.DrawToBitmap(bitmap, new(0, 0, this.chart.Width, this.chart.Height));
        System.Windows.Forms.Clipboard.SetImage((Image)bitmap);
    } // private void CopyPlotsToClipboard ()
} // internal sealed class SpectraPreviewWindow : Form

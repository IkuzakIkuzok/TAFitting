
// (c) 2024 Kazuki KOHZUKI

using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Clipboard;
using TAFitting.Controls.Charting;
using TAFitting.Controls.Spectra;
using TAFitting.Data;
using TAFitting.Data.Solver;
using TAFitting.Model;

namespace TAFitting.Controls;

[DesignerCategory("Code")]
internal sealed class MainWindow : Form
{
    private const string TextBase = "TA Fitting";

    private readonly SplitContainer mainContainer, paramsContainer;

    private readonly CustomChart chart;
    private readonly Axis axisX, axisY;
    private readonly DisplayRangeSelector rangeSelector;
    private readonly CheckBox cb_invert;
    private bool suppressAutoInvert = false;

    private readonly ParametersTable parametersTable;

    private readonly ToolStripMenuItem menu_model;
    private Guid selectedModel = Guid.Empty;

    private Decays? decays;
    private readonly Series s_observed, s_fit;
    private ParametersTableRow? row;
    private string sampleName = string.Empty;
    private readonly CustomNumericUpDown nud_time0;

    private IReadOnlyDictionary<double, double[]> ParametersList
        => this.parametersTable.ParameterRows
               .ToDictionary(row => row.Wavelength, row => row.Parameters.ToArray());

    private double SelectedWavelength => this.row?.Wavelength ?? double.NaN;

    private readonly List<SpectraPreviewWindow> previewWindows = [];

    internal MainWindow()
    {
        this.Text = TextBase;
        this.Size = new Size(1200, 800);
        this.KeyPreview = true;

        var defaultModel = Program.DefaultModel;
        if (ModelManager.Models.ContainsKey(defaultModel))
            this.selectedModel = defaultModel;

        this.mainContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BorderStyle = BorderStyle.Fixed3D,
            Parent = this,
        };

        #region chart

        this.chart = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderlineColor = Color.Black,
            BorderlineWidth = 2,
            BorderlineDashStyle = ChartDashStyle.Solid,
            SuppressExceptions = true,
            Parent = this.mainContainer.Panel1,
        };

        this.axisX = new Axis()
        {
            Title = "Time (µs)",
            Minimum = 0.05,
            Maximum = 1000,
            LogarithmBase = 10,
            //Interval = 1,
            LabelStyle = new() { Format = "#.0e+0" },
        };
        this.axisY = new Axis()
        {
            Title = "ΔµOD",
            Minimum = 10,
            Maximum = 10000,
            LogarithmBase = 10,
            //Interval = 1,
            LabelStyle = new() { Format = "#.0e+0" },
        };

        this.axisX.MinorGrid.Enabled = this.axisY.MinorGrid.Enabled = true;
        //this.axisX.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        AddDummySeries();

        this.s_observed = new Series()
        {
            Color = Program.ObservedColor,
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_observed);

        this.s_fit = new Series()
        {
            Color = Program.FitColor,
            ChartType = SeriesChartType.Line,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_fit);

        #endregion chart

        #region params

        this.paramsContainer = new()
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BorderStyle = BorderStyle.FixedSingle,
            Parent = this.mainContainer.Panel2,
        };

        this.parametersTable = new()
        {
            Dock = DockStyle.Fill,
            Parent = this.paramsContainer.Panel1,
        };
        this.parametersTable.SelectedRowChanged += ChangeRow;
        this.parametersTable.CellValueChanged += ShowFit;
        this.parametersTable.CellValueChanged += UpdatePreviewsParameters;
        this.parametersTable.UserDeletedRow += RemoveDecay;
        if (this.selectedModel != Guid.Empty)
            this.parametersTable.SetColumns(ModelManager.Models[this.selectedModel].Model);

        #endregion params

        #region view options

        this.rangeSelector = new(this.axisX, this.axisY)
        {
            Top = 10,
            Left = 10,
            Parent = this.paramsContainer.Panel2,
        };
        this.chart.AxisXMaximum = (double)this.rangeSelector.Time.FromMinimum;
        this.chart.AxisXMinimum = (double)this.rangeSelector.Time.ToMaximum;
        this.chart.AxisYMaximum = (double)this.rangeSelector.Signal.FromMinimum;
        this.chart.AxisYMinimum = (double)this.rangeSelector.Signal.ToMaximum;

        this.cb_invert = new()
        {
            Text = "Invert vertical axis",
            Size = new(160, 20),
            Location = new(10, 80),
            Parent = this.paramsContainer.Panel2,
        };
        this.cb_invert.CheckedChanged += InvertMagnitude;
        this.cb_invert.CheckedChanged += ShowPlots;

        _ = new Label()
        {
            Text = "Time 0",
            Size = new(50, 20),
            Location = new(170, 82),
            Parent = this.paramsContainer.Panel2,
        };

        this.nud_time0 = new()
        {
            Size = new(80, 20),
            Location = new(220, 80),
            DecimalPlaces = 3,
            Increment = 0.1M,
            ScrollIncrement = 0.01M,
            Minimum = -1000,
            Maximum = 1000,
            Parent = this.paramsContainer.Panel2,
        };
        this.nud_time0.ValueChanged += ChangeTime0;

        _ = new Label()
        {
            Text = "µs",
            Size = new(20, 20),
            Location = new(305, 82),
            Parent = this.paramsContainer.Panel2,
        };

        #endregion view options

        #region menu

        this.MainMenuStrip = new()
        {
            Parent = this,
        };

        #region menu.file

        var menu_file = new ToolStripMenuItem("&File");
        this.MainMenuStrip.Items.Add(menu_file);

        var menu_fileOpen = new ToolStripMenuItem("&Open")
        {
            ShortcutKeys = Keys.Control | Keys.O,
        };
        menu_fileOpen.Click += LoadDecays;
        menu_file.DropDownItems.Add(menu_fileOpen);

        menu_file.DropDownItems.Add(new ToolStripSeparator());

        var menu_fileExit = new ToolStripMenuItem("E&xit")
        {
            ShortcutKeyDisplayString = "Alt+F4",
        };
        menu_fileExit.Click += (sender, e) => Close();
        menu_file.DropDownItems.Add(menu_fileExit);

        #endregion menu.file

        #region menu.view

        var menu_view = new ToolStripMenuItem("&View");
        this.MainMenuStrip.Items.Add(menu_view);

        var menu_viewColor = new ToolStripMenuItem("&Color");
        menu_view.DropDownItems.Add(menu_viewColor);

        var menu_viewColorObserved = new ToolStripMenuItem("&Observed");
        menu_viewColorObserved.Click += SetObservedColor;
        menu_viewColor.DropDownItems.Add(menu_viewColorObserved);

        var menu_viewColorFit = new ToolStripMenuItem("&Fit");
        menu_viewColorFit.Click += SetFitColor;
        menu_viewColor.DropDownItems.Add(menu_viewColorFit);

        var menu_viewFont = new ToolStripMenuItem("&Font");
        menu_view.DropDownItems.Add(menu_viewFont);

        var menu_viewFontAxisLabel = new ToolStripMenuItem("&Axis Label");
        menu_viewFont.DropDownItems.Add(menu_viewFontAxisLabel);
        menu_viewFontAxisLabel.Click += SelectAxisLabelFont;

        var menu_viewFontAxisTitle = new ToolStripMenuItem("&Axis Title");
        menu_viewFont.DropDownItems.Add(menu_viewFontAxisTitle);
        menu_viewFontAxisTitle.Click += SelectAxisTitleFont;

        #endregion menu.view

        #region menu.data

        var menu_data = new ToolStripMenuItem("&Data");
        this.MainMenuStrip.Items.Add(menu_data);

        var menu_dataPrevireSpec = new ToolStripMenuItem("Preview &spectra")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.S,
        };
        menu_dataPrevireSpec.Click += ShowSpectraPreview;
        menu_data.DropDownItems.Add(menu_dataPrevireSpec);

        var menu_dataFileNameFormat = new ToolStripMenuItem("&Filename format");
        menu_dataFileNameFormat.Click += EditFilenameFormat;
        menu_data.DropDownItems.Add(menu_dataFileNameFormat);

        menu_data.DropDownItems.Add(new ToolStripSeparator());

        var menu_dataLma = new ToolStripMenuItem("&Levenberg\u2013Marquardt");
        menu_data.DropDownItems.Add(menu_dataLma);

        var menu_dataLmaSelected = new ToolStripMenuItem("Selected row")
        {
            ShortcutKeys = Keys.Control | Keys.L,
        };
        menu_dataLmaSelected.Click += LevenbergMarquardtEstimationSelectedRow;
        menu_dataLma.DropDownItems.Add(menu_dataLmaSelected);

        var menu_dataLmaAll = new ToolStripMenuItem("All rows")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.L,
        };
        menu_dataLmaAll.Click += LevenbergMarquardtEstimationAllRows;
        menu_dataLma.DropDownItems.Add(menu_dataLmaAll);

        var menu_dataAutoFit = new ToolStripMenuItem("&Auto-fit")
        {
            Checked = Program.AutoFit,
        };
        menu_dataAutoFit.Click += ToggleAutoFit;
        menu_data.DropDownItems.Add(menu_dataAutoFit);

        menu_data.DropDownItems.Add(new ToolStripSeparator());

        var menu_dataPaste = new ToolStripMenuItem("&Paste table")
        {
            ShortcutKeys = Keys.Control | Keys.V,
        };
        menu_dataPaste.Click += PasteTable;
        menu_data.DropDownItems.Add(menu_dataPaste);

        #endregion menu.data

        #region menu.model

        this.menu_model = new ToolStripMenuItem("&Model");
        this.MainMenuStrip.Items.Add(this.menu_model);
        ModelManager.ModelsChanged += UpdateModelList;
        UpdateModelList();

        #endregion menu.model

        #region menu.help

        var menu_help = new ToolStripMenuItem("&Help");
        this.MainMenuStrip.Items.Add(menu_help);

        #endregion menu.help

        #endregion menu

        this.mainContainer.SplitterDistance = 750;
        this.mainContainer.Panel2MinSize = 420;

        this.paramsContainer.SplitterDistance = 600;
        this.paramsContainer.Panel2MinSize = 120;
    } // ctor ()

    override protected void OnShown(EventArgs e)
    {
        base.OnShown(e);

        this.rangeSelector.Time.FromChanged += AdjustXAxisInterval;
        this.rangeSelector.Time.ToChanged += AdjustXAxisInterval;
        this.rangeSelector.Time.LogarithmicChanged += AdjustXAxisInterval;
        this.rangeSelector.Signal.FromChanged += AdjustYAxisInterval;
        this.rangeSelector.Signal.ToChanged += AdjustYAxisInterval;
        this.rangeSelector.Signal.LogarithmicChanged += AdjustYAxisInterval;

        this.rangeSelector.Time.Logarithmic = true;
        this.rangeSelector.Signal.Logarithmic = true;
    } // override protected void OnShown (EventArgs)

    override protected void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Control && e.KeyCode == Keys.R)
            this.cb_invert.Checked = !this.cb_invert.Checked;
    } // override protected void OnKeyDown (KeyEventArgs)

    private string GetTitle()
    {
        var sb = new StringBuilder(TextBase);

        if (this.selectedModel != Guid.Empty)
        {
            var model = ModelManager.Models[this.selectedModel].Model;
            sb.Append(" - ");
            sb.Append(model.Name);
        }

        if (!string.IsNullOrEmpty(this.sampleName))
        {
            sb.Append(" | ");
            sb.Append(this.sampleName);
        }

        if (this.row is not null)
        {
            sb.Append(" (");
            sb.Append(this.row.Wavelength);
            sb.Append(" nm)");
        }

        return sb.ToString();
    } // private string GetTitle ()

    private void LoadDecays(object? sender, EventArgs e)
    {
        if (this.parametersTable.Edited)
        {
            var dr = MessageBox.Show(
                "The current data will be lost. Do you want to continue?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );
            if (dr != DialogResult.Yes) return;
        }

        LoadDecays();
    } // private void LoadDecays (object?, EventArgs)

    private void LoadDecays()
    {
        var ofd = new OpenFolderDialog()
        {
            Title = "Select a TAS data folder",
        };
        if (!(ofd.ShowDialog() ?? false)) return;

        this.row = null;

        var folderName = ofd.FolderName;
        this.sampleName = Path.GetFileName(folderName);
        Task.Run(() =>
        {
            // Temporary change the negative sign to U+002D
            // because double.Parse throws an exception with U+2212.
            using var _ = new NegativeSignHandler();

            try
            {
                this.decays = Decays.FromFolder(folderName);
                Invoke(() =>
                {
                    this.Text = GetTitle();
                    this.rangeSelector.Time.To = (decimal)this.decays.MaxTime;
                    this.nud_time0.Value = (decimal)this.decays.Time0;
                    MakeTable();
                });
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
        });
    } // private void LoadDecays ()

    private void AddDummySeries()
    {
        var dummy = new Series()
        {
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        dummy.Points.AddXY(1e-6, 1e-6);
        this.chart.Series.Add(dummy);
    } // private void AddDummySeries ()

    private void UpdateModelList(object? sender, EventArgs e)
        => UpdateModelList();

    private void UpdateModelList()
    {
        this.menu_model.DropDownItems.Clear();
        var models = ModelManager.Models.GroupBy(m => m.Value.Category);
        foreach (var category in models)
        {
            var categoryName = category.Key;
            if (string.IsNullOrEmpty(categoryName))
                categoryName = "Other";
            var categoryItem = new ToolStripMenuItem(categoryName);
            this.menu_model.DropDownItems.Add(categoryItem);

            foreach ((var guid, var model) in category)
            {
                var modelItem = new ToolStripMenuItem(model.Model.Name)
                {
                    Tag = guid,
                    Checked = guid == this.selectedModel,
                };
                modelItem.Click += SelectModel;
                categoryItem.DropDownItems.Add(modelItem);

                if (!ModelManager.EstimateProviders.TryGetValue(guid, out var providers)) continue;
                foreach (var provider in providers)
                {
                    var estimateItem = new ToolStripMenuItem(provider.Name);
                    modelItem.DropDownItems.Add(estimateItem);

                    var estimateAll = new ToolStripMenuItem("All rows")
                    {
                        Tag = provider,
                    };
                    estimateAll.Click += EstimateParametersAllRows;
                    estimateItem.DropDownItems.Add(estimateAll);

                    var estimateNotEdited = new ToolStripMenuItem("Not edited rows only")
                    {
                        Tag = provider,
                    };
                    estimateNotEdited.Click += EstimateParametersNotEditedRows;
                    estimateItem.DropDownItems.Add(estimateNotEdited);
                }
            }
        }
        
        this.menu_model.DropDownItems.Add(new ToolStripSeparator());

        var add_model = new ToolStripMenuItem()
        {
            Text = "Add model",
        };
        add_model.Click += AddModel;
        this.menu_model.DropDownItems.Add(add_model);
    } // private void UpdateModelList ()

    private void AddModel(object? sender, EventArgs e)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = "Assembly files|*.dll|All files|*.*",
            Title = "Select an assembly file",
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        try
        {
            ModelManager.Load(dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    } // private void AddModel (object?, EventArgs)

    private void SelectModel(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not Guid guid) return;

        if (guid == this.selectedModel) return;

        if (this.parametersTable.Edited)
        {
            var dr = MessageBox.Show(
                "Changing the model will clear the current fitting parameters. Do you want to continue?",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (dr != DialogResult.Yes) return;
        }

        foreach (var child in this.menu_model.DropDownItems)
        {
            if (child is not ToolStripMenuItem categoryItem) continue;
            foreach (var i in categoryItem.DropDownItems)
            {
                if (i is not ToolStripMenuItem modelItem) continue;
                modelItem.Checked = false;
            }
        }

        item.Checked = true;
        this.selectedModel = Program.DefaultModel = guid;
        var model = ModelManager.Models[guid].Model;
        this.rangeSelector.Time.Logarithmic = model.XLogScale;
        this.rangeSelector.Signal.Logarithmic = model.YLogScale;
        this.parametersTable.SetColumns(model);
        MakeTable();
        foreach (var preview in this.previewWindows)
            preview.ModelId = guid;
    } // private void SelectModel (object?, EventArgs)

    private void MakeTable()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;

        try
        {
            this.parametersTable.StopUpdateRSquared = true;
            this.parametersTable.Rows.Clear();
            foreach (var wl in this.decays.Keys)
            {
                var decay = this.decays[wl];
                var row = this.parametersTable.Add(wl, decay);
                row.Inverted = decay.OnlyAfterT0.Signals.Average() < 0;
            }

            if (Program.AutoFit)
                LevenbergMarquardtEstimationAllRows();
        }
        finally
        {
            this.parametersTable.StopUpdateRSquared = false;
        }

        UpdatePreviewsParameters();

        this.s_observed.Points.Clear();
        this.s_fit.Points.Clear();

        ShowPlots();
    } // private void MakeTable ()
    
    private void PasteTable(object? sender, EventArgs e)
        => PasteTable();

    private void PasteTable()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;

        var model = ModelManager.Models[this.selectedModel].Model;
        var parameters = model.Parameters.Select(p => p.Name);
        var rows = ClipboardHandler.GetRowsFromClipboard(parameters);
        foreach (var r in rows)
        {
            var wavelength = r.Wavelength;
            var row = this.parametersTable[wavelength];
            if (row is null) continue;
            row.Parameters = r.Parameters;
        }
    } // private void PasteTable ()

    private void ChangeRow(object? sender, ParametersTableSelectionChangedEventArgs e)
    {
        var movedToNewRow = (this.row?.Index ?? -1) != e.Row.Index;
        this.row = e.Row;

        if (movedToNewRow)
        {
            this.suppressAutoInvert = true;
            this.cb_invert.Checked = e.Row.Inverted;
            this.suppressAutoInvert = false;
        }

        this.Text = GetTitle();

        ShowPlots();
        UpdatePreviewsSelectedWavelength();
    } // private void ChangeRow (object?, ParametersTableSelectionChangedEventArgs)

    private void RemoveDecay(object? sender, DataGridViewRowEventArgs e)
    {
        if (this.decays is null) return;
        if (e.Row is not ParametersTableRow row) return;
        var wavelength = row.Wavelength;
        this.decays.Remove(wavelength);
        ShowPlots();
        UpdatePreviewsParameters();
    } // private void RemoveDecay (object?, DataGridViewRowEventArgs)

    private void InvertMagnitude(object? sender, EventArgs e)
    {
        if (this.suppressAutoInvert) return;
        InvertMagnitude();
    } // private void InvertMagnitude (object?, EventArgs)

    private void InvertMagnitude()
        => this.row?.InvertMagnitude();

    private void ChangeTime0(object? sender, EventArgs e)
        => ChangeTime0();

    private void ChangeTime0()
    {
        if (this.decays is null) return;
        var t0 = (double)this.nud_time0.Value;
        this.decays.Time0 = t0;
        ShowPlots();
    } // private void ChangeTime0 ()

    private void ShowPlots(object? sender, EventArgs e)
        => ShowPlots();

    private void ShowPlots()
    {
        ShowObserved();
        ShowFit();
    } // private void ShowPlots ()

    private void ShowObserved()
    {
        if (this.row is null) return;
        var decay = this.row.Decay;

        if (this.cb_invert.Checked)
            decay = decay.Inverted;

        this.s_observed.Points.Clear();
        this.s_observed.Points.AddDecay(decay);
    } // private void ShowObserved ()

    private void ShowFit(object? sender, EventArgs e)
        => ShowFit();

    private void ShowFit()
    {
        if (this.selectedModel == Guid.Empty) return;
        var model = ModelManager.Models[this.selectedModel].Model;
        if (this.row is null) return;
        var decay = this.row.Decay;

        var parameters = this.row.Parameters;
        var func = model.GetFunction(parameters);
        var invert = this.cb_invert.Checked ? -1 : 1;
        var times = decay.Times;
        var signals = times.Select(t => func(t) * invert);

        this.s_fit.Points.Clear();
        this.s_fit.Points.AddDecay(times, signals);
    } // private void ShowFit ()

    private void AdjustXAxisInterval(object? sender, EventArgs e)
    {
        this.chart.ChartAreas[0].RecalculateAxesScale();
        var pixelInterval = this.axisX.IsLogarithmic ? 30 : 100;
        this.axisX.AdjustAxisInterval(pixelInterval);
    } // private void AdjustXAxisInterval (object?, EventArgs)

    private void AdjustYAxisInterval(object? sender, EventArgs e)
    {
        this.chart.ChartAreas[0].RecalculateAxesScale();
        var pixelInterval = this.axisY.IsLogarithmic ? 30 : 100;
        this.axisY.AdjustAxisInterval(pixelInterval);
    } // private void AdjustYAxisInterval (object?, EventArgs)

    private void LevenbergMarquardtEstimationSelectedRow(object? sender, EventArgs e)
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        if (this.row is null) return;
        LevenbergMarquardtEstimation([this.row]);
    } // private void LevenbergMarquardtEstimationSelectedRow (object?, EventArgs)

    private void LevenbergMarquardtEstimationAllRows(object? sender, EventArgs e)
        => LevenbergMarquardtEstimationAllRows();

    private void LevenbergMarquardtEstimationAllRows()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        LevenbergMarquardtEstimation(this.parametersTable.ParameterRows);
    } // private void LevenbergMarquardtEstimationAllRows ()

    private async void LevenbergMarquardtEstimation(IEnumerable<ParametersTableRow> rows)
    {
        var text = this.Text;
        this.Text += " - Fitting...";
        var model = ModelManager.Models[this.selectedModel];
        var source = rows.ToArray();

        var start = DateTime.Now;

        try
        {
            this.parametersTable.StopUpdateRSquared = true;
            if (source.Length >= Program.ParallelThreshold)
            {
                var results = new ConcurrentDictionary<ParametersTableRow, IReadOnlyList<double>>();
                await Task.Run(() => Parallel.ForEach(source, (row) =>
                {
                    var parameters = LevenbergMarquardtEstimation(row);
                    results.TryAdd(row, parameters);
                }));

                // updating parameters on the UI thread
                // Invoke() in each iteration is too slow
                foreach (var (row, parameters) in results)
                {
                    if (row.DataGridView is null) continue;  // Removed from the table during the fitting
                    row.Parameters = parameters;
                }
            }
            else
            {
                foreach (var row in rows)
                    row.Parameters = LevenbergMarquardtEstimation(row);
            }
        }
        finally
        {
            this.parametersTable.StopUpdateRSquared = false;
        }

        var elapsed = DateTime.Now - start;

        this.Text = text;
        FadingMessageBox.Show(
            $"Fitting completed in {elapsed.TotalSeconds:F1} seconds.",
            0.8, 1000, 75, 0.1
        );
    } // private void LevenbergMarquardtEstimation (IEnumerable<ParametersTableRow>)

    private IReadOnlyList<double> LevenbergMarquardtEstimation(ParametersTableRow row)
    {
        var model = ModelManager.Models[this.selectedModel].Model;
        var decay = row.Decay.OnlyAfterT0;

        var lma = new LevenbergMarquardt(model, decay.Times, decay.Signals, row.Parameters)
        {
            MaxIteration = Program.MaxIterations,
        };
        lma.Fit();
        return lma.Parameters;
    } // private void LevenbergMarquardtEstimation (ParametersTableRow)

    private void ToggleAutoFit(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        Program.AutoFit = item.Checked = !item.Checked;
    } // private void ToggleAutoFit (object?, EventArgs)

    private void EstimateParametersAllRows(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not IEstimateProvider estimateProvider) return;
        if (!estimateProvider.SupportedModels.Contains(this.selectedModel)) return;
        EstimateParameters(estimateProvider, this.parametersTable.ParameterRows);
    } // private void EstimateParametersAllRows (object?, EventArgs)

    private void EstimateParametersNotEditedRows(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not IEstimateProvider estimateProvider) return;
        if (!estimateProvider.SupportedModels.Contains(this.selectedModel)) return;
        EstimateParameters(estimateProvider, this.parametersTable.NotEditedRows);
    } // private void EstimateParametersNotEditedRows (object?, EventArgs)

    private void EstimateParameters(IEstimateProvider estimateProvider, IEnumerable<ParametersTableRow> rows)
    {
        foreach (var row in rows)
        {
            var decay = row.Decay;

            var parameters = estimateProvider.EstimateParameters(decay.Times, decay.Signals, this.selectedModel);
            row.Parameters = parameters;
        }
    } // private void EstimateParametersAllRows (IEstimateProvider)

    private void ShowSpectraPreview(object? sender, EventArgs e)
        => ShowSpectraPreview();

    private void ShowSpectraPreview()
    {
        var preview = new SpectraPreviewWindow(this.ParametersList)
        {
            ModelId = this.selectedModel,
            SelectedWavelength = this.SelectedWavelength,
        };
        this.previewWindows.Add(preview);
        preview.FormClosed += (s, e) => this.previewWindows.Remove(preview);
        preview.Show();
    } // private void ShowSpectraPreview ()

    private void UpdatePreviewsParameters(object? sender, EventArgs e)
        => UpdatePreviewsParameters();

    private void UpdatePreviewsParameters()
    {
        foreach (var preview in this.previewWindows)
            preview.SetParameters(this.ParametersList);
    } // private void UpdatePreviewsParameters ()

    private void UpdatePreviewsSelectedWavelength()
    {
        var wavelength = this.SelectedWavelength;
        foreach (var preview in this.previewWindows)
            preview.SelectedWavelength = wavelength;
    } // private void UpdatePreviewsSelectedWavelength ()

    private static void EditFilenameFormat(object? sender, EventArgs e)
    {
        using var fnfd = new FileNameFormatDialog()
        {
            AMinusBFormat = Program.AMinusBSignalFormat,
            BFormat = Program.BSignalFormat,
        };
        if (fnfd.ShowDialog() != DialogResult.OK) return;

        Program.AMinusBSignalFormat = fnfd.AMinusBFormat;
        Program.BSignalFormat = fnfd.BFormat;
    } // private static void EditFilenameFormat (object?, EventArgs)

    #region change appearance

    private void SetObservedColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.ObservedColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_observed.Color = Program.ObservedColor = cd.Color;
    } // private void SetObservedColor (object?, EventArgs)

    private void SetFitColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.FitColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_fit.Color = Program.FitColor = cd.Color;
    } // private void SetFitColor (object?, EventArgs)

    private void SelectAxisLabelFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisLabelFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont = fd.Font;
    } // private void SelectAxisLabelFont (object?, EventArgs)

    private void SelectAxisTitleFont(object? sender, EventArgs e)
    {
        using var fd = new FontDialog()
        {
            Font = Program.AxisTitleFont,
        };
        if (fd.ShowDialog() != DialogResult.OK) return;
        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont = fd.Font;
    } // private void SelectAxisTitleFont (object?, EventArgs)

    #endregion change appearance
} // internal sealed class MainWindow : Form

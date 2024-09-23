
// (c) 2024 Kazuki KOHZUKI

using Microsoft.Win32;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Clipboard;
using TAFitting.Controls.Charting;
using TAFitting.Controls.Spectra;
using TAFitting.Data;
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
        this.parametersTable.CellValueChanged += UpdatePreviews;
        if (this.selectedModel != Guid.Empty)
            this.parametersTable.SetColumns(ModelManager.Models[this.selectedModel]);

        #endregion params

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

    private void LoadDecays(object? sender, EventArgs e)
        => LoadDecays();

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
                    this.Text = $"{TextBase} - {this.sampleName}";
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
        var models = ModelManager.Models;
        foreach ((var guid, var model) in models)
        {
            var modelItem = new ToolStripMenuItem(model.Name)
            {
                Tag = guid,
                Checked = guid == this.selectedModel,
            };
            modelItem.Click += SelectModel;
            this.menu_model.DropDownItems.Add(modelItem);

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

        if ((this.decays?.Count ?? 0) > 0 && this.selectedModel != Guid.Empty)
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
            if (child is not ToolStripMenuItem menuItem) continue;
            menuItem.Checked = false;
        }

        item.Checked = true;
        this.selectedModel = Program.DefaultModel = guid;
        var model = ModelManager.Models[guid];
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

        this.parametersTable.Rows.Clear();
        foreach (var wl in this.decays.Keys)
        {
            var row = this.parametersTable.Add(wl);

            var decay = this.decays[wl];
            var signals = decay.Signals;
            var positives = signals.Where(s => s > 0).Count();
            var negatives = signals.Where(s => s < 0).Count();
            row.Inverted = negatives > positives;
        }
        UpdatePreviews();

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

        var model = ModelManager.Models[this.selectedModel];
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

        this.Text = $"{TextBase} - {this.sampleName} ({e.Row.Wavelength} nm)";

        ShowPlots();
    } // private void ChangeRow (object?, ParametersTableSelectionChangedEventArgs)

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
        var wavelength = this.row.Wavelength;
        var decay = this.decays?[wavelength];
        if (decay is null) return;

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
        var model = ModelManager.Models[this.selectedModel];
        if (this.row is null) return;
        var wavelength = this.row.Wavelength;
        var decay = this.decays?[wavelength];
        if (decay is null) return;

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
            var wavelength = row.Wavelength;
            var decay = this.decays?[wavelength];
            if (decay is null) continue;

            var parameters = estimateProvider.EstimateParameters(decay.Times.ToList(), decay.Signals.ToList(), this.selectedModel);
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
        };
        this.previewWindows.Add(preview);
        preview.FormClosed += (s, e) => this.previewWindows.Remove(preview);
        preview.Show();
    } // private void ShowSpectraPreview ()

    private void UpdatePreviews(object? sender, EventArgs e)
        => UpdatePreviews();

    private void UpdatePreviews()
    {
        foreach (var preview in this.previewWindows)
            preview.SetParameters(this.ParametersList);
    } // private void UpdatePreviews ()

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

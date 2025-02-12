
// (c) 2024-2025 Kazuki KOHZUKI

using DisposalGenerator;
using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Clipboard;
using TAFitting.Controls.Charting;
using TAFitting.Controls.LinearCombination;
using TAFitting.Controls.Spectra;
using TAFitting.Data;
using TAFitting.Data.Solver;
using TAFitting.Data.Solver.SIMD;
using TAFitting.Filter;
using TAFitting.Model;

namespace TAFitting.Controls;

/// <summary>
/// Represents the main window.
/// </summary>
[DesignerCategory("Code")]
[AutoDisposal]
internal sealed partial class MainWindow : Form
{
    private static readonly Guid usTasDialog = new("E7757DD6-FDB0-4670-BD39-C499E9F46174");
    private static readonly Guid fsTasDialog = new("4801BD77-1083-4A89-B489-394BEC83D1C9");

    private readonly SplitContainer mainContainer, paramsContainer;

    private readonly CustomChart chart;
    private readonly Axis axisX, axisY;
    private readonly DisplayRangeSelector rangeSelector;
    private readonly CheckBox cb_invert;
    private bool suppressAutoInvert = false;

    private readonly ParametersTable parametersTable;

    private readonly ToolStripMenuItem menu_filter, menu_model;
    private readonly ToolStripMenuItem menu_autoApplyFilter, menu_hideOriginal, menu_unfilter;
    private Guid selectedModel = Guid.Empty;

    private Decays? decays;
    private readonly Series s_observed, s_filtered, s_fit;
    private bool stopDrawing = false;
    private ParametersTableRow? row;
    private string sampleName = string.Empty;
    private readonly CustomNumericUpDown nud_time0;
    private readonly Label lb_t0, lb_timeUnit;

    /// <summary>
    /// Gets the sample name.
    /// </summary>
    internal string SampleName
        => this.sampleName;

    /// <summary>
    /// Gets the parameters list corresponding to the wavelengths.
    /// </summary>
    /// <value>A dictionary that contains the wavelengths as the keys and the parameters as the values.</value>
    private IReadOnlyDictionary<double, double[]> ParametersList
        => this.parametersTable.ParameterRows
               .ToDictionary(row => row.Wavelength, row => row.Parameters.ToArray());

    /// <summary>
    /// Gets the selected wavelength.
    /// </summary>
    private double SelectedWavelength => this.row?.Wavelength ?? double.NaN;

    /// <summary>
    /// Gets the selected model.
    /// </summary>
    /// <value>The selected model if is valid; otherwise, <see langword="null"/>.</value>
    private IFittingModel? SelectedModel => ModelManager.Models.TryGetValue(this.selectedModel, out var model) ? model.Model : null;

    private readonly List<SpectraPreviewWindow> previewWindows = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    internal MainWindow()
    {
        this.Text = Program.AppName;
        this.Size = new Size(1200, 800);
        this.KeyPreview = true;

        this.mainContainer = new()
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
        //this.axisX.MinorGrid.Interval = this.axisY.MinorGrid.Interval = 1;
        this.axisX.MinorGrid.LineColor = this.axisY.MinorGrid.LineColor = Color.LightGray;

        this.axisX.TitleFont = this.axisY.TitleFont = Program.AxisTitleFont;
        this.axisX.LabelStyle.Font = this.axisY.LabelStyle.Font = Program.AxisLabelFont;

        this.chart.ChartAreas.Add(new ChartArea()
        {
            AxisX = this.axisX,
            AxisY = this.axisY,
        });

        AddDummySeries();

        this.s_observed = new()
        {
            Color = Program.ObservedColor,
            ChartType = SeriesChartType.Point,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_observed);

        this.s_filtered = new()
        {
            Color = Program.FilteredColor,
            ChartType = SeriesChartType.Line,
            IsVisibleInLegend = false,
            IsXValueIndexed = false,
        };
        this.chart.Series.Add(this.s_filtered);

        this.s_fit = new()
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

        this.lb_t0 = new()
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

        this.lb_timeUnit = new()
        {
            Text = "µs",
            Size = new(20, 20),
            Location = new(305, 82),
            Parent = this.paramsContainer.Panel2,
        };

        #endregion view options

        var defaultModel = Program.DefaultModel;
        if (ModelManager.Models.ContainsKey(defaultModel))
            SelectModel(defaultModel);

        #region menu

        this.MainMenuStrip = new()
        {
            ShowItemToolTips = true,
            Parent = this,
        };

        #region menu.file

        var menu_file = new ToolStripMenuItem("&File");
        this.MainMenuStrip.Items.Add(menu_file);

        var menu_fileOpenMicrosecond = new ToolStripMenuItem("&Open µs-TAS")
        {
            ShortcutKeys = Keys.Control | Keys.O,
            ToolTipText = "Open a µs-TAS data folder",
        };
        menu_fileOpenMicrosecond.Click += LoadMicrosecondDecays;
        menu_file.DropDownItems.Add(menu_fileOpenMicrosecond);

        var menu_fileOpenFemtosecond = new ToolStripMenuItem("&Open fs-TAS")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.O,
            ToolTipText = "Open a fs-TAS data file",
        };
        menu_fileOpenFemtosecond.Click += LoadFemtosecondDecays;
        menu_file.DropDownItems.Add(menu_fileOpenFemtosecond);

        menu_file.DropDownItems.Add(new ToolStripSeparator());

        var menu_fileExit = new ToolStripMenuItem("E&xit")
        {
            ShortcutKeyDisplayString = "Alt+F4",
            ToolTipText = "Exit the application",
        };
        menu_fileExit.Click += (sender, e) => Close();
        menu_file.DropDownItems.Add(menu_fileExit);

        #endregion menu.file

        #region menu.view

        var menu_view = new ToolStripMenuItem("&View");
        this.MainMenuStrip.Items.Add(menu_view);

        var menu_viewColor = new ToolStripMenuItem("&Color")
        {
            ToolTipText = "Change the colors",
        };
        menu_view.DropDownItems.Add(menu_viewColor);

        var menu_viewColorObserved = new ToolStripMenuItem("&Observed")
        {
            ToolTipText = "Change the color of the observed data",
        };
        menu_viewColorObserved.Click += SetObservedColor;
        menu_viewColor.DropDownItems.Add(menu_viewColorObserved);

        var menu_viewColorFiltered = new ToolStripMenuItem("&Filtered")
        {
            ToolTipText = "Change the color of the filtered data",
        };
        menu_viewColorFiltered.Click += SetFilteredColor;
        menu_viewColor.DropDownItems.Add(menu_viewColorFiltered);

        var menu_viewColorFit = new ToolStripMenuItem("&Fit")
        {
            ToolTipText = "Change the color of the fitting curve",
        };
        menu_viewColorFit.Click += SetFitColor;
        menu_viewColor.DropDownItems.Add(menu_viewColorFit);

        var menu_viewFont = new ToolStripMenuItem("&Font")
        {
            ToolTipText = "Change the fonts",
        };
        menu_view.DropDownItems.Add(menu_viewFont);

        var menu_viewFontAxisLabel = new ToolStripMenuItem("&Axis Label")
        {
            ToolTipText = "Change the font of the axis labels",
        };
        menu_viewFont.DropDownItems.Add(menu_viewFontAxisLabel);
        menu_viewFontAxisLabel.Click += SelectAxisLabelFont;

        var menu_viewFontAxisTitle = new ToolStripMenuItem("&Axis Title")
        {
            ToolTipText = "Change the font of the axis titles",
        };
        menu_viewFont.DropDownItems.Add(menu_viewFontAxisTitle);
        menu_viewFontAxisTitle.Click += SelectAxisTitleFont;

        #endregion menu.view

        #region menu.data

        var menu_data = new ToolStripMenuItem("&Data");
        this.MainMenuStrip.Items.Add(menu_data);

        var menu_dataPrevireSpec = new ToolStripMenuItem("Preview &spectra")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.S,
            ToolTipText = "Show the spectra preview window",
        };
        menu_dataPrevireSpec.Click += ShowSpectraPreview;
        menu_data.DropDownItems.Add(menu_dataPrevireSpec);

        var menu_dataFileNameFormat = new ToolStripMenuItem("&Filename format")
        {
            ToolTipText = "Change the filename format",
        };
        menu_dataFileNameFormat.Click += EditFilenameFormat;
        menu_data.DropDownItems.Add(menu_dataFileNameFormat);

        menu_data.DropDownItems.Add(new ToolStripSeparator());

        var menu_dataUndo = new ToolStripMenuItem("&Undo")
        {
            ShortcutKeys = Keys.Control | Keys.Z,
            ToolTipText = "Undo the last change",
        };
        menu_dataUndo.Click += Undo;
        menu_data.DropDownItems.Add(menu_dataUndo);

        var menu_dataRedo = new ToolStripMenuItem("&Redo")
        {
            ShortcutKeys = Keys.Control | Keys.Y,
            ToolTipText = "Redo the last change",
        };
        menu_dataRedo.Click += Redo;
        menu_data.DropDownItems.Add(menu_dataRedo);

        menu_data.DropDownOpening += (sender, e) =>
        {
            menu_dataUndo.Enabled = this.parametersTable.CanUndo;
            menu_dataRedo.Enabled = this.parametersTable.CanRedo;
        };

        menu_data.DropDownItems.Add(new ToolStripSeparator());

        var menu_dataPaste = new ToolStripMenuItem("&Paste table")
        {
            ShortcutKeys = Keys.Control | Keys.V,
            ToolTipText = "Paste the table from the clipboard",
        };
        menu_dataPaste.Click += PasteTable;
        menu_data.DropDownItems.Add(menu_dataPaste);

        #endregion menu.data

        #region menu.filter
		 
        this.menu_filter = new("&Filter");
        this.MainMenuStrip.Items.Add(this.menu_filter);

        this.menu_autoApplyFilter = new("&Auto-apply")
        {
            Checked = Program.AutoApplyFilter,
            ToolTipText = "Automatically apply the filter",
        };
        this.menu_autoApplyFilter.Click += ToggleAutoApplyFilter;

        this.menu_hideOriginal = new("&Hide observed")
        {
            Checked = Program.HideOriginalData,
            ToolTipText = "Hide the original data",
        };
        this.menu_hideOriginal.Click += ToggleHideOriginal;

        this.menu_unfilter = new("&Unfilter")
        {
            ToolTipText = "Remove the filter",
        };
        
        var menu_unfilterSelectedRow = new ToolStripMenuItem("&Selected row")
        {
            ToolTipText = "Remove the filter from the selected row",
        };
        menu_unfilterSelectedRow.Click += UnfilterSelectedRow;
        this.menu_unfilter.DropDownItems.Add(menu_unfilterSelectedRow);

        var menu_unfilterAll = new ToolStripMenuItem("&All rows")
        {
            ToolTipText = "Remove the filter from all rows",
        };
        menu_unfilterAll.Click += UnfilterAll;
        this.menu_unfilter.DropDownItems.Add(menu_unfilterAll);

        FilterManager.FiltersChanged += UpdateFilterList;
        UpdateFilterList();

        #endregion menu.filter

        #region menu.model

        this.menu_model = new ToolStripMenuItem("&Model");
        this.MainMenuStrip.Items.Add(this.menu_model);
        ModelManager.ModelsChanged += UpdateModelList;
        UpdateModelList();

        #endregion menu.model

        #region menu.fit

        var menu_fit = new ToolStripMenuItem("&Fit");
        this.MainMenuStrip.Items.Add(menu_fit);

        var menu_fitAverage = new ToolStripMenuItem("Take average")
        {
            ShortcutKeys = Keys.Control | Keys.M,
            ToolTipText = "Fit the data using the average of the observed data",
        };
        menu_fitAverage.Click += TakeAverage;
        menu_file.DropDownItems.Add(menu_fitAverage);

        var menu_fitLma = new ToolStripMenuItem("&Levenberg\u2013Marquardt")
        {
            ToolTipText = "Fit the data using the Levenberg\u2013Marquardt algorithm",
        };
        menu_fit.DropDownItems.Add(menu_fitLma);

        var menu_fitLmaSelected = new ToolStripMenuItem("Selected row")
        {
            ShortcutKeys = Keys.Control | Keys.L,
            ToolTipText = "Fit the selected row",
        };
        menu_fitLmaSelected.Click += LevenbergMarquardtEstimationSelectedRow;
        menu_fitLma.DropDownItems.Add(menu_fitLmaSelected);

        var menu_fitLmaAll = new ToolStripMenuItem("All rows")
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.L,
            ToolTipText = "Fit all rows",
        };
        menu_fitLmaAll.Click += LevenbergMarquardtEstimationAllRows;
        menu_fitLma.DropDownItems.Add(menu_fitLmaAll);

        var menu_fitAuto = new ToolStripMenuItem("&Auto-fit")
        {
            Checked = Program.AutoFit,
            ToolTipText = "Automatically fit the data after loading",
        };
        menu_fitAuto.Click += ToggleAutoFit;
        menu_fit.DropDownItems.Add(menu_fitAuto);

        #endregion menu.fit

        #region menu.help

        var menu_help = new ToolStripMenuItem("&Help");
        this.MainMenuStrip.Items.Add(menu_help);

        var menu_helpGitHub = new ToolStripMenuItem("Open &GitHub")
        {
            ToolTipText = "Open the GitHub repository",
        };
        menu_helpGitHub.Click += (sender, e) => Program.OpenGitHub();
        menu_help.DropDownItems.Add(menu_helpGitHub);

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

        this.chart.SizeChanged += AdjustAxesIntervals;
    } // override protected void OnShown (EventArgs)

    override protected void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Control && e.KeyCode == Keys.R)
            this.cb_invert.Checked = !this.cb_invert.Checked;
    } // override protected void OnKeyDown (KeyEventArgs)

    /// <summary>
    /// Gets the title of the window.
    /// </summary>
    /// <returns>The title of the window representing the current state.</returns>
    private string GetTitle(string? additional = null)
    {
        var sb = new StringBuilder(Program.AppName);

        var model = this.SelectedModel;
        if (model is not null)
        {
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

        if (!string.IsNullOrEmpty(additional))
        {
            sb.Append(" - ");
            sb.Append(additional);
        }

        return sb.ToString();
    } // private string GetTitle ([string])

    #region Data loading

    /// <summary>
    /// Checks whether the current data will be lost and asks the user to continue.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the data has not been edited or the user wants to continue; otherwise, <see langword="false"/>.
    /// </returns>
    private bool CheckOverwriteDecays()
    {
        if (!this.parametersTable.Edited) return true;

        var dr = MessageBox.Show(
            "The current data will be lost. Do you want to continue?",
            "Warning",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        );
        return dr == DialogResult.Yes;
    } // private bool CheckOverwriteDecays ()

    private void LoadMicrosecondDecays(object? sender, EventArgs e)
    {
        if (!CheckOverwriteDecays()) return;
        LoadMicrosecondDecays();
    } // private void LoadMicrosecondDecays (object?, EventArgs)

    /// <summary>
    /// Loads the microsecond decay data.
    /// </summary>
    private void LoadMicrosecondDecays()
    {
        var ofd = new OpenFolderDialog()
        {
            Title = "Select a µs-TAS data folder",
            ClientGuid = Program.Config.SeparateFileDialogState ? usTasDialog : Program.FileDialogCommonId,
        };
        if (!(ofd.ShowDialog() ?? false)) return;

        LoadMicrosecondDecays(ofd.FolderName);
    } // private void LoadMicrosecondDecays ()

    /// <summary>
    /// Loads the microsecond decay data from the specified path.
    /// </summary>
    /// <param name="path">The path to the folder from which the decay data is loaded.</param>
    internal void LoadMicrosecondDecays(string path)
        => LoadDecays(path, Decays.MicrosecondFromFolder);

    private void LoadFemtosecondDecays(object? sender, EventArgs e)
    {
        if (!CheckOverwriteDecays()) return;
        LoadFemtosecondDecays();
    } // private void LoadFemtosecondDecays (object?, EventArgs)

    /// <summary>
    /// Loads the femtosecond decay data.
    /// </summary>
    private void LoadFemtosecondDecays()
    {
        using var ofd = new System.Windows.Forms.OpenFileDialog()
        {
            Filter = ExtensionFilter.CsvFiles,
            Title = "Select a fs-TAS data file",
            ClientGuid = Program.Config.SeparateFileDialogState ? fsTasDialog : Program.FileDialogCommonId,
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        LoadFemtosecondDecays(ofd.FileName);
    } // private void LoadFemtosecondDecays ()

    /// <summary>
    /// Loads the femtosecond decay data from the specified path.
    /// </summary>
    /// <param name="path">The path to the file from which the decay data is loaded.</param>
    internal void LoadFemtosecondDecays(string path)
        => LoadDecays(path, Decays.FemtosecondFromFile);

    /// <summary>
    /// Loads the decay data from the specified path.
    /// </summary>
    /// <param name="path">The path to the file or the folder from which the decay data is loaded.</param>
    /// <param name="decaysLoader">The function to load the decay data from the file or the folder.</param>
    private void LoadDecays(string path, Func<string, Decays> decaysLoader)
    {
        var old_sample = this.sampleName;

        this.row = null;
        this.sampleName = Path.GetFileName(path);
        this.Text = GetTitle("Loading...");
        Task.Run(() =>
        {
            // Temporary change the negative sign to U+002D
            // because double.Parse throws an exception with U+2212.
            using var _ = new NegativeSignHandler();

            try
            {
                this.decays = decaysLoader(path);
                Invoke(() =>
                {
                    this.Text = GetTitle();

                    this.axisX.Title = $"Time ({this.decays.TimeUnit})";
                    this.axisY.Title = this.decays.SignalUnit;
                    this.rangeSelector.Time.Text = $"Time ({this.decays.TimeUnit}):";
                    this.rangeSelector.Signal.Text = $"{this.decays.SignalUnit}:";

                    this.rangeSelector.Time.To = (decimal)this.decays.MaxTime;
                    this.rangeSelector.Signal.To = (decimal)(this.decays.MaxAbsSignal * 1.5);

                    // The table rows must be cleared before the t0 is set.
                    this.parametersTable.Rows.Clear();
                    // The t0 must be set before the filter is applied.
                    this.nud_time0.Value = (decimal)this.decays.Time0;

                    if (Program.AutoApplyFilter)
                    {
                        var filter = FilterManager.DefaultFilter;
                        if (filter is not null)
                        {
                            foreach (var decay in this.decays.Values)
                                decay.Filter(filter);
                        }
                    }

                    MakeTable();
                    this.parametersTable.ClearUndoBuffer();
                    UpdatePreviewsUnits();
                });
            }
            catch (Exception e)
            {
                this.sampleName = old_sample;
                Invoke(() =>
                {
                    this.Text = GetTitle();
                });

                MessageBox.Show(
                    e.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        });
    } // private void LoadDecays (string, Func<string, Decays>)

    #endregion Data loading

    #region filters

    private void UpdateFilterList(object? sender, EventArgs e)
        => UpdateFilterList();

    private void UpdateFilterList()
    {
        this.menu_filter.DropDownItems.Clear();
        var filters = FilterManager.Filters;
        foreach ((var guid, var filter) in filters)
        {
            var item = new ToolStripMenuItem(filter.Name)
            {
                ToolTipText = filter.Description,
            };
            this.menu_filter.DropDownItems.Add(item);

            var applySelectedRow = new ToolStripMenuItem("Apply to selected row")
            {
                Tag = filter,
            };
            applySelectedRow.Click += ApplyFilterSelectedRow;
            item.DropDownItems.Add(applySelectedRow);

            var applyAllRows = new ToolStripMenuItem("Apply to all rows")
            {
                Tag = filter,
            };
            applyAllRows.Click += ApplyFilterAllRows;
            item.DropDownItems.Add(applyAllRows);

            if (guid == Program.DefaultFilter)
            {
                applySelectedRow.ShortcutKeys = Keys.Control | Keys.F;
                applyAllRows.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
            }
        }

        this.menu_filter.DropDownItems.Add(new ToolStripSeparator());

        this.menu_filter.DropDownItems.Add(this.menu_autoApplyFilter);
        this.menu_filter.DropDownItems.Add(this.menu_hideOriginal);
        this.menu_filter.DropDownItems.Add(this.menu_unfilter);

        var add_filter = new ToolStripMenuItem()
        {
            Text = "Add filter",
            ToolTipText = "Add a new filter",
        };
        add_filter.Click += AddFilter;
        this.menu_filter.DropDownItems.Add(add_filter);
    } // private void UpdateFilterList ()

    private void ApplyFilterSelectedRow(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not IFilter filter) return;
        if (this.row is null) return;
        ApplyFilter(filter, [this.row]);
    } // private void ApplyFilterSelectedRow (object?, EventArgs)

    private void ApplyFilterAllRows(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not IFilter filter) return;
        ApplyFilter(filter, this.parametersTable.ParameterRows);
    } // private void ApplyFilterAllRows (object?, EventArgs)

    private void ApplyFilter(IFilter filter, IEnumerable<ParametersTableRow> rows)
    {
        foreach (var row in rows)
        {
            var decay = row.Decay;
            decay?.Filter(filter);
        } // foreach (var row in rows)

        ShowPlots();
    } // private void ApplyFilter (IFilter, IEnumerable<ParametersTableRow>)

    private void AddFilter(object? sender, EventArgs e)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = ExtensionFilter.Assemblies,
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
    } // private void AddFilter (object?, EventArgs)

    private void ToggleAutoApplyFilter(object? sender, EventArgs e)
        => this.menu_autoApplyFilter.Checked = Program.AutoApplyFilter = !this.menu_autoApplyFilter.Checked;

    private void ToggleHideOriginal(object? sender, EventArgs e)
    {
        this.menu_hideOriginal.Checked = Program.HideOriginalData = !this.menu_hideOriginal.Checked;
        ShowPlots();
    } // private void ToggleHideOriginal (object?, EventArgs)

    private void UnfilterSelectedRow(object? sender, EventArgs e)
    {
        if (this.row is null) return;
        Unfilter([this.row]);
    } // private void UnfilterSelectedRow (object?, EventArgs)

    private void UnfilterAll(object? sender, EventArgs e)
        => Unfilter(this.parametersTable.ParameterRows);

    private void Unfilter(IEnumerable<ParametersTableRow> rows)
    {
        foreach (var row in rows)
            row.Decay?.RestoreOriginal();
        ShowPlots();
    } // private void Unfilter (IEnumerable<ParametersTableRow>)

    #endregion filters

    #region Models

    private void UpdateModelList(object? sender, EventArgs e)
        => UpdateModelList();

    /// <summary>
    /// Updates the model list shown in the menu.
    /// </summary>
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
                    ToolTipText = model.Model.Description,
                    Checked = guid == this.selectedModel,
                };
                modelItem.Click += SelectModel;
                categoryItem.DropDownItems.Add(modelItem);

                if (model.Model is LinearCombinationModel)
                {
                    var removeModel = new ToolStripMenuItem("Remove")
                    {
                        Tag = guid,
                    };
                    removeModel.Click += RemoveLinearCombination;
                    modelItem.DropDownItems.Add(removeModel);
                }

                if (!ModelManager.EstimateProviders.TryGetValue(guid, out var providers)) continue;
                foreach (var provider in providers)
                {
                    var estimateItem = new ToolStripMenuItem(provider.Name)
                    {
                        ToolTipText = provider.Description,
                    };
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
            ToolTipText = "Add a new model",
        };
        add_model.Click += AddModel;
        this.menu_model.DropDownItems.Add(add_model);

        var add_linear_combination = new ToolStripMenuItem()
        {
            Text = "Add linear combination",
            ToolTipText = "Add the linear combination of models",
        };
        add_linear_combination.Click += AddLinearCombination;
        this.menu_model.DropDownItems.Add(add_linear_combination);
    } // private void UpdateModelList ()

    private void AddModel(object? sender, EventArgs e)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = ExtensionFilter.Assemblies,
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
        SelectModel(guid);
    } // private void SelectModel (object?, EventArgs)

    /// <summary>
    /// Changes the selected model.
    /// </summary>
    /// <param name="guid">The GUID of the model to select.</param>
    private void SelectModel(Guid guid)
    {
        this.selectedModel = Program.DefaultModel = guid;
        this.Text = GetTitle();
        var model = this.SelectedModel;
        this.parametersTable.SetColumns(model);
        if (model is null) return;

        this.rangeSelector.Time.Logarithmic = model.XLogScale;
        this.rangeSelector.Signal.Logarithmic = model.YLogScale;
        MakeTable();
        this.parametersTable.ClearUndoBuffer();
        foreach (var preview in this.previewWindows)
            preview.ModelId = guid;
    } // private void SelectModel (Guid)

    private void AddLinearCombination(object? sender, EventArgs e)
    {
        using var dialog = new LinearCombinationEditWindow();
        dialog.ShowDialog();
    } // private void AddLinearCombination (object?, EventArgs)

    private void RemoveLinearCombination(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not Guid guid) return;
        if (this.selectedModel == guid)
            SelectModel(Guid.Empty);
        Program.RemoveLinearCombination(guid);
    } // private void RemoveLinearCombination (object?, EventArgs)

    /// <summary>
    /// Makes the table of the parameters.
    /// </summary>
    #pragma warning disable IDE0079
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "The newly created row must NOT be disposed.")]
#pragma warning restore
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

    #endregion Models

    private void Undo(object? sender, EventArgs e)
        => this.parametersTable.Undo();

    private void Redo(object? sender, EventArgs e)
        => this.parametersTable.Redo();

    private void PasteTable(object? sender, EventArgs e)
        => PasteTable();

    /// <summary>
    /// Pastes the table from the clipboard.
    /// </summary>
    private void PasteTable()
    {
        if (this.decays is null) return;

        var model = this.SelectedModel;
        if (model is null) return;

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

    /// <summary>
    /// Selects the decay data with the specified wavelength.
    /// </summary>
    /// <param name="wavelength">The wavelength to select.</param>
    internal void SelectWavelength(double wavelength)
    {
        if (this.decays is null) return;

        var col = this.row?.Cells.OfType<DataGridViewCell>()
            .Select((Cell, Index) => (Cell, Index))
            .Where(x => x.Cell.Selected)
            .FirstOrDefault().Index ?? 0;

        var closest = this.decays.Keys.OrderBy(wl => Math.Abs(wl - wavelength)).First();
        var row = this.parametersTable[closest];
        if (row is null) return;
        foreach (var r in this.parametersTable.ParameterRows)
            r.Selected = false;
        row.Selected = true;
        row.Cells[col].Selected = true;

        var index = row.Index;
        this.parametersTable.FirstDisplayedScrollingRowIndex = Math.Max(0, index - 5);
    } // internal void SelectWavelength (double)

    private void RemoveDecay(object? sender, DataGridViewRowEventArgs e)
    {
        if (this.decays is null) return;
        if (e.Row is not ParametersTableRow row) return;
        var wavelength = row.Wavelength;
        this.decays.Remove(wavelength);
        ShowPlots();
        UpdatePreviewsParameters();
    } // private void RemoveDecay (object?, DataGridViewRowEventArgs)

    #region Data manipulation

    private void InvertMagnitude(object? sender, EventArgs e)
    {
        if (this.suppressAutoInvert) return;
        InvertMagnitude();
    } // private void InvertMagnitude (object?, EventArgs)

    /// <summary>
    /// Inverts the magnitude of the decay data.
    /// </summary>
    private void InvertMagnitude()
        => this.row?.InvertMagnitude();

    private void ChangeTime0(object? sender, EventArgs e)
        => ChangeTime0();

    /// <summary>
    /// Changes the time zero of the decay data.
    /// </summary>
    private void ChangeTime0()
    {
        if (this.decays is null) return;
        var t0 = (double)this.nud_time0.Value;
        this.decays.Time0 = t0;
        foreach (var row in this.parametersTable.ParameterRows)
            row.Decay = this.decays[row.Wavelength];
        ShowPlots();
    } // private void ChangeTime0 ()

    #endregion Data manipulation

    #region Plots

    /// <summary>
    /// Adds a dummy series to the chart.
    /// </summary>
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

    #region Show plots

    private void ShowPlots(object? sender, EventArgs e)
        => ShowPlots();

    /// <summary>
    /// Shows the observed and the fitting plots.
    /// </summary>
    private void ShowPlots()
    {
        ShowObserved();
        ShowFit();
    } // private void ShowPlots ()

    /// <summary>
    /// Shows the observed plot.
    /// </summary>
    private void ShowObserved()
    {
        if (this.stopDrawing) return;
        if (this.row is null) return;
        var decay = this.row.Decay;
        var filtered = decay.Filtered;

        if (this.cb_invert.Checked)
        {
            decay = decay.Inverted;
            filtered = filtered.Inverted;
        }

        this.s_observed.Points.Clear();
        if (!this.menu_hideOriginal.Checked)
            this.s_observed.Points.AddDecay(decay);

        this.s_filtered.Points.Clear();
        this.s_filtered.Points.AddDecay(filtered);
    }// private void ShowObserved ()

    private void ShowFit(object? sender, EventArgs e)
        => ShowFit();

    /// <summary>
    /// Shows the fitting plot.
    /// </summary>
    private void ShowFit()
    {
        if (this.stopDrawing) return;
        if (this.row is null) return;

        var model = this.SelectedModel;
        if (model is null) return;
        
        var decay = this.row.Decay;

        var parameters = this.row.Parameters;
        var func = model.GetFunction(parameters);
        var invert = this.cb_invert.Checked ? -1 : 1;
        var times = decay.Times;
        var signals = times.Select(t => func(t) * invert);

        this.s_fit.Points.Clear();
        this.s_fit.Points.AddDecay(times, signals);
    } // private void ShowFit ()

    #endregion Show plots

    #region Adjust axes

    private void AdjustXAxisInterval(object? sender, EventArgs e)
        => AdjustXAxisInterval();

    private void AdjustYAxisInterval(object? sender, EventArgs e)
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

    private void AdjustAxesIntervals(object? sender, EventArgs e)
        => AdjustAxesIntervals();

    private void AdjustAxesIntervals()
    {
        AdjustXAxisInterval();
        AdjustYAxisInterval();
    } // private void AdjustAxesIntervals ()

    #endregion Adjust axes

    #endregion Plots

    #region Levenberg-Marquardt estimation

    private void LevenbergMarquardtEstimationSelectedRow(object? sender, EventArgs e)
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        if (this.row is null) return;
        LevenbergMarquardtEstimation([this.row]);
    } // private void LevenbergMarquardtEstimationSelectedRow (object?, EventArgs)

    private void LevenbergMarquardtEstimationAllRows(object? sender, EventArgs e)
        => LevenbergMarquardtEstimationAllRows();

    /// <summary>
    /// Fits all rows using the Levenberg-Marquardt algorithm.
    /// </summary>
    private void LevenbergMarquardtEstimationAllRows()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        LevenbergMarquardtEstimation(this.parametersTable.ParameterRows);
    } // private void LevenbergMarquardtEstimationAllRows ()

    /// <summary>
    /// Fits the specified rows using the Levenberg-Marquardt algorithm.
    /// </summary>
    /// <param name="rows">The rows to fit.</param>
    private async void LevenbergMarquardtEstimation(IEnumerable<ParametersTableRow> rows)
    {
        var text = this.Text;
        this.Text += " - Fitting...";
        var source = rows.ToArray();

        var model = this.SelectedModel;
        if (model is null) return;

        Func<ParametersTableRow, IFittingModel, IReadOnlyList<double>> estimation = LevenbergMarquardtEstimation;
        var n = this.decays?.Values.Select(d => d.OnlyAfterT0.Times.Count).Max() ?? 0;
        if (model is IVectorizedModel<AvxVector1024> && LevenbergMarquardtSIMD<AvxVector1024>.CheckSupport(n))
            estimation = LevenbergMarquardtEstimationSIMD<AvxVector1024>;
        else if (model is IVectorizedModel<AvxVector2048> && LevenbergMarquardtSIMD<AvxVector2048>.CheckSupport(n))
            estimation = LevenbergMarquardtEstimationSIMD<AvxVector2048>;

        var start = Stopwatch.GetTimestamp();

        try
        {
            this.parametersTable.StopUpdateRSquared = true;
            this.stopDrawing = true;
            if (source.Length >= Program.ParallelThreshold)
            {
                var results = new ConcurrentDictionary<ParametersTableRow, IReadOnlyList<double>>();
                await Task.Run(() => Parallel.ForEach(source, (row) =>
                {
                    var parameters = estimation(row, model);
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
                    row.Parameters = estimation(row, model);
            }
        }
        finally
        {
            this.parametersTable.StopUpdateRSquared = false;
            this.stopDrawing = false;
        }

        var elapsed = Stopwatch.GetElapsedTime(start);

        ShowPlots();

        this.Text = text;
        FadingMessageBox.Show(
            $"Fitting completed in {elapsed.TotalSeconds:F1} seconds.",
            0.8, 1000, 75, 0.1
        );
    } // private void LevenbergMarquardtEstimation (IEnumerable<ParametersTableRow>)

    /// <summary>
    /// Fits the specified row using the Levenberg-Marquardt algorithm.
    /// </summary>
    /// <param name="row">The row to fit.</param>
    /// <param name="model">The model to fit.</param>
    /// <returns>The estimated parameters.</returns>
    private static IReadOnlyList<double> LevenbergMarquardtEstimation(ParametersTableRow row, IFittingModel model)
    {
        var decay = row.Decay.OnlyAfterT0;
        var lma = new LevenbergMarquardt(model, decay.Times, decay.Filtered.Signals, row.Parameters)
        {
            MaxIteration = Program.MaxIterations,
        };
        lma.Fit();
        return lma.Parameters;
    } // private static void LevenbergMarquardtEstimation (ParametersTableRow, IFittingModel)

    /// <summary>
    /// Fits the specified row using the SIMD-accelerated Levenberg-Marquardt algorithm.
    /// </summary>
    /// <param name="row">The row to fit.</param>
    /// <param name="model">The model to fit.</param>
    /// <returns>The estimated parameters.</returns>
    private static IReadOnlyList<double> LevenbergMarquardtEstimationSIMD<TVector>(ParametersTableRow row, IFittingModel model) where TVector : IIntrinsicVector<TVector>
    {
        var decay = row.Decay.OnlyAfterT0;
        var lma = new LevenbergMarquardtSIMD<TVector>((IVectorizedModel<TVector>)model, decay.Times, decay.Signals, row.Parameters)
        {
            MaxIteration = Program.MaxIterations,
        };
        lma.Fit();
        return lma.Parameters;
    } // private static IReadOnlyList<double> LevenbergMarquardtEstimationSIMD<TVector> (ParametersTableRow, IAnalyticallyDifferentiable) where TVector : IIntrinsicVector<TVector>

    private void ToggleAutoFit(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        Program.AutoFit = item.Checked = !item.Checked;
    } // private void ToggleAutoFit (object?, EventArgs)

    #endregion Levenberg-Marquardt estimation

    #region Estimate parameters

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

    #endregion Estimate parameters

    #region Average and outlier removal

    private void TakeAverage(object? sender, EventArgs e)
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        if (this.row is null) return;
        TakeAverage(this.row);
    } // private void TakeAverage (object?, EventArgs)

    private void TakeAverage(ParametersTableRow row)
    {
        if (this.decays is null) return;
        var wl = row.Wavelength;
        var prev = this.decays.Keys.Where(w => w < wl);
        var next = this.decays.Keys.Where(w => w > wl);

        if (!(prev.Any() || next.Any())) return;  // Only one row exists

        if (prev.Any() && next.Any())
        {
            var p = this.parametersTable[prev.Max()];
            var n = this.parametersTable[next.Min()];
            if (p is null || n is null) return;

            for (var i = 0; i < row.Parameters.Count; i++)
                row[i] = (p[i] + n[i]) / 2;
        }
        else if (prev.Any())
        {
            var p = this.parametersTable[prev.Max()];
            if (p is null) return;
            row.Parameters = p.Parameters;
        }
        else
        {
            var n = this.parametersTable[next.Min()];
            if (n is null) return;
            row.Parameters = n.Parameters;
        }

    } // private void TakeAverage (ParametersTableRow)

    #endregion Average and outlier removal

    #region Spectra preview

    private void ShowSpectraPreview(object? sender, EventArgs e)
        => ShowSpectraPreview();

    /// <summary>
    /// Shows the spectra preview window.
    /// </summary>
    private void ShowSpectraPreview()
    {
        var preview = new SpectraPreviewWindow(this.ParametersList)
        {
            ModelId = this.selectedModel,
            SelectedWavelength = this.SelectedWavelength,
        };
        if (this.decays is not null)
            preview.SignalUnit = this.decays.SignalUnit;
        SetTimeTable(preview);
        this.previewWindows.Add(preview);
        preview.FormClosed += (s, e) => this.previewWindows.Remove(preview);
        preview.Show();
    } // private void ShowSpectraPreview ()

    private void UpdatePreviewsParameters(object? sender, EventArgs e)
        => UpdatePreviewsParameters();

    /// <summary>
    /// Updates the parameters in the spectra preview windows.
    /// </summary>
    private void UpdatePreviewsParameters()
    {
        foreach (var preview in this.previewWindows)
        {
            preview.SetParameters(this.ParametersList);
            SetTimeTable(preview);
        }
    } // private void UpdatePreviewsParameters ()

    private void SetTimeTable(SpectraPreviewWindow preview)
        => preview.SetTimeTable((double)this.rangeSelector.Time.To);

    /// <summary>
    /// Updates the selected wavelength in the spectra preview windows.
    /// </summary>
    private void UpdatePreviewsSelectedWavelength()
    {
        var wavelength = this.SelectedWavelength;
        foreach (var preview in this.previewWindows)
            preview.SelectedWavelength = wavelength;
    } // private void UpdatePreviewsSelectedWavelength ()

    /// <summary>
    /// Updates the signal unit in the spectra preview windows.
    /// </summary>
    private void UpdatePreviewsUnits()
    {
        if (this.decays is null) return;
        var unit = this.decays.SignalUnit;
        foreach (var preview in this.previewWindows)
            preview.SignalUnit = unit;
    } // private void UpdatePreviewsUnits ()

    #endregion Spectra preview

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

    private void SetFilteredColor(object? sender, EventArgs e)
    {
        using var cd = new ColorDialog()
        {
            Color = Program.FilteredColor,
        };
        if (cd.ShowDialog() != DialogResult.OK) return;
        this.s_filtered.Color = Program.FilteredColor = cd.Color;
    } // private void SetFilteredColor (object?, EventArgs)

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
} // internal sealed partial class MainWindow : Form

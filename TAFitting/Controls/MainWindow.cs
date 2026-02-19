
// (c) 2024-2025 Kazuki KOHZUKI

using DisposalGenerator;
using Microsoft.Win32;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;
using TAFitting.Clipboard;
using TAFitting.Controls.Analyzers;
using TAFitting.Controls.Charting;
using TAFitting.Controls.LinearCombination;
using TAFitting.Controls.Toast;
using TAFitting.Data;
using TAFitting.Excel;
using TAFitting.Filter;
using TAFitting.Model;
using TAFitting.Sync;
using TAFitting.Update;
using Timer = System.Windows.Forms.Timer;

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
    private readonly PlotHelper plotHelper;
    private readonly DisplayRangeSelector rangeSelector;
    private readonly CheckBox cb_invert;
    private bool suppressAutoInvert = false;

    private readonly Timer timeRangeChangedEventTimer;
    private readonly ParametersTable parametersTable;

    private readonly ToolStripMenuItem menu_filter, menu_model;
    private readonly ToolStripMenuItem menu_autoApplyFilter, menu_hideOriginal, menu_unfilter;
    private Guid selectedModel = Guid.Empty;
    private readonly ToolStripMenuItemGroup<Guid> modelItemGroup;

    private Decays? decays;

    private ParametersTableRow? row;
    private string sampleName = string.Empty;
    private readonly CustomNumericUpDown nud_time0;
    private readonly Label lb_t0, lb_timeUnit;

    private readonly List<IDecayAnalyzer> analyzers = [];

    private readonly LevenbergMarquardtEstimationHelper lmHelper;
    private readonly SpectraPreviewHelper spectraPreviewHelper;

    /// <summary>
    /// Gets the sample name.
    /// </summary>
    internal string SampleName
        => this.sampleName;

    /// <summary>
    /// Gets the selected wavelength.
    /// </summary>
    private double SelectedWavelength => this.row?.Wavelength ?? double.NaN;

    /// <summary>
    /// Gets the selected model.
    /// </summary>
    /// <value>The selected model if is valid; otherwise, <see langword="null"/>.</value>
    private IFittingModel? SelectedModel => ModelManager.Models.TryGetValue(this.selectedModel, out var model) ? model.Model : null;

    /// <summary>
    /// Gets the list of preview windows for the spectra.
    /// </summary>
    internal IEnumerable<int> SpectraIds => this.spectraPreviewHelper.SpectraIds;

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

        this.plotHelper = new(this.chart);

        this.plotHelper.AddDummySeries();

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
        this.parametersTable.FileDropped += ReadSpreadSheet;

        #endregion params

        #region view options

        this.rangeSelector = new(this.plotHelper.AxisX, this.plotHelper.AxisY)
        {
            Top = 10,
            Left = 10,
            Parent = this.paramsContainer.Panel2,
        };
        this.rangeSelector.Time.FromChanged += ChangeTimeRangeWithDelay;
        this.rangeSelector.Time.ToChanged += ChangeTimeRangeWithDelay;

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
            SelectModel(defaultModel).Wait();

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

        #region menu.view.plot

        var menu_view = new ToolStripMenuItem("&View");
        this.MainMenuStrip.Items.Add(menu_view);

        var menu_viewPlot = new ToolStripMenuItem("&Plot");
        menu_view.DropDownItems.Add(menu_viewPlot);

        var menu_viewObserved = new ToolStripMenuItem("&Observed")
        {
            ToolTipText = "Observed data",
        };
        menu_viewPlot.DropDownItems.Add(menu_viewObserved);

        var menu_viewObservedColor = new ToolStripMenuItem("&Color")
        {
            ToolTipText = "Change the color of the observed data",
        };
        menu_viewObservedColor.Click += this.plotHelper.SetObservedColor;
        menu_viewObserved.DropDownItems.Add(menu_viewObservedColor);

        var menu_viewObservedSize = new ToolStripMenuItem("&Size")
        {
            ToolTipText = "Change the size of the observed data",
        };
        menu_viewObserved.DropDownItems.Add(menu_viewObservedSize);

        var overservedSizeGroup = new ToolStripMenuItemGroup<int>(this.plotHelper.ChangeObservedSize);
        for (var i = 1; i <= 10; i++)
        {
            var item = new GenericToolStripMenuItem<int>(i.ToInvariantString(), i, overservedSizeGroup)
            {
                Checked = i == Program.ObservedSize,
            };
            menu_viewObservedSize.DropDownItems.Add(item);
        }

        var menu_viewFiltered = new ToolStripMenuItem("&Filtered")
        {
            ToolTipText = "Filtered data",
        };
        menu_viewPlot.DropDownItems.Add(menu_viewFiltered);

        var menu_viewFilteredColor = new ToolStripMenuItem("&Color")
        {
            ToolTipText = "Change the color of the filtered data",
        };
        menu_viewFilteredColor.Click += this.plotHelper.SetFilteredColor;
        menu_viewFiltered.DropDownItems.Add(menu_viewFilteredColor);

        var menu_viewFilteredWidth = new ToolStripMenuItem("&Width")
        {
            ToolTipText = "Change the width of the filtered data",
        };
        menu_viewFiltered.DropDownItems.Add(menu_viewFilteredWidth);

        var filteredWidthGroup = new ToolStripMenuItemGroup<int>(this.plotHelper.ChangeFilteredWidth);
        for (var i = 1; i <= 10; i++)
        {
            var item = new GenericToolStripMenuItem<int>(i.ToInvariantString(), i, filteredWidthGroup)
            {
                Checked = i == Program.FilteredWidth,
            };
            menu_viewFilteredWidth.DropDownItems.Add(item);
        }

        var menu_viewFit = new ToolStripMenuItem("&Fit")
        {
            ToolTipText = "Fitting curve",
        };
        menu_viewPlot.DropDownItems.Add(menu_viewFit);

        var menu_viewFitColor = new ToolStripMenuItem("&Color")
        {
            ToolTipText = "Change the color of the fitting curve",
        };
        menu_viewFitColor.Click += this.plotHelper.SetFitColor;
        menu_viewFit.DropDownItems.Add(menu_viewFitColor);

        var menu_viewFitWidth = new ToolStripMenuItem("&Width")
        {
            ToolTipText = "Change the width of the fitting curve",
        };
        menu_viewFit.DropDownItems.Add(menu_viewFitWidth);

        var fitWidthGroup = new ToolStripMenuItemGroup<int>(this.plotHelper.ChangeFitWidth);
        for (var i = 1; i <= 10; i++)
        {
            var item = new GenericToolStripMenuItem<int>(i.ToInvariantString(), i, fitWidthGroup)
            {
                Checked = i == Program.FitWidth,
            };
            menu_viewFitWidth.DropDownItems.Add(item);
        }

        #endregion menu.view.plot

        #region menu.view.font

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
        menu_viewFontAxisLabel.Click += this.plotHelper.SelectAxisLabelFont;

        var menu_viewFontAxisTitle = new ToolStripMenuItem("&Axis Title")
        {
            ToolTipText = "Change the font of the axis titles",
        };
        menu_viewFont.DropDownItems.Add(menu_viewFontAxisTitle);
        menu_viewFontAxisTitle.Click += this.plotHelper.SelectAxisTitleFont;

        #endregion menu.view.font

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

        var menu_dataReplace = new ToolStripMenuItem("&Replace decay")
        {
            ShortcutKeys = Keys.Control | Keys.H,
            ToolTipText = "Load and replace the decay data for the selected wavelength",
        };
        menu_data.DropDownOpening += (sender, e)
            => menu_dataReplace.Enabled = CheckDecayCanBeReplaced();
        menu_dataReplace.Click += ReplaceDecay;
        menu_data.DropDownItems.Add(menu_dataReplace);

        #endregion menu.data

        #region menu.analyze

        var menu_analyze = new ToolStripMenuItem("&Analyze");
        this.MainMenuStrip.Items.Add(menu_analyze);

        var menu_analyzeFourier = new ToolStripMenuItem("&Fourier transform")
        {
            ToolTipText = "Perform the Fourier transform",
        };
        menu_analyzeFourier.Click += ShowFourierAnalyzer;
        menu_analyze.DropDownItems.Add(menu_analyzeFourier);

        #endregion menu.analyze

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
        this.modelItemGroup = new(SelectModel);
        UpdateModelList();

        #endregion menu.model

        #region menu.fit

        var menu_fit = new ToolStripMenuItem("&Fit");
        this.MainMenuStrip.Items.Add(menu_fit);

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
        
        var menu_fitSIMD = new ToolStripMenuItem("Use &SIMD")
        {
            Checked = Program.UseSIMD,
            ToolTipText = "Fit the data using SIMD instructions",
        };
        menu_fitSIMD.Click += ToggleUseSIMD;
        menu_fit.DropDownItems.Add(menu_fitSIMD);

        menu_fit.DropDownItems.Add(new ToolStripSeparator());

        var menu_fitAverage = new ToolStripMenuItem("Take average")
        {
            ShortcutKeys = Keys.Control | Keys.M,
            ToolTipText = "Fit the data using the average of the observed data",
        };
        menu_fitAverage.Click += TakeAverage;
        menu_fit.DropDownItems.Add(menu_fitAverage);

        #endregion menu.fit

        #region menu.help

        var menu_help = new ToolStripMenuItem("&Help");
        this.MainMenuStrip.Items.Add(menu_help);

        var menu_helpCheckUpdates = new ToolStripMenuItem("Check for &updates", null, ForceCheckUpdate)
        {
            ToolTipText = "Check for updates",
        };
        menu_help.DropDownItems.Add(menu_helpCheckUpdates);

        var menu_helpGitHub = new ToolStripMenuItem("Open &GitHub")
        {
            ToolTipText = "Open the GitHub repository",
        };
        menu_helpGitHub.Click += (sender, e) => Program.OpenGitHub();
        menu_help.DropDownItems.Add(menu_helpGitHub);

        #endregion menu.help

        #endregion menu

        this.lmHelper = new(this.parametersTable);
        this.spectraPreviewHelper = new();

        UpdateManager.NewerVersionFound += OnNewerReleaseFound;

        this.mainContainer.SplitterDistance = 750;
        this.mainContainer.Panel2MinSize = 420;

        this.paramsContainer.SplitterDistance = 600;
        this.paramsContainer.Panel2MinSize = 120;

        this.timeRangeChangedEventTimer = new()
        {
            Interval = 50
        };
        this.timeRangeChangedEventTimer.Tick += SetTimeRangeToTable;
    } // ctor ()

    override protected void OnShown(EventArgs e)
    {
        base.OnShown(e);

        this.rangeSelector.Time.FromChanged += this.plotHelper.AdjustXAxisInterval;
        this.rangeSelector.Time.ToChanged += this.plotHelper.AdjustXAxisInterval;
        this.rangeSelector.Time.LogarithmicChanged += this.plotHelper.AdjustXAxisInterval;
        this.rangeSelector.Signal.FromChanged += this.plotHelper.AdjustYAxisInterval;
        this.rangeSelector.Signal.ToChanged += this.plotHelper.AdjustYAxisInterval;
        this.rangeSelector.Signal.LogarithmicChanged += this.plotHelper.AdjustYAxisInterval;

        this.chart.SizeChanged += this.plotHelper.AdjustAxesIntervals;

        this.plotHelper.AdjustAxesIntervals();
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

        var page = new TaskDialogPage()
        {
            Heading = "Do you want to open new data?",
            Text = "The current data will be lost.",
            Caption = Program.AppName,
            Icon = TaskDialogIcon.Warning,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            DefaultButton = TaskDialogButton.No,
        };
        var result = TaskDialog.ShowDialog(page);
        return result == TaskDialogButton.Yes;
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
        var dialog = new OpenFolderDialog()
        {
            Title = "Select a µs-TAS data folder",
            ClientGuid = Program.Config.SeparateFileDialogState ? usTasDialog : Program.FileDialogCommonId,
        };
        if (!(dialog.ShowDialog() ?? false)) return;

        LoadMicrosecondDecays(dialog.FolderName);
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
        using var dialog = new System.Windows.Forms.OpenFileDialog()
        {
            Filter = ExtensionFilter.FsTasFiles,
            Title = "Select a fs-TAS data file",
            ClientGuid = Program.Config.SeparateFileDialogState ? fsTasDialog : Program.FileDialogCommonId,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        LoadFemtosecondDecays(dialog.FileName);
    } // private void LoadFemtosecondDecays ()

    /// <summary>
    /// Loads the femtosecond decay data from the specified path.
    /// </summary>
    /// <param name="path">The path to the file from which the decay data is loaded.</param>
    internal void LoadFemtosecondDecays(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is not ".csv" and not ".ufs")
        {
            MessageBox.Show(
                "The selected file is not a valid fs-TAS data file.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        if (ext == ".csv")
            LoadDecays(path, Decays.FemtosecondFromCsvFile);
        else
            LoadDecays(path, Decays.FemtosecondFromUfsFile);
    } // internal void LoadFemtosecondDecays (string)

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
        Task.Run(async () =>
        {
            // Temporary change the negative sign to U+002D
            // because double.Parse throws an exception with U+2212.
            using var _ = new NegativeSignHandler();

            try
            {
                this.decays = decaysLoader(path);
                await Invoke(async () =>
                {
                    this.Text = GetTitle();

                    this.plotHelper.AxisX.Title = $"Time ({this.decays.TimeUnit})";
                    this.plotHelper.AxisY.Title = this.decays.SignalUnit;
                    this.rangeSelector.Time.Text = $"Time ({this.decays.TimeUnit}):";
                    this.rangeSelector.Signal.Text = $"{this.decays.SignalUnit}:";
                    this.lb_timeUnit.Text = this.decays.TimeUnit;

                    this.rangeSelector.Time.To = (decimal)this.decays.MaxTime;
                    var maxAbsSignal = this.decays.MaxAbsSignal;
                    this.rangeSelector.Signal.To = Math.Min((decimal)Math.Min(maxAbsSignal * 1.5, +7.9e28), this.rangeSelector.Signal.ToMaximum);
                    this.rangeSelector.Signal.From = Math.Max((decimal)(maxAbsSignal * 0.01), this.rangeSelector.Signal.FromMinimum);

                    // The table rows must be cleared before the t0 is set.
                    this.parametersTable.Rows.Clear();
                    // The t0 must be set before the filter is applied.
                    this.nud_time0.Value = (decimal)this.decays.Time0;

                    if (Program.AutoApplyFilter)
                    {
                        var filter = FilterManager.DefaultFilter;
                        if (filter is not null)
                        {
                            foreach (var decay in this.decays.GetDecaysEnumerable())
                                decay.Filter(filter);
                        }
                    }

                    await MakeTable();
                    this.parametersTable.ClearUndoBuffer();
                    this.spectraPreviewHelper.UpdateTimeUnit(this.decays.TimeUnit);
                    this.spectraPreviewHelper.UpdateSignalUnits(this.decays.SignalUnit);
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

    /// <summary>
    /// Checks whether the decay for the selected row can be replaced.
    /// </summary>
    /// <returns><see langword="true"/> if the decay can be replaced; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckDecayCanBeReplaced()
        => (this.row?.Decay.Mode ?? TasMode.None) == TasMode.Microsecond;

    async private void ReplaceDecay(object? sender, EventArgs e)
    {
        if (!CheckDecayCanBeReplaced()) return;

        var dialog = new OpenFolderDialog()
        {
            Title = "Select a µs-TAS data folder",
            ClientGuid = Program.Config.SeparateFileDialogState ? usTasDialog : Program.FileDialogCommonId,
        };
        if (!(dialog.ShowDialog() ?? false)) return;

        await ReplaceDecay(dialog.FolderName);
    } // async private void ReplaceDecay (object?, EventArgs)

    async private Task ReplaceDecay(string path)
    {
        if (this.decays is null) return;
        if (this.row is null) return;

        var basename = Path.GetFileName(path);
        var filename = FileNameHandler.GetFileName(basename, Program.AMinusBSignalFormat);
        var filePath = Path.Combine(path, filename);
        if (!File.Exists(filePath))
        {
            MessageBox.Show(
                "The selected folder does not contain the decay data file.",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        if (Program.WarnBeforeMismatchReplacement && (!Decays.TryGetWavelength(basename, out var wavelength) || wavelength != this.row.Wavelength))
        {
            var page = new TaskDialogPage()
            {
                Heading = "The wavelength in the selected folder does not match the selected row.",
                Text = "Do you want to continue?",
                Caption = Program.AppName,
                Icon = TaskDialogIcon.Warning,
                AllowCancel = true,
                Verification = new TaskDialogVerificationCheckBox()
                {
                    Text = "Do not show this message again.",
                    Checked = false,
                },
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                DefaultButton = TaskDialogButton.No,
            };
            var result = TaskDialog.ShowDialog(page);
            if (result != TaskDialogButton.Yes) return;
            Program.WarnBeforeMismatchReplacement = !page.Verification.Checked;
        }

        var oldDecay = this.row.Decay;
        var decay = Decay.FromFile(filePath, oldDecay.TimeUnit, oldDecay.SignalUnit);

        // t0 determination is skipped and the old t0 is applied
        var t0 = this.decays.Time0;
        decay.AddTime(-t0);

        if (Program.AutoApplyFilter)
        {
            var filter = FilterManager.DefaultFilter;
            if (filter is not null)
                decay.Filter(filter);
        }

        this.decays[this.row.Wavelength] = decay;
        this.row.Decay = decay;

        if (Program.AutoFit)
            await LevenbergMarquardtEstimation([this.row]);

        ShowPlots();
    } // async private Task ReplaceDecay (string)

    #endregion Data loading

    #region filters

    private void UpdateFilterList(object? sender, EventArgs e)
        => UpdateFilterList();

    private void UpdateFilterList()
    {
        this.menu_filter.DropDownItems.Clear();
        var filters = FilterManager.Filters.GroupBy(f => f.Value.Category);
        foreach (var category in filters)
        {
            var categoryName = category.Key;
            if (string.IsNullOrEmpty(categoryName))
                categoryName = "Other";
            var categoryItem = new ToolStripMenuItem(categoryName);
            this.menu_filter.DropDownItems.Add(categoryItem);

            foreach ((var guid, var filterItem) in category)
            {
                var filter = filterItem.Instance;

                var item = new ToolStripMenuItem(filter.Name)
                {
                    ToolTipText = filter.Description,
                };
                categoryItem.DropDownItems.Add(item);

                var applySelectedRow = new ToolStripMenuItem("Apply to selected row")
                {
                    Tag = filter,
                    ToolTipText = "Apply this filter to the selected row",
                };
                applySelectedRow.Click += ApplyFilterSelectedRow;
                item.DropDownItems.Add(applySelectedRow);

                var applyAllRows = new ToolStripMenuItem("Apply to all rows")
                {
                    Tag = filter,
                    ToolTipText = "Apply this filter to all rows",
                };
                applyAllRows.Click += ApplyFilterAllRows;
                item.DropDownItems.Add(applyAllRows);

                item.DropDownItems.Add(new ToolStripSeparator());

                var setDefault = new ToolStripMenuItem("Set as default")
                {
                    Tag = guid,
                    ToolTipText = "Set this filter as the default filter",
                };
                setDefault.Click += SetDefaultFilter;
                item.DropDownItems.Add(setDefault);

                if (guid == Program.DefaultFilter)
                {
                    item.Text += " (default)";
                    applySelectedRow.ShortcutKeys = Keys.Control | Keys.F;
                    applyAllRows.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F;
                    setDefault.Enabled = false;
                }
            }
        }

        this.menu_filter.DropDownItems.Add(new ToolStripSeparator());

        var menu_interpolate = new ToolStripMenuItem("&Interpolate")
        {
            ToolTipText = "Interpolate the data",
        };
        this.menu_filter.DropDownItems.Add(menu_interpolate);

        foreach (var mode in Enum.GetValues<InterpolationMode>())
        {
            var menu_interpolateMode = new ToolStripMenuItem(mode.ToDefaultSerializeValue())
            {
                ToolTipText = mode.ToDescription(),
            };
            menu_interpolate.DropDownItems.Add(menu_interpolateMode);

            var menu_interpolateSelectedRow = new GenericToolStripMenuItem<InterpolationMode>("&Selected row", mode)
            {
                ToolTipText = "Interpolate the selected row",
            };
            menu_interpolateSelectedRow.Click += InterpolateSelectedRow;
            menu_interpolateMode.DropDownItems.Add(menu_interpolateSelectedRow);

            var menu_interpolateAll = new GenericToolStripMenuItem<InterpolationMode>("&All rows", mode)
            {
                ToolTipText = "Interpolate all rows",
            };
            menu_interpolateAll.Click += InterpolateAll;
            menu_interpolateMode.DropDownItems.Add(menu_interpolateAll);
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

    private void SetDefaultFilter(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not Guid guid) return;
        Program.DefaultFilter = guid;
        UpdateFilterList();
    } // private void SetDefaultFilter (object?, EventArgs)

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

    private void ApplyFilter(IFilter filter, ParametersTableRowsEnumerable rows)
    {
        foreach (var row in rows)
        {
            var decay = row.Decay;
            decay?.Filter(filter);
        } // foreach (var row in rows)

        ShowPlots();
        UpdateAnalyzers();
    } // private void ApplyFilter (IFilter, ParametersTableRowsEnumerable)

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

    private void Unfilter(ParametersTableRowsEnumerable rows)
    {
        foreach (var row in rows)
            row.Decay?.RestoreOriginal();
        ShowPlots();
        UpdateAnalyzers();
    } // private void Unfilter (ParametersTableRowsEnumerable)

    private void InterpolateSelectedRow(object? sender, EventArgs e)
    {
        if (this.row is null) return;
        if (sender is not GenericToolStripMenuItem<InterpolationMode> item) return;
        Interpolate([this.row], item.Tag);
    } // private void InterpolateSelectedRow (object?, EventArgs)

    private void InterpolateAll(object? sender, EventArgs e)
    {
        if (sender is not GenericToolStripMenuItem<InterpolationMode> item) return;
        Interpolate(this.parametersTable.ParameterRows, item.Tag);
    } // private void InterpolateAll (object?, EventArgs)

    private void Interpolate(ParametersTableRowsEnumerable rows, InterpolationMode mode)
    {
        var filter = Program.AutoApplyFilter ? FilterManager.DefaultFilter : null;

        foreach (var row in rows)
        {
            var decay = row.Decay;
            if (decay is null) continue;
            decay.Interpolate(mode);
            if (filter is not null)
                decay.Filter(filter);
        }
        this.plotHelper.ShowObserved(this.row, this.menu_hideOriginal.Checked);
        UpdateAnalyzers();
    } // private void Interpolate (ParametersTableRowsEnumerable, InterpolationMode)

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
        this.modelItemGroup.RemoveAll();
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
                var modelItem = new GenericToolStripMenuItem<Guid>(model.Model.Name, guid, this.modelItemGroup)
                {
                    ToolTipText = model.Model.Description,
                    Checked = guid == this.selectedModel,
                };
                categoryItem.DropDownItems.Add(modelItem);

                if (model.Model is LinearCombinationModel)
                {
                    var removeModel = new ToolStripMenuItem("Remove", null, RemoveLinearCombination)
                    {
                        Tag = guid,
                    };
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

                    var estimateAll = new ToolStripMenuItem("All rows", null, EstimateParametersAllRows)
                    {
                        Tag = provider,
                    };
                    estimateItem.DropDownItems.Add(estimateAll);

                    var estimateNotEdited = new ToolStripMenuItem("Not edited rows only", null, EstimateParametersNotEditedRows)
                    {
                        Tag = provider,
                    };
                    estimateItem.DropDownItems.Add(estimateNotEdited);
                }
            } // foreach ((var guid, var model) in category)
        } // foreach (var category in models)

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

    private void SelectModel(object? sender, ToolStripMenuItemGroupSelectionChangedEventArgs<Guid> e)
    {
        if (e.SelectedItem is null) return;
        var guid = e.SelectedItem.Tag;

        if (guid == this.selectedModel) return;

        if (this.parametersTable.Edited && Program.WarnBeforeChangeModel)
        {
            var page = new TaskDialogPage()
            {
                Heading = "Do you want to change the model?",
                Text = "Changing the model will clear the current fitting parameters.",
                Caption = Program.AppName,
                Icon = TaskDialogIcon.Warning,
                AllowCancel = true,
                Verification = new TaskDialogVerificationCheckBox()
                {
                    Text = "Do not show this message again",
                },
                Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            };
            var resultButton = TaskDialog.ShowDialog(page);
            if (resultButton != TaskDialogButton.Yes) return;
            Program.WarnBeforeChangeModel = !page.Verification.Checked;
        }

        _ = SelectModel(guid);
    } // private void SelectModel (object?, EventArgs)

    /// <summary>
    /// Changes the selected model.
    /// </summary>
    /// <param name="guid">The GUID of the model to select.</param>
    async private Task SelectModel(Guid guid)
    {
        this.selectedModel = Program.DefaultModel = guid;
        this.Text = GetTitle();
        var model = this.SelectedModel;
        this.parametersTable.SetColumns(model);
        if (model is null) return;

        this.rangeSelector.Time.Logarithmic = model.XLogScale;
        this.rangeSelector.Signal.Logarithmic = model.YLogScale;
        await MakeTable();
        this.parametersTable.ClearUndoBuffer();

        // MakeTable method calls UpdatePreviewsParameters method, which also updates the model ID.
        // Updating model ID here is redundant.
        //foreach (var preview in this.previewWindows)
        //    preview.ModelId = guid;
    } // async private Task SelectModel (Guid)

    private void AddLinearCombination(object? sender, EventArgs e)
    {
        using var dialog = new LinearCombinationEditWindow();
        dialog.ShowDialog();
    } // private void AddLinearCombination (object?, EventArgs)

    async private void RemoveLinearCombination(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        if (item.Tag is not Guid guid) return;
        if (this.selectedModel == guid)
            await SelectModel(Guid.Empty);
        Program.RemoveLinearCombination(guid);
    } // async private void RemoveLinearCombination (object?, EventArgs)

    /// <summary>
    /// Makes the table of the parameters.
    /// </summary>
    #pragma warning disable IDE0079
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "IDisposableAnalyzers.Correctness",
        "IDISP001:Dispose created",
        Justification = "The newly created row must NOT be disposed.")]
#pragma warning restore
    async private Task MakeTable()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;

        try
        {
            // Adding row raises the SelectionChanged event,
            // whch is computationally expensive.
            this.parametersTable.StopSelectionChanged = true;
            this.parametersTable.StopUpdateRSquared = true;

            this.parametersTable.Rows.Clear();
            foreach (var (wl, decay) in this.decays)
            {
                var row = this.parametersTable.Add(wl, decay);
                row.Inverted = decay.SignalsAfterT0.Average() < 0;
            }

            if (Program.AutoFit)
                await LevenbergMarquardtEstimationAllRows();
        }
        finally
        {
            this.parametersTable.StopSelectionChanged = false;
            this.parametersTable.StopUpdateRSquared = false;
        }

        // Clearing the cache is required to update the masked data after changing the data set or the model.
        this.spectraPreviewHelper.ClearSpectraMaskingCache();
        UpdatePreviewsParameters();

        this.plotHelper.ClearObserved();
        this.plotHelper.ClearFit();

        ChangeRow(this.parametersTable.ParameterRows.FirstOrDefault(), true);
    } // async private Task MakeTable ()

    #endregion Models

    #region table manipulation

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

        var parameters = model.Parameters.Names;
        try
        {
            var rows = ClipboardHandler.GetParameterValuesFromClipboard(parameters);
            foreach (var r in rows)
            {
                var wavelength = r.Wavelength;
                var row = this.parametersTable[wavelength];
                if (row is null) continue;
                row.Parameters = r.Parameters;
            }
        }
        catch
        { }
    } // private void PasteTable ()

    private void ReadSpreadSheet(object? sender, FileDroppedEventArgs e)
        => ReadSpreadSheet(e.FilePath);

    private void ReadSpreadSheet(string path)
    {
        if (this.decays is null) return;

        var model = this.SelectedModel;
        if (model is null) return;

        var ext = Path.GetExtension(path);

        try
        {
            using var _ = new NegativeSignHandler();
            using var reader = GetSpreadSheetReader(ext);

            reader.Open(path);

            if (!reader.IsOpened)
            {
                FadingMessageBox.Show(
                    "The selected spreadsheet could not be opened.",
                    0.8, 1000, 75, 0.1
                );
                return;
            }

            if (!reader.ModelMatched)
            {
                FadingMessageBox.Show(
                    "The selected spreadsheet does not match the selected model.",
                    0.8, 1000, 75, 0.1
                );
                return;
            }

            var buffer = (stackalloc double[model.Parameters.Count]);
            while (reader.ReadNextRow(out var wavelength, buffer))
            {
                var tableRow = this.parametersTable[wavelength];
                if (tableRow is null) continue;
                tableRow.SetParameters(buffer);
            }

            UpdatePreviewsParameters();
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
    } // private void ReadSpreadSheet (string)

    private ISpreadSheetReader GetSpreadSheetReader(string extension)
        => extension.ToUpperInvariant() switch
        {
            ".CSV" => new CsvReader(this.SelectedModel!),
            ".XLSX" => new ExcelReader(this.SelectedModel!),
            _ => throw new NotSupportedException("The selected file format is not supported."),
        };

    private void ChangeRow(object? sender, ParametersTableSelectionChangedEventArgs e)
    {
        var movedToNewRow = (this.row?.Index ?? -1) != e.Row.Index;
        ChangeRow(e.Row, movedToNewRow);
    } // private void ChangeRow (object?, ParametersTableSelectionChangedEventArgs)

    private void ChangeRow(ParametersTableRow? row, bool movedToNewRow)
    {
        if (row is null) return;

        var compare = ModifierKeys.HasFlag(Keys.Control) || ModifierKeys.HasFlag(Keys.Shift);

        if (!compare)
            this.row = row;

        this.plotHelper.ClearCompare();

        if (movedToNewRow)
        {
            if (compare)
            {
                // Suppress the unnecessary event during restoring the selected cell.
                this.parametersTable.StopSelectionChanged = true;
                try
                {
                    this.parametersTable.RestoreSelectedCell(this.row?.Index ?? 0);
                }
                finally
                {
                    this.parametersTable.StopSelectionChanged = false;
                }
                this.plotHelper.ShowCompare(row);
                return;
            }
            else
            {
                this.suppressAutoInvert = true;
                this.cb_invert.Checked = row.Inverted;
                this.suppressAutoInvert = false;
            }
        }

        this.Text = GetTitle();

        ShowPlots();
        this.spectraPreviewHelper.UpdateSelectedWavelength(this.SelectedWavelength);
        UpdateAnalyzers();
    } // private void ChangeRow (ParametersTableRow?, bool)

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

        var closest = this.decays.GetClosestWavelength(wavelength);
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

    /// <summary>
    /// Handles the time range change with a delay.
    /// </summary>
    /// <remarks>
    /// Add callbck to the <see cref="Timer.Tick"/> event of the <see cref="timeRangeChangedEventTimer"/> timer
    /// to handle the time range change with a delay.
    /// </remarks>
    private void ChangeTimeRangeWithDelay(object? sender, EventArgs e)
    {
        this.timeRangeChangedEventTimer.Stop();
        this.timeRangeChangedEventTimer.Start();
    } // private void ChangeTimeRangeWithDelay (object?, EventArgs)

    private void SetTimeRangeToTable(object? sender, EventArgs e)
    {
        this.timeRangeChangedEventTimer.Stop();
        var range = this.rangeSelector.Time;
        this.parametersTable.StopUpdateRSquared = true;
        try
        {
            this.parametersTable.TimeMin = (double)range.From;
            this.parametersTable.TimeMax = (double)range.To;
        }
        finally
        {
            this.parametersTable.StopUpdateRSquared = false;
        }
    } // private void SetTimeRangeToTable (object?, EventArgs)

    #endregion table manipulation

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
        ShowPlots();
    } // private void ChangeTime0 ()

    #endregion Data manipulation

    #region Show plots

    private void ShowPlots(object? sender, EventArgs e)
        => ShowPlots();

    /// <summary>
    /// Shows the observed and the fitting plots.
    /// </summary>
    private void ShowPlots()
        => this.plotHelper.ShowPlots(this.row, this.SelectedModel, this.menu_hideOriginal.Checked);

    private void ShowFit(object? sender, EventArgs e)
        => this.plotHelper.ShowFit(this.row, this.SelectedModel);

    #endregion Show plots

    #region Levenberg-Marquardt estimation

    async private void LevenbergMarquardtEstimationSelectedRow(object? sender, EventArgs e)
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        if (this.row is null) return;
        await LevenbergMarquardtEstimation([this.row]);
    } // async private void LevenbergMarquardtEstimationSelectedRow (object?, EventArgs)

    async private void LevenbergMarquardtEstimationAllRows(object? sender, EventArgs e)
        => await LevenbergMarquardtEstimationAllRows();

    /// <summary>
    /// Fits all rows using the Levenberg-Marquardt algorithm.
    /// </summary>
    async private Task LevenbergMarquardtEstimationAllRows()
    {
        if (this.selectedModel == Guid.Empty) return;
        if (this.decays is null) return;
        await LevenbergMarquardtEstimation(this.parametersTable.ParameterRows);
    } // async private Task LevenbergMarquardtEstimationAllRows ()

    /// <summary>
    /// Fits the specified rows using the Levenberg-Marquardt algorithm.
    /// </summary>
    /// <param name="rows">The rows to fit.</param>
    private async Task LevenbergMarquardtEstimation(ParametersTableRowsEnumerable rows)
    {
        var model = this.SelectedModel;
        if (model is null) return;

        var text = this.Text;
        this.Text += " - Fitting...";

        this.plotHelper.StopDrawing = true;
        var start = Stopwatch.GetTimestamp();

        var success = await this.lmHelper.Estimate(rows, model);
        if (!success) return;

        var elapsed = Stopwatch.GetElapsedTime(start);
        this.plotHelper.StopDrawing = false;

        this.Text = text;
        FadingMessageBox.Show(
            $"Fitting completed in {elapsed.TotalSeconds:F1} seconds.",
            0.8, 1000, 75, 0.1
        );

        var selected = this.parametersTable.SelectedRow;
        if (selected is not null)
            this.row = selected;

        ShowPlots();
        UpdatePreviewsParameters();
    } // private async Task LevenbergMarquardtEstimation (ParametersTableRowsEnumerable)

    private void ToggleAutoFit(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        Program.AutoFit = item.Checked = !item.Checked;
    } // private void ToggleAutoFit (object?, EventArgs)

    private void ToggleUseSIMD(object? sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem item) return;
        Program.UseSIMD = item.Checked = !item.Checked;
    } // private void ToggleUseSIMD (object?, EventArgs)

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

    private void EstimateParameters(IEstimateProvider estimateProvider, ParametersTableRowsEnumerable rows)
    {
        foreach (var row in rows)
        {
            var decay = row.Decay;

            var parameters = estimateProvider.EstimateParameters(decay.Times, decay.Signals, this.selectedModel);
            row.Parameters = parameters;
        }
    } // private void EstimateParametersAllRows (IEstimateProvider, ParametersTableRowsEnumerable)

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
        var prev = this.decays.TryGetNearestLowerWavelength(wl, out var prevWl);
        var next = this.decays.TryGetNearestLargerWavelength(wl, out var nextWl);

        if (!(prev || next)) return;  // Only one row exists

        if (prev && next)
        {
            var p = this.parametersTable[prevWl];
            var n = this.parametersTable[nextWl];
            if (p is null || n is null) return;

            for (var i = 0; i < row.Parameters.Count; i++)
                row[i] = (p[i] + n[i]) / 2;
        }
        else if (prev)
        {
            var p = this.parametersTable[prevWl];
            if (p is null) return;
            row.Parameters = p.Parameters;
        }
        else
        {
            var n = this.parametersTable[nextWl];
            if (n is null) return;
            row.Parameters = n.Parameters;
        }

    } // private void TakeAverage (ParametersTableRow)

    #endregion Average and outlier removal

    #region Spectra preview

    private void ShowSpectraPreview(object? sender, EventArgs e)
        => this.spectraPreviewHelper.ShowPreviewWindow(this.selectedModel, this.SelectedWavelength, this.parametersTable.ParametersList, this.decays, (double)this.rangeSelector.Time.To);

    private void UpdatePreviewsParameters(object? sender, EventArgs e)
        => UpdatePreviewsParameters();

    private void UpdatePreviewsParameters()
        => this.spectraPreviewHelper.UpdateParameters(this.selectedModel, this.parametersTable.ParametersList, (double)this.rangeSelector.Time.To);

    internal SpectraSyncObject? GetSyncSpectra(int spectraId)
        => this.spectraPreviewHelper.GetSyncSpectra(spectraId);

    #endregion Spectra preview

    #region analyzers

    private void ShowFourierAnalyzer(object? sender, EventArgs e)
    {
        var analyzer = new FourierAnalyzer();
        if (this.row is not null)
            analyzer.SetDecay(this.row.Decay, this.row.Wavelength);
        this.analyzers.Add(analyzer);
        analyzer.FormClosed += (_, _) => this.analyzers.Remove(analyzer);
        analyzer.Show();
    } // private void ShowFourierAnalyzer (object?, EventArgs)

    private void UpdateAnalyzers(object? sender, EventArgs e)
        => UpdateAnalyzers();

    private void UpdateAnalyzers()
    {
        if (this.row is null) return;
        var decay = this.row.Decay;
        var wavelength = this.row.Wavelength;
        foreach (var analyzer in this.analyzers)
            analyzer.SetDecay(decay, wavelength);
    } // private void UpdateAnalyzers ()

    #endregion analyzers

    private static void EditFilenameFormat(object? sender, EventArgs e)
    {
        using var dialog = new FileNameFormatDialog()
        {
            AMinusBFormat = Program.AMinusBSignalFormat,
            BFormat = Program.BSignalFormat,
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        Program.AMinusBSignalFormat = dialog.AMinusBFormat;
        Program.BSignalFormat = dialog.BFormat;
    } // private static void EditFilenameFormat (object?, EventArgs)

    #region update

    private static void OnNewerReleaseFound(object? sender, NewerVersionFoundEventArgs e)
    {
        var release = e.LatestRelease;
        var url = release.BrowserUrl;
        if (string.IsNullOrEmpty(url)) return;

        var toast = new ToastNotification($"Version {release.Version} is available.", "A newer version is available.")
            .AddText($"You are using version {UpdateManager.CurrentVersion.Version}.")
            .AddButton("Download", args => Program.OpenUrl(url))
            .AddButton("Later");
        toast.Show();
    } // private static void OnNewerReleaseFound (object?, NewerVersionFoundEventArgs)

    async private static void ForceCheckUpdate(object? sender, EventArgs e)
    {
        var release = await UpdateManager.GetLatestVersionAsync(true);
        // If newer version is found, the update manager raises the event, and no need to show the message here.
        if (release.IsNewerThan(UpdateManager.CurrentVersion)) return;

        var toast = new ToastNotification($"You are using the latest version ({UpdateManager.CurrentVersion.Version}).", "No updates found.")
            .AddButton("OK");
        toast.Show();
    } // private static void ForceCheckUpdate (object?, EventArgs)

    #endregion update
} // internal sealed partial class MainWindow : Form

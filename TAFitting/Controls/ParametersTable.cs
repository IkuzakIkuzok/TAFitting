
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Data;
using TAFitting.Model;
using TAFitting.Stats;

namespace TAFitting.Controls;

/// <summary>
/// Represents a table of parameters.
/// </summary>
[DesignerCategory("Code")]
internal sealed partial class ParametersTable : DataGridView
{
    private ParameterConstraints[] constraints = [];
    private double[] initialValues = [];
    private int[] magnitudeColumns = [];
    private bool stopUpdateRSquared = false;

    private readonly UndoBuffer<ParamsEditCommand> undoBuffer = new();
    private double oldValue, newValue;

    internal IFittingModel? Model { get; private set; }

    private int ParametersCount => this.constraints.Length;

    /// <summary>
    /// Occurs when the selected row is changed.
    /// </summary>
    internal event ParametersTableSelectionChangedEventHandler? SelectedRowChanged;

    /// <summary>
    /// Gets the parameter rows.
    /// </summary>
    internal IEnumerable<ParametersTableRow> ParameterRows
        => this.Rows.OfType<ParametersTableRow>();

    /// <summary>
    /// Gets the not edited rows.
    /// </summary>
    internal IEnumerable<ParametersTableRow> NotEditedRows
        => this.ParameterRows.Where(row => !row.Edited);

    /// <summary>
    /// Gets the selected row.
    /// </summary>
    internal ParametersTableRow? SelectedRow
    {
        get
        {
            var selected = this.SelectedCells;
            if (selected.Count == 0) return null;
            var cell = selected[0];
            return cell.OwningRow as ParametersTableRow;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the parameters are edited.
    /// </summary>
    internal bool Edited
        => this.ParameterRows.Any(row => row.Edited);

    /// <summary>
    /// Gets or sets a value indicating whether to stop updating R-squared values.
    /// </summary>
    internal bool StopUpdateRSquared
    {
        get => this.stopUpdateRSquared;
        set
        {
            if (this.stopUpdateRSquared == value) return;
            this.stopUpdateRSquared = value;
            if (value)
            {
                ControlDrawingSuspender.StopPainting(this);
            }
            else
            {
                RecalculateRSquared();
                ControlDrawingSuspender.ResumePainting(this);
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to stop selection changed event.
    /// </summary>
    internal bool StopSelectionChanged { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the undo operation can be performed.
    /// </summary>
    /// <value><see langword="true"/> if the undo operation can be performed; otherwise, <see langword="false"/>.</value>
    internal bool CanUndo => this.undoBuffer.CanUndo;

    /// <summary>
    /// Gets a value indicating whether the redo operation can be performed.
    /// </summary>
    /// <value><see langword="true"/> if the redo operation can be performed; otherwise, <see langword="false"/>.</value>
    internal bool CanRedo => this.undoBuffer.CanRedo;

    /// <summary>
    /// Gets the parameter row at the specified wavelength.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <returns>The parameter row at the specified wavelength.</returns>
    internal ParametersTableRow? this[double wavelength]
        => this.ParameterRows.FirstOrDefault(row => row.Wavelength == wavelength);

    /// <summary>
    /// Initializes a new instance of the <see cref="ParametersTable"/> class.
    /// </summary>
    internal ParametersTable()
    {
        this.AllowUserToAddRows = false;
        this.MultiSelect = false;
        this.DefaultCellStyle.SelectionBackColor = Color.Gray;
        this.DefaultCellStyle.SelectionForeColor = Color.White;
    } // ctor ()

    /// <inheritdoc/>
    override protected void OnKeyDown(KeyEventArgs e)
    {
        // Suppress moving to the next row when pressing Enter key
        if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true;
        base.OnKeyDown(e);
    } // override protected void OnKeyDown (KeyEventArgs)

    /// <inheritdoc/>
    override protected bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Enter && this.IsCurrentCellInEditMode)
        {
            EndEdit();
            return true;  // Suppress moving to the next row after committing the edit
        }
        return base.ProcessDialogKey(keyData);
    } // override protected bool ProcessDialogKey (Keys)

    /// <inheritdoc/>
    override protected void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
    {
        NegativeSignHandler.SetHyphenMinus();
        this.oldValue = (double)this[e.ColumnIndex, e.RowIndex].Value;
        base.OnCellBeginEdit(e);
    } // override protected void OnCellBeginEdit (DataGridViewCellCancelEventArgs)

    /// <inheritdoc/>
    override protected void OnCellEndEdit(DataGridViewCellEventArgs e)
    {
        NegativeSignHandler.SetMinusSign();
        this.newValue = (double)this[e.ColumnIndex, e.RowIndex].Value;
        base.OnCellEndEdit(e);

        if (this.StopUpdateRSquared) return;
        if (this.oldValue == this.newValue) return;
        var row = (ParametersTableRow)this.Rows[e.RowIndex];
        var wl = row.Wavelength;
        var idx = e.ColumnIndex - 1;
        this.undoBuffer.Push(new(this, wl, idx, this.oldValue, this.newValue));
    } // override protected void OnCellEndEdit (DataGridViewCellEventArgs)

    /// <inheritdoc/>
    override protected void OnCellValidating(DataGridViewCellValidatingEventArgs e)
    {
        base.OnCellValidating(e);

        this.Rows[e.RowIndex].ErrorText = string.Empty;

        if (e.ColumnIndex < 1 || e.ColumnIndex > this.ParametersCount) return;

        if (!double.TryParse(e.FormattedValue?.ToString(), out var value))
        {
            e.Cancel = true;
            this.Rows[e.RowIndex].ErrorText = "Invalid value.";
            BeginEdit(true);
            return;
        }

        var constraint = this.constraints[e.ColumnIndex - 1];

        if (constraint.HasFlag(ParameterConstraints.Integer))
        {
            if (value % 1 != 0)
            {
                e.Cancel = true;
                this.Rows[e.RowIndex].ErrorText = "Integer value is required.";
                BeginEdit(true);
                return;
            }
        }

        if (constraint.HasFlag(ParameterConstraints.Positive))
        {
            if (value <= 0)
            {
                e.Cancel = true;
                this.Rows[e.RowIndex].ErrorText = "Positive value is required.";
                BeginEdit(true);
                return;
            }
        }

        if (constraint.HasFlag(ParameterConstraints.NonNegative))
        {
            if (value < 0)
            {
                e.Cancel = true;
                this.Rows[e.RowIndex].ErrorText = "Non-negative value is required.";
                BeginEdit(true);
                return;
            }
        }
    } // override protected void OnCellValidating (DataGridViewCellValidatingEventArgs)

    /// <inheritdoc/>
    override protected void OnCellValueChanged(DataGridViewCellEventArgs e)
    {
        if (this.StopUpdateRSquared) return;
        if (e.RowIndex < 0) return;
        if (e.ColumnIndex < 1 || e.ColumnIndex > this.ColumnCount - 2) return;

        base.OnCellValueChanged(e);

        if (this.Rows[e.RowIndex] is not ParametersTableRow row) return;
        CalculateRSquared(row);
    } // override protected void OnCellValueChanged (DataGridViewCellEventArgs)

    /// <summary>
    /// Undoes the last command.
    /// </summary>
    internal void Undo()
        => this.undoBuffer.Undo();

    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    internal void Redo()
        => this.undoBuffer.Redo();

    /// <summary>
    /// Clears the undo buffer.
    /// </summary>
    internal void ClearUndoBuffer()
    {
        this.undoBuffer.Clear();
    } // internal void ClearUndoBuffer ()

    /// <summary>
    /// Sets the columns with the specified model.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    internal void SetColumns(IFittingModel? model)
    {
        this.Rows.Clear();
        this.Columns.Clear();

        if (model is null) return;

        this.Model = model;

        var col_wavelength = new DataGridViewTextBoxColumn
        {
            HeaderText = "Wavelength",
            DataPropertyName = "Wavelength",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
        };
        this.Columns.Add(col_wavelength);

        var parameters = model.Parameters;
        this.constraints = new ParameterConstraints[parameters.Count];
        this.initialValues = new double[parameters.Count];
        foreach ((var i, var parameter) in parameters.Enumerate())
        {
            var name = parameter.Name;
            var col = new DataGridViewNumericBoxColumn(parameter.InitialValue)
            {
                HeaderText = name,
                DataPropertyName = name,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            };
            col.HeaderCell.ContextMenuStrip = GetColumnContextMenu(col);
            this.Columns.Add(col);
            this.constraints[i] = parameter.Constraints;
            this.initialValues[i] = parameter.InitialValue;
        }

        this.magnitudeColumns = [..
            parameters
            .Select((p, i) => (Parameter: p, Index: i))
            .Where(item => item.Parameter.IsMagnitude)
            .Select(item => item.Index)
        ];

        var col_r2 = new DataGridViewTextBoxColumn
        {
            HeaderText = "R²",
            DataPropertyName = "R2",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_r2);
    } // internal void SetColumns (IFittingModel?)

    /// <summary>
    /// Gets the context menu of the specified column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>The context menu of the specified column.</returns>
    private ContextMenuStrip GetColumnContextMenu(DataGridViewColumn column)
    {
        var menu = new ContextMenuStrip();

        var batchInput = new ToolStripMenuItem("Batch input");
        menu.Items.Add(batchInput);

        var batchInputAll = new ToolStripMenuItem("All rows")
        {
            Tag = column,
        };
        batchInputAll.Click += BatchInputAllRows;
        batchInput.DropDownItems.Add(batchInputAll);

        var batchInputNotEdited = new ToolStripMenuItem("Not edited rows only")
        {
            Tag = column,
        };
        batchInputNotEdited.Click += BatchInputNotEditedRowsOnly;
        batchInput.DropDownItems.Add(batchInputNotEdited);

        return menu;
    } // private ContextMenuStrip GetColumnContextMenu (DataGridViewColumn)

    private void BatchInputAllRows(object? sender, EventArgs e)
    {
        if (this.Rows.Count == 0) return;
        if (sender is not ToolStripMenuItem menuItem) return;
        if (menuItem.Tag is not DataGridViewNumericBoxColumn column) return;
        BatchInput(column, this.ParameterRows);
    } // private void BatchInputAllRows (object?, EventArgs)

    private void BatchInputNotEditedRowsOnly(object? sender, EventArgs e)
    {
        if (this.Rows.Count == 0) return;
        if (sender is not ToolStripMenuItem menuItem) return;
        if (menuItem.Tag is not DataGridViewNumericBoxColumn column) return;
        BatchInput(column, this.NotEditedRows);
    } // private void BatchInputNotEditedRowsOnly (object?, EventArgs

    /// <summary>
    /// Batch inputs the specified column to the rows.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="rows">The rows.</param>
    private void BatchInput(DataGridViewNumericBoxColumn column, IEnumerable<ParametersTableRow> rows)
    {
        // Casting decimal.MinValue and decimal.MaxValue to double results in the OverflowException.
        // Therefore, the approximate values of them are used.
        const double DecimalMin = -7.9e28;  // -79,228,162,514,264,337,593,543,950,335
        const double DecimalMax = +7.9e28;  // +79,228,162,514,264,337,593,543,950,335

        var val = Math.Clamp((double)rows.First().Cells[column.Index].Value, DecimalMin, DecimalMax);
        using var nib = new NumericInputBox()
        {
            Text = column.HeaderText,
            Minimum = (decimal)Math.Max(column.Minimum, DecimalMin),
            Maximum = (decimal)Math.Min(column.Maximum, DecimalMax),
            DecimalPlaces = column.DecimalPlaces,
            Value = (decimal)val,
        };
        using var _ = new NegativeSignHandler();
        if (nib.ShowDialog() != DialogResult.OK) return;

        var value = (double)nib.Value;
        var index = column.Index;
        SetFreezeEditedState(true);
        foreach (var row in rows)
            row.Cells[index].Value = value;
        SetFreezeEditedState(false);
    } // private void BatchInputAllRows (DataGridViewNumericBoxColumn, IEnumerable<ParametersTableRow>)

    /// <summary>
    /// Sets the freeze edited state of the rows.
    /// </summary>
    /// <param name="value">The freeze edited state.</param>
    private void SetFreezeEditedState(bool value)
    {
        foreach (var row in this.ParameterRows)
            row.FreezeEditedState = value;
    } // private void SetFreezeEditedState (bool)

    /// <summary>
    /// Adds a row with the specified wavelength.
    /// </summary>
    /// <param name="wavelength">The wavelength.</param>
    /// <returns>The added row.</returns>
    /// <exception cref="Exception">The columns are not set to the current instance.</exception>
    internal ParametersTableRow Add(double wavelength, Decay decay)
    {
        if (this.Columns.Count == 0) throw new Exception("Columns are not set.");

        var row = new ParametersTableRow(decay);
        row.CreateCells(this);
        row.Wavelength = wavelength;
        row.FreezeEditedState = true;
        row.SetMagnitudeColumns(this.magnitudeColumns);
        for (var i = 0; i < this.initialValues.Length; i++)
            row[i] = this.initialValues[i];
        row.RSquared = 0.0;
        row.FreezeEditedState = false;
        this.Rows.Add(row);
        return row;
    } // internal ParametersTableRow Add (double, Decay)

    /// <inheritdoc/>
    override protected void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);

        if (this.StopSelectionChanged) return;

        var cell = this.SelectedCells.Cast<DataGridViewCell>().FirstOrDefault();
        if (cell is null) return;
        var rowIndex = cell.RowIndex;
        var colIndex = cell.ColumnIndex;

        foreach (var col in this.Columns.Cast<DataGridViewColumn>())
            col.DefaultCellStyle.BackColor = col.Index == colIndex ? Color.LightGray : Color.White;

        SelectedRowChanged?.Invoke(this, new ParametersTableSelectionChangedEventArgs(this.ParameterRows.ElementAt(rowIndex)));
    } // override protected void OnSelectionChanged (EventArgs)

    /// <summary>
    /// Recalculates the R-squared values.
    /// </summary>
    internal void RecalculateRSquared()
    {
        foreach (var row in this.ParameterRows)
            CalculateRSquared(row);
    } // internal void RecalculateRSquared ()

    private void CalculateRSquared(ParametersTableRow row)
    {
        if (this.Model is null) return;

        var decay = row.Decay.OnlyAfterT0;

        var func = this.Model.GetFunction(row.Parameters);
        var inverse = row.Inverted ? -1 : 1;
        var scaler = this.Model.YLogScale ? Math.Log10 : (Func<double, double>)(x => x);

        var X = decay.Times.ToArray();
        var Y = decay.Signals.Select(y => scaler(y * inverse)).ToArray();
        var average = Y.AverageNumbers();
        var Se = 0.0;
        var St = 0.0;
        var N = 0;
        if (X.Length != Y.Length) return;
        for (var i = 0; i < X.Length; ++i)
        {
            var x = X[i];
            var y = Y[i];
            if (double.IsNaN(y)) continue;

            var y_fit = scaler(func(x) * inverse);
            if (double.IsNaN(y_fit)) continue;

            var de = y - y_fit;
            var dt = y - average;
            Se += de * de;
            St += dt * dt;
            ++N;
        }
        var p = row.Parameters.Count;
        row.RSquared = 1 - Se / St * (N - 1) / (N - p - 1);
    } // private void CalculateRSquared (ParametersTableRow)
} // internal sealed partial class ParametersTable : DataGridView

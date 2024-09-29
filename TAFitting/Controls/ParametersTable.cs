
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Data;
using TAFitting.Model;

namespace TAFitting.Controls;

/// <summary>
/// Represents a table of parameters.
/// </summary>
[DesignerCategory("Code")]
internal sealed class ParametersTable : DataGridView
{
    private ParameterConstraints[] constraints = [];
    private double[] initialValues = [];
    private int[] magnitudeColumns = [];

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
    /// Gets a value indicating whether the parameters are edited.
    /// </summary>
    internal bool Edited
        => this.ParameterRows.Any(row => row.Edited);

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
        base.OnCellBeginEdit(e);
    } // override protected void OnCellBeginEdit (DataGridViewCellCancelEventArgs)

    /// <inheritdoc/>
    override protected void OnCellEndEdit(DataGridViewCellEventArgs e)
    {
        NegativeSignHandler.SetMinusSign();
        base.OnCellEndEdit(e);
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

    /// <summary>
    /// Sets the columns with the specified model.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    internal void SetColumns(IFittingModel model)
    {
        this.Rows.Clear();
        this.Columns.Clear();

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

        this.magnitudeColumns = parameters
            .Select((p, i) => (Parameter: p, Index: i))
            .Where(item => item.Parameter.IsMagnitude)
            .Select(item => item.Index)
            .ToArray();

        var col_r2 = new DataGridViewTextBoxColumn
        {
            HeaderText = "R²",
            DataPropertyName = "R2",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells,
        };
        this.Columns.Add(col_r2);
    } // internal void SetColumns (IFittingModel)

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
        var nib = new NumericInputBox()
        {
            Text = column.HeaderText,
            Minimum = (decimal)Math.Max(column.Minimum, -1e28),
            Maximum = (decimal)Math.Min(column.Maximum, 1e28),
            DecimalPlaces = column.DecimalPlaces,
            Value = (decimal)(double)this.Rows[0].Cells[column.Index].Value,
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

        var cell = this.SelectedCells.Cast<DataGridViewCell>().FirstOrDefault();
        if (cell is null) return;
        var rowIndex = cell.RowIndex;
        var colIndex = cell.ColumnIndex;

        foreach (var col in this.Columns.Cast<DataGridViewColumn>())
            col.DefaultCellStyle.BackColor = col.Index == colIndex ? Color.LightGray : Color.White;

        SelectedRowChanged?.Invoke(this, new ParametersTableSelectionChangedEventArgs(this.ParameterRows.ElementAt(rowIndex)));
    } // override protected void OnSelectionChanged (EventArgs)
} // internal sealed class ParametersTable : DataGridView


// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Controls;

[DesignerCategory("Code")]
internal sealed class ParametersTable : DataGridView
{
    private ParameterConstraints[] constraints = [];
    private double[] initialValues = [];
    private int[] magnitudeColumns = [];

    internal event ParametersTableSelectionChangedEventHandler? SelectedRowChanged;

    internal IEnumerable<ParametersTableRow> ParameterRows
        => this.Rows.OfType<ParametersTableRow>();

    internal ParametersTableRow? this[double wavelength]
        => this.ParameterRows.FirstOrDefault(row => row.Wavelength == wavelength);

    internal ParametersTable()
    {
        this.AllowUserToAddRows = false;
        this.MultiSelect = false;
        this.DefaultCellStyle.SelectionBackColor = Color.Gray;
        this.DefaultCellStyle.SelectionForeColor = Color.White;
    } // ctor ()

    override protected void OnKeyDown(KeyEventArgs e)
    {
        // Suppress moving to the next row when pressing Enter key
        if (e.KeyCode == Keys.Enter) e.SuppressKeyPress = true;
        base.OnKeyDown(e);
    } // override protected void OnKeyDown (KeyEventArgs)

    override protected bool ProcessDialogKey(Keys keyData)
    {
        if (keyData == Keys.Enter && this.IsCurrentCellInEditMode)
        {
            EndEdit();
            return true;  // Suppress moving to the next row after committing the edit
        }
        return base.ProcessDialogKey(keyData);
    } // override protected bool ProcessDialogKey (Keys)

    override protected void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
    {
        NegativeSignHandler.ChangeNegativeSign("-");
        base.OnCellBeginEdit(e);
    } // override protected void OnCellBeginEdit (DataGridViewCellCancelEventArgs)

    override protected void OnCellEndEdit(DataGridViewCellEventArgs e)
    {
        NegativeSignHandler.ChangeNegativeSign("\u2212");
        base.OnCellEndEdit(e);
    } // override protected void OnCellEndEdit (DataGridViewCellEventArgs)

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
        } // foreach

        this.magnitudeColumns = parameters
            .Select((p, i) => (Parameter: p, Index: i))
            .Where(item => item.Parameter.IsMagnitude)
            .Select(item => item.Index)
            .ToArray();
    } // internal void SetColumns (IFittingModel)

    private ContextMenuStrip GetColumnContextMenu(DataGridViewColumn column)
    {
        var menu = new ContextMenuStrip();

        var batchInput = new ToolStripMenuItem("Batch input")
        {
            Tag = column,
        };
        batchInput.Click += BatchInput;
        menu.Items.Add(batchInput);

        return menu;
    } // private ContextMenuStrip GetColumnContextMenu (DataGridViewColumn)

    private void BatchInput(object? sender, EventArgs e)
    {
        if (this.Rows.Count == 0) return;
        if (sender is not ToolStripMenuItem menuItem) return;
        if (menuItem.Tag is not DataGridViewNumericBoxColumn column) return;

        var nib = new NumericInputBox()
        {
            Text = column.HeaderText,
            Minimum = (decimal)Math.Max(column.Minimum, -1e28),
            Maximum = (decimal)Math.Min(column.Maximum, 1e28),
            DecimalPlaces = column.DecimalPlaces,
            Value = (decimal)(double)this.Rows[0].Cells[column.Index].Value,
        };
        using var _ = new NegativeSignHandler("-");
        if (nib.ShowDialog() != DialogResult.OK) return;

        var value = (double)nib.Value;
        foreach (var row in this.ParameterRows)
            row.Cells[column.Index].Value = value;
    } // private void BatchInput (object?, EventArgs)

    internal ParametersTableRow Add(double wavelength)
    {
        if (this.Columns.Count == 0) throw new Exception("Columns are not set.");

        var row = new ParametersTableRow();
        row.CreateCells(this);
        row.Wavelength = wavelength;
        row.SetMagnitudeColumns(this.magnitudeColumns);
        for (var i = 0; i < this.initialValues.Length; i++)
            row[i] = this.initialValues[i];
        this.Rows.Add(row);
        return row;
    } // internal ParametersTableRow Add (double)

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

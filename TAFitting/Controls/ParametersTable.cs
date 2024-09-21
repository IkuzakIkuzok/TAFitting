
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Controls;

[DesignerCategory("Code")]
internal sealed class ParametersTable : DataGridView
{
    private ParameterConstraints[] constraints = [];
    private double[] initialValues = [];

    internal event ParametersTableSelectionChangedEventHandler? SelectedRowChanged;

    internal IEnumerable<ParametersTableRow> ParameterRows
        => this.Rows.OfType<ParametersTableRow>();

    internal ParametersTable()
    {
        this.AllowUserToAddRows = false;
        this.MultiSelect = false;
        this.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    } // ctor ()

    internal void SetColumns(IFittingModel model)
    {
        this.Rows.Clear();
        this.Columns.Clear();

        var col_wavelength = new DataGridViewTextBoxColumn
        {
            HeaderText = "Wavelength",
            DataPropertyName = "Wavelength",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
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
            this.Columns.Add(col);
            this.constraints[i] = parameter.Constraints;
            this.initialValues[i] = parameter.InitialValue;
        } // foreach
    } // internal void SetColumns (IFittingModel)

    internal void Add(double wavelength)
    {
        if (this.Columns.Count == 0) throw new Exception("Columns are not set.");

        var row = new ParametersTableRow();
        row.CreateCells(this);
        row.Wavelength = wavelength;
        for (var i = 0; i < this.initialValues.Length; i++)
            row[i] = this.initialValues[i];
        this.Rows.Add(row);
    } // internal void Add (double)

    override protected void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);

        var row = this.SelectedRows.Cast<ParametersTableRow>().FirstOrDefault();
        if (row is null) return;
        SelectedRowChanged?.Invoke(this, new ParametersTableSelectionChangedEventArgs(row));
    } // override protected void OnSelectionChanged (EventArgs)
} // internal sealed class ParametersTable : DataGridView

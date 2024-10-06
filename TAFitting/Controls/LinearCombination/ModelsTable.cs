
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Controls.LinearCombination;

[DesignerCategory("Code")]
internal sealed class ModelsTable : DataGridView
{
    internal IEnumerable<ModelRow> ModelRows
        => this.Rows.OfType<ModelRow>();

    internal ModelsTable()
    {
        this.AllowUserToAddRows = false;
        
        var col_name = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Name",
            DataPropertyName = "Name",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_name); // 0

        var col_category = new DataGridViewTextBoxColumn()
        {
            HeaderText = "Category",
            DataPropertyName = "Category",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_category); // 1

        var col_numParams = new DataGridViewTextBoxColumn()
        {
            HeaderText = "# of parameters",
            DataPropertyName = "NumParams",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
        };
        this.Columns.Add(col_numParams); // 2
    } // ctor ()

    internal void AddModel(ModelItem modelItem)
    {
        this.Rows.Add(new ModelRow(modelItem));
    } // internal void AddModel (ModelItem)
} // internal sealed class ModelsTable : DataGridView

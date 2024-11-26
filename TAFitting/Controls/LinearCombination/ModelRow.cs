
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Model;

namespace TAFitting.Controls.LinearCombination;

internal sealed partial class ModelRow : DataGridViewRow
{
    internal IFittingModel Model { get; }

    internal ModelRow(ModelItem modelItem)
    {
        this.Model = modelItem.Model;
        this.Cells.Add(new DataGridViewTextBoxCell() { Value = this.Model.Name });
        this.Cells.Add(new DataGridViewTextBoxCell() { Value = modelItem.Category });
        this.Cells.Add(new DataGridViewTextBoxCell() { Value = this.Model.Parameters.Count });
    } // internal ModelRow (ModelItem)
} // internal sealed partial class ModelRow : DataGridViewRow

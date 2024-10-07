
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics;

namespace TAFitting.Model;

[DebuggerDisplay("[{Category,nq}] {Model.Name,nq}")]
internal sealed class ModelItem
{
    internal IFittingModel Model { get; }

    internal string Category { get; }

    internal ModelItem(IFittingModel model, string category)
    {
        this.Model = model;
        this.Category = category;
    } // internal ModelItem (IFittingModel, category)

    override public string ToString()
        => this.Model.Name;
} // internal sealed class ModelItem

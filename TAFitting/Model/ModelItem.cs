
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

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

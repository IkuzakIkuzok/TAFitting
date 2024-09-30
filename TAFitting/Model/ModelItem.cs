
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

internal sealed class ModelItem
{
    internal IFittingModel Model { get; }

    internal string Category { get; }

    internal ModelItem(IFittingModel model)
    {
        this.Model = model;
        this.Category = this.Model.GetType().Namespace?.Split('.').Last() ?? string.Empty;
    } // internal ModelItem (IFittingModel)
} // internal sealed class ModelItem

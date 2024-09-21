
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Model;

internal sealed class ModelItem
{
    internal IFittingModel Model { get; }

    internal ModelItem(IFittingModel model)
    {
        this.Model = model;
    } // internal ModelItem (IFittingModel)
} // internal sealed class ModelItem

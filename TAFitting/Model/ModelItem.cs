
// (c) 2024 Kazuki KOHZUKI

using System.Diagnostics;

namespace TAFitting.Model;

/// <summary>
/// Represents an item of fitting model with its category.
/// </summary>
[DebuggerDisplay("[{Category,nq}] {Model.Name,nq}")]
internal sealed class ModelItem
{
    /// <summary>
    /// Gets the fitting model.
    /// </summary>
    internal IFittingModel Model { get; }

    /// <summary>
    /// Gets the category of the fitting model.
    /// </summary>
    internal string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelItem"/> class.
    /// </summary>
    /// <param name="model">The fitting model.</param>
    /// <param name="category">The category of the fitting model.</param>
    internal ModelItem(IFittingModel model, string category)
    {
        this.Model = model;
        this.Category = category;
    } // internal ModelItem (IFittingModel, category)

    override public string ToString()
        => this.Model.Name;
} // internal sealed class ModelItem

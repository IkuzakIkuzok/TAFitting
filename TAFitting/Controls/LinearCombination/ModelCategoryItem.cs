
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls.LinearCombination;

internal sealed class ModelCategoryItem
{
    internal bool IsDefined { get; init; } = true;

    internal string Category { get; }

    internal ModelCategoryItem(string category)
    {
        this.Category = category;
    } // internal ModelCategoryItem (string)

    override public string ToString()
        => this.Category;
} // internal sealed class ModelCategoryItem

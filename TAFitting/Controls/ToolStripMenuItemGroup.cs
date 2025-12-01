
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Controls;

/// <summary>
/// Represents a group of generic ToolStrip menu items that manages their selection state and notifies listeners when the selection changes.
/// </summary>
/// <remarks>This class ensures that only one menu item in the group is selected at a time, unless zero selection is allowed.
/// It provides events to notify when the selection changes, enabling integration with custom selection logic or UI updates.</remarks>
/// <typeparam name="T">The type of value associated with each menu item in the group.</typeparam>
internal sealed class ToolStripMenuItemGroup<T>
{
    private readonly List<GenericToolStripMenuItem<T>> items = [];

    /// <summary>
    /// Gets a value indicating whether a selection of zero is considered valid.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the group allows no items to be selected; otherwise, <see langword="false"/>.
    /// </value>
    internal bool AcceptZeroSelection { get; init; } = false;

    /// <summary>
    /// Occurs when the selection within the group of menu items changes.
    /// </summary>
    /// <remarks>This event is triggered whenever the selected item in the group is modified, allowing subscribers to respond to selection changes.
    /// Handlers can use the event arguments to determine which item was selected or deselected.</remarks>
    internal event ToolStripMenuItemGroupSelectionChangedEventHandler<T>? SelectionChanged;

    /// <summary>
    /// Initializes a new instance of the ToolStripMenuItemGroup class.
    /// </summary>
    internal ToolStripMenuItemGroup() { }

    /// <summary>
    /// Initializes a new instance of the ToolStripMenuItemGroup class
    /// with the specified selection changed event handler and zero-selection acceptance setting.
    /// </summary>
    /// <param name="selectionChanged">The delegate to invoke when the selection in the group changes.</param>
    /// <param name="acceptZeroSelection">A value indicating whether the group allows no items to be selected.
    /// If <see langword="true"/>, zero selection s permitted; otherwise, at least one item must be selected.</param>
    internal ToolStripMenuItemGroup(ToolStripMenuItemGroupSelectionChangedEventHandler<T> selectionChanged, bool acceptZeroSelection = false)
    {
        SelectionChanged += selectionChanged;
        this.AcceptZeroSelection = acceptZeroSelection;
    } // ctor (ToolStripMenuItemGroupSelectionChangedEventHandler<T>, bool)

    /// <summary>
    /// Adds the specified menu item to the collection.
    /// </summary>
    /// <param name="item">The menu item to add to the collection.</param>
    internal void AddItem(GenericToolStripMenuItem<T> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.items.Add(item);
    } // internal void AddItem (GenericToolStripMenuItem<T>)

    /// <summary>
    /// Removes the specified item from the collection of menu items.
    /// </summary>
    /// <param name="item">The menu item to remove from the collection.</param>
    internal void RemoveItem(GenericToolStripMenuItem<T> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        this.items.Remove(item);
    } // internal void RemoveItem (GenericToolStripMenuItem<T>)

    /// <summary>
    /// Removes all items from the collection.
    /// </summary>
    internal void RemoveAll()
    {
        this.items.Clear();
    } // internal void RemoveAll ()

    /// <summary>
    /// Updates the selection state of menu items in response to a click event and raises the SelectionChanged event.
    /// </summary>
    /// <param name="item">The menu item that was clicked. This item will be selected or deselected based on the current selection rules.</param>
    internal void NotifyItemClicked(GenericToolStripMenuItem<T> item)
    {
        var originalCheckedState = item.Checked;
        foreach (var i in this.items)
            i.Checked = false;

        item.Checked = !this.AcceptZeroSelection || !originalCheckedState;
        SelectionChanged?.Invoke(this, new(item));
    } // internal void NotifyItemClicked (GenericToolStripMenuItem<T>)
} // internal sealed class ToolStripMenuItemGroup

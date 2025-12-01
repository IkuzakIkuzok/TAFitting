
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Controls;

internal delegate void ToolStripMenuItemGroupSelectionChangedEventHandler<T>(object? sender, ToolStripMenuItemGroupSelectionChangedEventArgs<T> e);

/// <summary>
/// Provides data for an event that is raised when the selected item in a group of generic ToolStripMenuItems changes.
/// </summary>
/// <typeparam name="T">The type of value associated with each ToolStripMenuItem in the group.</typeparam>
internal sealed class ToolStripMenuItemGroupSelectionChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// Gets the currently selected item in the menu, or \<see langword="null"/> if no item is selected.
    /// </summary>
    internal GenericToolStripMenuItem<T>? SelectedItem { get; }

    /// <summary>
    /// Initializes a new instance of the ToolStripMenuItemGroupSelectionChangedEventArgs class with the specified selected item.
    /// </summary>
    /// <remarks>If the provided menu item is not checked, the <see cref="SelectedItem"/> property will be <see langword="nuint"/>.
    /// This ensures that only checked items are considered as selected in the event arguments.</remarks>
    /// <param name="selectedItem">The menu item that was selected.</param>
    internal ToolStripMenuItemGroupSelectionChangedEventArgs(GenericToolStripMenuItem<T> selectedItem)
    {
        this.SelectedItem = selectedItem.Checked ? selectedItem : null;
    } // ctor (GenericToolStripMenuItem<T>)
} // internal sealed class ToolStripMenuItemGroupSelectionChangedEventArgs


// (c) 2025 Kazuki Kohzuki


namespace TAFitting.Controls;

/// <summary>
/// Represents a ToolStripMenuItem that associates a strongly typed value with the menu item using a generic tag property.
/// </summary>
/// <typeparam name="T">The value type to associate with the menu item.</typeparam>
[DesignerCategory("code")]
internal class GenericToolStripMenuItem<T> : ToolStripMenuItem
{
    /// <summary>
    /// Gets or sets an object that contains data about the control.
    /// </summary>
    new public T Tag { get; set; }

    /// <summary>
    /// Gets or sets the group to which this menu item belongs.
    /// </summary>
    /// <remarks>Changing the group will automatically remove the item from its previous group and add it to the new group.
    /// This property is intended for internal use and may affect the grouping behavior of menu items within a container.</remarks>
    internal ToolStripMenuItemGroup<T>? Group
    {
        get;
        set
        {
            if (field == value) return;
            field?.RemoveItem(this);
            (field = value)?.AddItem(this);
        }
    }

    /// <summary>
    /// Initializes a new instance of the GenericToolStripMenuItem class with the specified display text, associated tag value, and click event handler.
    /// </summary>
    /// <param name="text">The text to display on the menu item.</param>
    /// <param name="tag">The value to associate with the menu item. This value is assigned to the <see cref="Tag"/> property.</param>
    /// <param name="group">The group to which this menu item belongs. If provided, the menu item will be added to the specified group.</param>
    internal GenericToolStripMenuItem(string text, T tag, ToolStripMenuItemGroup<T>? group = null) : base(text)
    {
        this.Tag = tag;
        this.Group = group;
    } // ctor (string, T, ToolStripMenuItemGroup<T>?)

    override protected void OnClick(EventArgs e)
    {
        this.Group?.NotifyItemClicked(this);
        base.OnClick(e);
    } // override protected void OnClick (EventArgs)
} // internal class GenericToolStripMenuItem<T> : ToolStripMenuItem

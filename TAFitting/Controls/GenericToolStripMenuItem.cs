
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Controls;

/// <summary>
/// Represents a ToolStripMenuItem that associates a strongly typed value with the menu item using a generic tag property.
/// </summary>
/// <typeparam name="T">The value type to associate with the menu item. Must be a struct.</typeparam>
[DesignerCategory("code")]
internal class GenericToolStripMenuItem<T> : ToolStripMenuItem where T : struct
{
    /// <summary>
    /// Gets or sets an object that contains data about the control.
    /// </summary>
    new public T Tag { get; set; }

    /// <summary>
    /// Initializes a new instance of the GenericToolStripMenuItem class with the specified display text, associated tag value, and click event handler.
    /// </summary>
    /// <param name="text">The text to display on the menu item.</param>
    /// <param name="tag">The value to associate with the menu item. This value is assigned to the <see cref="Tag"/> property.</param>
    /// <param name="onClick">The event handler to invoke when the menu item is clicked.</param>
    internal GenericToolStripMenuItem(string text, T tag, EventHandler? onClick) : base(text, null, onClick)
    {
        this.Tag = tag;
    } // ctor (string, T, EventHandler?)
} // internal class GenericToolStripMenuItem<T> : ToolStripMenuItem

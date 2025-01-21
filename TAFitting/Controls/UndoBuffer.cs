
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Provides a buffer for undo and redo operations.
/// </summary>
/// <typeparam name="T">The type of the undo command.</typeparam>
internal class UndoBuffer<T> where T : IUndoCommand
{
    protected readonly Stack<T> undo_buffer = [];
    protected readonly Stack<T> redo_buffer = [];

    /// <summary>
    /// Gets a value indicating whether the undo operation can be performed.
    /// </summary>
    /// <value><see langword="true"/> if the undo operation can be performed; otherwise, <see langword="false"/>.</value>
    internal bool CanUndo => this.undo_buffer.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the redo operation can be performed.
    /// </summary>
    /// <value><see langword="true"/> if the redo operation can be performed; otherwise, <see langword="false"/>.</value>
    internal bool CanRedo => this.redo_buffer.Count > 0;

    /// <summary>
    /// Pushes a command to the undo buffer.
    /// </summary>
    /// <param name="command">The command to be pushed.</param>
    internal void Push(T command)
    {
        this.undo_buffer.Push(command);
    } // internal void Push (T)

    /// <summary>
    /// Undoes the last command.
    /// </summary>
    internal virtual void Undo()
    {
        if (!this.CanUndo) return;
        var command = this.undo_buffer.Pop();
        command.Undo();
        this.redo_buffer.Push(command);
    } // internal virtual void Undo ()

    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    internal virtual void Redo()
    {
        if (!this.CanRedo) return;
        var command = this.redo_buffer.Pop();
        command.Redo();
        this.undo_buffer.Push(command);
    } // internal virtual void Redo ()

    /// <summary>
    /// Clears the undo and redo buffers.
    /// </summary>
    internal void Clear()
    {
        this.undo_buffer.Clear();
        this.redo_buffer.Clear();
    } // internal void Clear ()
} // internal class UndoBuffer<T> where T : IUndoCommand

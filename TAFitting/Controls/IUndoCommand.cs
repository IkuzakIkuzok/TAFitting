
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Represents a command that can be undone and redone.
/// </summary>
internal interface IUndoCommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    void Redo();

    /// <summary>
    /// Undoes the command.
    /// </summary>
    void Undo();
} // internal interface IUndoCommand

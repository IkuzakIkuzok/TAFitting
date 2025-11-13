
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Represents a command that edits a parameter.
/// </summary>
/// <param name="table">The parameters table.</param>
/// <param name="wavelength">The wavelength of the parameter.</param>
/// <param name="index">The column index of the parameter.</param>
/// <param name="oldValue">The old value of the parameter.</param>
/// <param name="newValue">The new value of the parameter.</param>
internal sealed class ParamsEditCommand(ParametersTable table, double wavelength, int index, double oldValue, double newValue) : IUndoCommand
{
    private readonly ParametersTable table = table;

    /// <summary>
    /// Gets the wavelength of the parameter.
    /// </summary>
    internal double Wavelength { get; } = wavelength;

    /// <summary>
    /// Gets the column index of the parameter.
    /// </summary>
    internal int Index { get; } = index;

    /// <summary>
    /// Gets the old value of the parameter.
    /// </summary>
    internal double OldValue { get; } = oldValue;

    /// <summary>
    /// Gets the new value of the parameter.
    /// </summary>
    internal double NewValue { get; } = newValue;

    public void Redo()
    {
        var row = this.table[this.Wavelength];
        row?[this.Index] = this.NewValue;
    } // public void Redo()

    public void Undo()
    {
        var row = this.table[this.Wavelength];
        row?[this.Index] = this.OldValue;
    } // public void Undo()
} // internal sealed class ParamsEditCommand : IUndoCommand

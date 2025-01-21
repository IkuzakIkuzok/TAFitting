
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Controls;

internal sealed class ParamsEditCommand(ParametersTable table, double wavelength, int index, double oldValue, double newValue) : IUndoCommand
{
    private readonly ParametersTable table = table;

    internal double Wavelength { get; } = wavelength;

    internal int Index { get; } = index;

    internal double OldValue { get; } = oldValue;

    internal double NewValue { get; } = newValue;

    public void Redo()
    {
        var row = this.table[this.Wavelength];
        if (row is not null) row[this.Index] = this.NewValue;
    } // public void Redo()

    public void Undo()
    {
        var row = this.table[this.Wavelength];
        if (row is not null) row[this.Index] = this.OldValue;
    } // public void Undo()
} // internal sealed class ParamsEditCommand : IUndoCommand

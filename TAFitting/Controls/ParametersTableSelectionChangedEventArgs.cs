
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

internal delegate void ParametersTableSelectionChangedEventHandler(object? sender, ParametersTableSelectionChangedEventArgs e);

internal sealed class ParametersTableSelectionChangedEventArgs : EventArgs
{
    internal ParametersTableRow Row { get; }

    internal ParametersTableSelectionChangedEventArgs(ParametersTableRow row)
    {
        this.Row = row;
    } // ctor () (ParametersTableRow)
} // internal sealed class ParametersTableSelectionChangedEventArgs : EventArgs


// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

internal delegate void ParametersTableSelectionChangedEventHandler(object? sender, ParametersTableSelectionChangedEventArgs e);

/// <summary>
/// Represents the event data for the <see cref="ParametersTableSelectionChangedEventHandler"/> delegate.
/// </summary>
internal sealed class ParametersTableSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the newly selected row.
    /// </summary>
    internal ParametersTableRow Row { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParametersTableSelectionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="row">The newly selected row.</param>
    internal ParametersTableSelectionChangedEventArgs(ParametersTableRow row)
    {
        this.Row = row;
    } // ctor () (ParametersTableRow)
} // internal sealed class ParametersTableSelectionChangedEventArgs : EventArgs

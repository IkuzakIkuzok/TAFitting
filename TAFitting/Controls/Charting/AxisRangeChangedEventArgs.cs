
// (c) 2024 Kazuki Kohzuki

using System.Windows.Forms.DataVisualization.Charting;

namespace TAFitting.Controls.Charting;

internal delegate void AxisRangeChangedEventHandler(object? sender, AxisRangeChangedEventArgs e);

/// <summary>
/// Represents the event data for the <see cref="AxisRangeChangedEventHandler"/> delegate.
/// </summary>
internal sealed class AxisRangeChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the minimum value of the axis.
    /// </summary>
    internal double Minimum { get; init; }

    /// <summary>
    /// Gets the maximum value of the axis.
    /// </summary>
    internal double Maximum { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AxisRangeChangedEventArgs"/> class.
    /// </summary>
    /// <param name="axis">The axis that the range has been changed.</param>
    internal AxisRangeChangedEventArgs(Axis axis)
    {
        this.Minimum = axis.Minimum;
        this.Maximum = axis.Maximum;
    } // internal AxisRangeChangedEventArgs (Axis)
} // internal sealed class AxisRangeChangedEventArgs : EventArgs

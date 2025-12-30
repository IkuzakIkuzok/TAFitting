
// (c) 2024 Kazuki KOHZUKI

using TAFitting.Config;
using TAFitting.Data;

namespace TAFitting.Controls;

/// <summary>
/// Represents a row of parameters.
/// </summary>
internal sealed partial class ParametersTableRow : DataGridViewRow
{
    private bool inverted = false;
    private readonly List<double> parameters = [];
    private int[] magnitudeColumns = [];

    private static RSquaredColorTable ColorTable => field ??= new(Program.Config.AppearanceConfig.RSquaredThresholds);

    private double GetCellValue(int index, double defaultValue) =>
        this.Cells[index].Value is double value ? value : defaultValue;

    /// <summary>
    /// Gets or sets the wavelength.
    /// </summary>
    internal double Wavelength
    {
        get => GetCellValue(0, 0.0);
        set => this.Cells[0].Value = value;
    }

    /// <summary>
    /// Gets or sets the decay.
    /// </summary>
    internal Decay Decay { get; set; }

    /// <summary>
    /// Gets the number of parameters.
    /// </summary>
    internal int ParametersCount
        => this.Cells.Count - 2;

    /// <summary>
    /// Gets or sets the parameter at the specified index.
    /// </summary>
    /// <param name="index">The index of the parameter.</param>
    /// <returns>The parameter at the specified index.</returns>
    internal double this[int index]
    {
        get
        {
            var cell = this.Cells[index + 1];
            if (cell.Value is double value) return value;
            if (cell is DataGridViewNumericBoxCell numericBoxCell)
                return numericBoxCell.DefaultValue;
            return 0.0;
        }
        set => this.Cells[index + 1].Value = value;
    }

    /// <summary>
    /// Gets or sets the parameters.
    /// </summary>
    internal IReadOnlyList<double> Parameters
    {
        get
        {
            var count = this.ParametersCount;
            this.parameters.Clear();
            this.parameters.EnsureCapacity(count);
            for (var i = 0; i < count; i++)
                this.parameters.Add(this[i]);
            return this.parameters;
        }
        set
        {
            for (var i = 0; i < value.Count; i++)
                this[i] = value[i];
        }
    }

    /// <summary>
    /// Gets or sets the R² value.
    /// </summary>
    internal double RSquared
    {
        get => GetCellValue(this.Cells.Count - 1, 0.0);
        set
        {
            var cell = this.Cells[this.Cells.Count - 1];
            cell.Value = NegativeSignHandler.FormatWithMinusSign(value, "F3");
            cell.Style.BackColor = GetRSquaredColor(value);
            this.DataGridView?.InvalidateCell(cell);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the parameters are inverted.
    /// </summary>
    internal bool Inverted
    {
        get => this.inverted;
        set
        {
            if (this.inverted == value) return;
            this.inverted = value;
            this.FreezeEditedState = true;
            foreach (var index in this.magnitudeColumns)
            {
                this[index] = -this[index];
                var cell = (DataGridViewNumericBoxCell)this.Cells[index + 1];
                cell.Invert = this.inverted;
            }
            this.FreezeEditedState = false;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the parameters are edited.
    /// </summary>
    internal bool Edited
    {
        get
        {
            foreach (var cell in this.Cells)
                if (cell is DataGridViewNumericBoxCell numericBoxCell && numericBoxCell.Edited)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the edited state is frozen.
    /// </summary>
    internal bool FreezeEditedState
    {
        get
        {
            foreach (var cell in this.Cells)
                if (cell is DataGridViewNumericBoxCell numericBoxCell && numericBoxCell.FreezeEditedState)
                    return true;
            return false;
        }
        set
        {
            foreach (var cell in this.Cells)
                if (cell is DataGridViewNumericBoxCell numericBoxCell)
                    numericBoxCell.FreezeEditedState = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the ParametersTableRow class with the specified decay parameter.
    /// </summary>
    /// <param name="decay">The decay parameter to associate with this table row.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="decay"/> parameter is <see langword="null"/>.</exception>
    internal ParametersTableRow(Decay decay)
    {
        ArgumentNullException.ThrowIfNull(decay);
        this.Decay = decay;
    } // ctor (decay)

    /// <summary>
    /// Sets the values of the parameters from the specified read-only span of doubles.
    /// </summary>
    /// <param name="parameters">A read-only span containing the parameter values to set. The length of the span must match the number of parameters.</param>
    internal void SetParameters(ReadOnlySpan<double> parameters)
    {
        for (var i = 0; i < parameters.Length; i++)
            this[i] = parameters[i];
    } // internal void SetParameters (ReadOnlySpan<double>)

    /// <summary>
    /// Sets the magnitude columns.
    /// </summary>
    /// <param name="indices">The indices of the magnitude columns.</param>
    internal void SetMagnitudeColumns(IEnumerable<int> indices)
        => this.magnitudeColumns = [.. indices];

    /// <summary>
    /// Inverts the magnitude columns.
    /// </summary>
    internal void InvertMagnitude()
        => this.Inverted = !this.Inverted;

    private static Color GetRSquaredColor(double value)
        => ColorTable.GetRSquaredColor(value);

    private class RSquaredColorTable
    {
        private readonly double[] thresholds;
        private readonly Color[] colors;

        internal RSquaredColorTable(IReadOnlyList<RSquaredThresholdItem> items)
        {
            this.thresholds = new double[items.Count];
            this.colors = new Color[items.Count];

            for (var i = 0; i < items.Count; i++)
            {
                this.thresholds[i] = items[i].Threshold;
                this.colors[i] = items[i].Color;
            }

            Array.Sort(this.thresholds, this.colors);
        } // ctor (IReadOnlyList<RSquaredThresholdItem>)

        internal Color GetRSquaredColor(double value)
        {
            for (var i = this.thresholds.Length; i > 0; i--)
            {
                if (value >= this.thresholds[i - 1])
                    return this.colors[i - 1];
            }
            return this.colors[0];
        } // internal static Color GetRSquaredColor (double value)
    } // private class RSquaredColorTable
} // internal sealed partial class ParametersTableRow : DataGridViewRow

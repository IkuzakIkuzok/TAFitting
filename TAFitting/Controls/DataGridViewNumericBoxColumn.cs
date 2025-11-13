
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;


/// <summary>
/// Represents a column that contains <see cref="DataGridViewNumericBoxCell"/> objects.
/// </summary>
internal partial class DataGridViewNumericBoxColumn : DataGridViewColumn
{
    private bool isFixed = false;

    /// <summary>
    /// Gets or sets the bias of the digit order for incrementing.
    /// </summary>
    internal double IncrementOrderBias
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate!).IncrementOrderBias;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate!).IncrementOrderBias = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.IncrementOrderBias = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum value of the cells in the column.
    /// </summary>
    internal double Maximum
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate!).Maximum;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate!).Maximum = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.Maximum = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum value of the cells in the column.
    /// </summary>
    internal double Minimum
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate!).Minimum;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate!).Minimum = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.Minimum = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the decimal places of the cells in the column.
    /// </summary>
    internal int DecimalPlaces
    {
        get => ((DataGridViewNumericBoxCell)this.CellTemplate!).DecimalPlaces;
        set
        {
            ((DataGridViewNumericBoxCell)this.CellTemplate!).DecimalPlaces = value;
            var dgw = this.DataGridView;
            if (dgw is not null)
            {
                for (var i = 0; i < dgw.RowCount; i++)
                {
                    var cell = (DataGridViewNumericBoxCell)dgw[this.Index, i];
                    cell.DecimalPlaces = value;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the values of the cells in the column are fixed.
    /// </summary>
    internal bool Fixed
    {
        get => this.isFixed;
        set
        {
            if (this.isFixed == value) return;
            this.isFixed = value;
            if (value)
                this.HeaderText += "*";
            else
                this.HeaderText = this.HeaderText[..^1];  // Do not use this.HeaderText.TrimEnd('*') since the header text may contain '*'.
            this.DataGridView?.Refresh();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridViewNumericBoxColumn"/> class.
    /// </summary>
    internal DataGridViewNumericBoxColumn() : this(0.0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridViewNumericBoxColumn"/> class
    /// with the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    internal DataGridViewNumericBoxColumn(double defaultValue)
    {
        this.CellTemplate = new DataGridViewNumericBoxCell(defaultValue);
        this.SortMode = DataGridViewColumnSortMode.Automatic;
    } // ctor (double)
} // internal partial class DataGridViewNumericBoxColumn : DataGridViewColumn


// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Displays an editable numeric value in a <see cref="DataGridView"/> control.
/// </summary>
internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell
{
    /// <summary>
    /// Gets or sets the bias of the digit order for incrementing.
    /// </summary>
    internal double IncrementOrderBias { get; set; } = -1;

    /// <summary>
    /// Gets or sets the maximum value of the cell.
    /// </summary>
    internal double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets the minimum value of the cell.
    /// </summary>
    internal double Minimum { get; set; } = double.MinValue;

    /// <summary>
    /// Gets or sets the decimal places.
    /// </summary>
    internal int DecimalPlaces
    {
        get => int.Parse(this.Style.Format[1..]);
        set => this.Style.Format = $"N{value}";
    }

    /// <summary>
    /// Gets or sets the maximum significant digit.
    /// </summary>
    internal int Digit { get; set; } = 6;

    /// <summary>
    /// Gets or sets a value indicating whether the increment and decrement are inverted.
    /// </summary>
    internal bool Invert { get; set; } = false;

    /// <summary>
    /// Gets a value indicating whether the cell is edited.
    /// </summary>
    internal bool Edited { get; private set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the edited state is frozen.
    /// </summary>
    internal bool FreezeEditedState { get; set; } = false;

    protected double defaultValue = 0.0;

    /// <summary>
    /// Gets the default value of the cell.
    /// </summary>
    internal double DefaultValue
        => this.OwningColumn?.CellTemplate is DataGridViewNumericBoxCell cell ? cell.defaultValue : this.defaultValue;

    /// <summary>
    /// Initializes a new instance of the DataGridViewNumericBoxCell class.
    /// </summary>
    public DataGridViewNumericBoxCell()
    {
        this.Style.Format = "N2";
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the DataGridViewNumericBoxCell class
    /// with the specified default value.
    /// </summary>
    /// <param name="defaultValue">The default value of the cell.</param>
    public DataGridViewNumericBoxCell(double defaultValue) : this()
    {
        this.defaultValue = defaultValue;
    } // ctor (double)

    /// <inheritdoc/>
    override public Type ValueType => typeof(double);

    /// <inheritdoc/>
    override public object DefaultNewRowValue => this.DefaultValue;

    /// <inheritdoc/>
    override protected object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
    {
        if (value is double d)
            return d.ToString("N2");
        return base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
    } // override protected object GetFormattedValue (object, int, ref DataGridViewCellStyle, TypeConverter, TypeConverter, DataGridViewDataErrorContexts)

    /// <inheritdoc/>
    override protected bool SetValue(int rowIndex, object value)
    {
        if (!this.FreezeEditedState) this.Edited = true;
        if (value is string s && double.TryParse(s, out var d))
        {
            d = Math.Max(this.Minimum, Math.Min(this.Maximum, d));
            return base.SetValue(rowIndex, d);
        }
        return base.SetValue(rowIndex, value);
    } // override protected bool SetValue (int, object)

    /// <inheritdoc/>
    protected double GetDoubleValue(int rowIndex)
        => Math.Round((double)GetValue(rowIndex), this.Digit);

    /// <inheritdoc/>
    override protected void OnKeyDown(KeyEventArgs e, int rowIndex)
    {
        if (e.Alt && this.DataGridView is not null)
        {
            if (e.KeyCode is Keys.Up or Keys.Down)
            {
                HandleUpDown(e, rowIndex);
                e.Handled = true;
                return;
            }
        }

        base.OnKeyDown(e, rowIndex);
    } // override protected void OnKeyDown (KeyEventArgs, int)

    /// <summary>
    /// Handles the up and down key events.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    /// <param name="rowIndex">The row index.</param>
    private void HandleUpDown(KeyEventArgs e, int rowIndex)
    {
        /*
         *        | Invert
         *    up  | T   F
         * -------+-------- ==> up = upKey ^ Invert
         * upKey T| F   T
         *       F| T   F
         */
        var upKey = e.KeyCode == Keys.Up;
        var up = upKey ^ this.Invert;
        var step = upKey ? CalcIncrement() : CalcDecrement();

        var additionalBias = e.Shift ? 10 : 1;

        if (up)
        {
            var value = GetDoubleValue(rowIndex);
            var increment = step * additionalBias;
            if (increment == 0) increment = 1;
            SetValue(rowIndex, Math.Floor(Math.Round(value / increment, this.Digit)) * increment + increment);
            e.Handled = true;
        }
        else
        {
            var value = GetDoubleValue(rowIndex);
            var decrement = step * additionalBias;
            if (decrement == 0) decrement = 1;
            var newValue = Math.Ceiling(Math.Round(value / decrement, this.Digit)) * decrement - decrement;
            SetValue(rowIndex, newValue);
            e.Handled = true;
        }
    } // private void HandleUpDown (KeyEventArgs, int)

    /// <summary>
    /// Gets the logarithmic value of the cell.
    /// </summary>
    /// <returns>The logarithmic value.</returns>
    protected virtual double GetLogValue()
    {
        var val = Math.Abs((double)GetValue(this.RowIndex));
        if (val == 0) return 0;
        return Math.Log10(val);
    } // protected virtual double GetLogValue ()

    /// <summary>
    /// Calculates the increment value.
    /// </summary>
    /// <returns>The increment value.</returns>
    protected virtual double CalcIncrement()
    {
        var order = Math.Floor(GetLogValue()) + this.IncrementOrderBias;
        return Math.Pow(10, order);
    } // protected virtual double CalcIncrement ()

    /// <summary>
    /// Calculates the decrement value.
    /// </summary>
    /// <returns>The decrement value.</returns>
    protected virtual double CalcDecrement()
    {
        var log = GetLogValue();
        var order = Math.Floor(log) + this.IncrementOrderBias;
        return log % 1 == 0 ? Math.Pow(10, order - 1) : Math.Pow(10, order);
    } // protected virtual double CalcDecrement ()
} // internal class DataGridViewNumericBoxCell : DataGridViewTextBoxCell

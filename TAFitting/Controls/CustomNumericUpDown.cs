﻿
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Represents a customizable numeric up-down control.
/// </summary>
[DesignerCategory("Code")]
internal sealed class CustomNumericUpDown : NumericUpDown
{
    private decimal _abs_increment, _increment;
    private decimal _abs_scroll_increment, _scroll_increment;

    #region properties

    /// <summary>
    /// Gets or sets the value to increment or decrement the spin box (also known as an up-down control) when the up or down buttons are clicked.
    /// </summary>
    new internal decimal Increment
    {
        set
        {
            this._increment = value;
            this._abs_increment = Math.Abs(value);
        }
        get => this._increment;
    }

    /// <summary>
    /// Gets or sets the value to increment or decrement the spin box (also known as an up-down control) when th mousee wheel spined.
    /// </summary>
    internal decimal ScrollIncrement
    {
        set
        {
            this._scroll_increment = value;
            this._abs_scroll_increment = Math.Abs(value);
        }
        get => this._scroll_increment;
    }

    #endregion properties

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomNumericUpDown"/> class.
    /// </summary>
    internal CustomNumericUpDown() : base()
    {
        this.Increment = 1;
        this.ScrollIncrement = 1;
        this.ImeMode = ImeMode.Disable;
    } // ctor ()

    new internal void UpButton() => UpDown(this._increment > 0);

    new internal void DownButton() => UpDown(this._increment < 0);

    private void UpDown(bool up)
    {
        try
        {
            this.Value = up
                ? Math.Min(checked(this.Value + this._abs_increment), this.Maximum) // increment
                : Math.Max(checked(this.Value - this._abs_increment), this.Minimum) // decrement
                ;
        }
        catch (OverflowException)
        {
            this.Value = up ? this.Maximum : this.Minimum;
        }
    } // private void UpDown (bool)

    override protected void OnMouseWheel(MouseEventArgs e)
    {
        if (e is HandledMouseEventArgs hme) hme.Handled = true;

        var up = e.Delta > 0 ^ this._scroll_increment > 0;
        try
        {
            this.Value = up
                ? Math.Max(checked(this.Value - this._abs_scroll_increment), this.Minimum) // decrement
                : Math.Min(checked(this.Value + this._abs_scroll_increment), this.Maximum) // increment
                ;
        }
        catch (OverflowException)
        {
            this.Value = up ? this.Maximum : this.Minimum;
        }
    } // override protected void OnMouseWheel (MouseEventArgs)

    override protected void OnGotFocus(EventArgs e)
    {
        NegativeSignHandler.ChangeNegativeSign("-");
        this.Text = this.Text.Replace("\u2212", "-");
        base.OnGotFocus(e);
    } // override protected void OnGotFocus (EventArgs)

    override protected void OnLostFocus(EventArgs e)
    {
        NegativeSignHandler.ChangeNegativeSign("\u2212");
        this.Text = this.Text.Replace("-", "\u2212");
        base.OnLostFocus(e);
    } // override protected void OnLostFocus (EventArgs)
} // internal sealed class CustomNumericUpDown : NumericUpDown

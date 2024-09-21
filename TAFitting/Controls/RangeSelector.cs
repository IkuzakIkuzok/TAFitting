
// (c) 2024 Kazuki Kohzuki

using Formatter = System.Func<decimal, string>;

namespace TAFitting.Controls;

/// <summary>
/// Represents a range selector.
/// </summary>
internal class RangeSelector
{
    private readonly Label lb_main, lb_from, lb_to;
    private readonly NumericUpDown nud_from, nud_to;
    private readonly LogarithmicNumericUpDown nud_log_from, nud_log_to;
    private readonly CheckBox cb_log;

    /// <summary>
    /// Gets or sets the text of the range selector.
    /// </summary>
    internal string Text
    {
        get => this.lb_main.Text;
        set => this.lb_main.Text = value;
    }

    /// <summary>
    /// Gets or sets the parent control.
    /// </summary>
    internal Control? Parent
    {
        get => this.lb_main.Parent;
        set
        {
            // `from` control must be added to parent control before `to` control
            // in order to set its tab index to be less than that of `to` control
            this.lb_main.Parent = this.lb_to.Parent = this.lb_from.Parent = value;
            this.nud_from.Parent = this.nud_to.Parent = value;
            this.nud_log_to.Parent = this.nud_log_from.Parent = value;
            this.cb_log.Parent = value;
        }
    }

    /// <summary>
    /// Gets or sets the top position.
    /// </summary>
    internal int Top
    {
        get => this.lb_main.Top;
        set
        {
            this.lb_to.Top = this.lb_from.Top = this.lb_main.Top = value;
            this.nud_from.Top = this.nud_to.Top = value - 2;
            this.nud_log_to.Top = this.nud_log_from.Top = value - 2;
            this.cb_log.Top = value - 2;
        }
    }

    /// <summary>
    /// Gets or sets the left position.
    /// </summary>
    internal int Left
    {
        get => this.lb_main.Left;
        set
        {
            this.lb_main.Left = value;
            this.lb_from.Left = value + 60;
            this.lb_to.Left = value + 190;

            this.nud_from.Left = this.nud_log_from.Left = value + 100;
            this.nud_to.Left = this.nud_log_to.Left = value + 210;
            this.cb_log.Left = value + 300;
        }
    }

    /// <summary>
    /// Gets or sets the start value of the range.
    /// </summary>
    internal decimal From
    {
        get => this.nud_from.Value;
        set => this.nud_from.Value = this.nud_log_from.Value = value;
    }

    /// <summary>
    /// Gets or sets the end value of the range.
    /// </summary>
    internal decimal To
    {
        get => this.nud_to.Value;
        set => this.nud_to.Value = this.nud_log_to.Value = value;
    }

    /// <summary>
    /// Gets or sets the decimal places of the start value.
    /// </summary>
    internal int FromDecimalPlaces
    {
        get => this.nud_from.DecimalPlaces;
        set => this.nud_from.DecimalPlaces = this.nud_log_from.DecimalPlaces = value;
    }

    /// <summary>
    /// Gets or sets the decimal places of the end value.
    /// </summary>
    internal int ToDecimalPlaces
    {
        get => this.nud_to.DecimalPlaces;
        set => this.nud_to.DecimalPlaces = this.nud_log_to.DecimalPlaces = value;
    }

    /// <summary>
    /// Gets or sets the minimum value of the start value.
    /// </summary>
    internal decimal FromMinimum
    {
        get => this.nud_from.Minimum;
        set => this.nud_from.Minimum = this.nud_log_from.Minimum = value;
    }

    /// <summary>
    /// Gets or sets the maximum value of the start value.
    /// </summary>
    internal decimal FromMaximum
    {
        get => this.nud_from.Maximum;
        set => this.nud_from.Maximum = this.nud_log_from.Maximum = value;
    }

    /// <summary>
    /// Gets or sets the minimum value of the end value.
    /// </summary>
    internal decimal ToMinimum
    {
        get => this.nud_to.Minimum;
        set => this.nud_to.Minimum = this.nud_log_to.Minimum = value;
    }

    /// <summary>
    /// Gets or sets the maximum value of the end value.
    /// </summary>
    internal decimal ToMaximum
    {
        get => this.nud_to.Maximum;
        set => this.nud_to.Maximum = this.nud_log_to.Maximum = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the range is logarithmic.
    /// </summary>
    internal bool Logarithmic
    {
        get => this.cb_log.Checked;
        set => this.cb_log.Checked = value;
    }

    /// <summary>
    /// Gets or sets the formatter for the values.
    /// </summary>
    internal Formatter? Formatter
    {
        get => this.nud_log_from.Formatter;
        set
        {
            this.nud_log_from.Formatter = value;
            this.nud_log_to.Formatter = value;
        }
    }

    /// <summary>
    /// Occurs when the start value changes.
    /// </summary>
    internal event EventHandler? FromChanged;

    /// <summary>
    /// Occurs when the end value changes.
    /// </summary>
    internal event EventHandler? ToChanged;
    
    /// <summary>
    /// Occurs when the logarithmic state changes.
    /// </summary>
    internal event EventHandler? LogarithmicChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeSelector"/> class.
    /// </summary>
    internal RangeSelector()
    {
        this.lb_main = new()
        {
            Size = new(60, 20),
        };

        this.lb_from = new()
        {
            Text = "From",
            Size = new(40, 20),
        };

        this.lb_to = new()
        {
            Text = "To",
            Size = new(20, 20),
        };

        this.nud_from = new()
        {
            Size = new(80, 20),
        };

        this.nud_to = new()
        {
            Size = new(80, 20),
        };

        this.nud_log_from = new()
        {
            Size = new(80, 20),
            Visible = false,
            Enabled = false,
        };

        this.nud_log_to = new()
        {
            Size = new(80, 20),
            Visible = false,
            Enabled = false,
        };
        
        this.nud_to.ValueChanged += (s, e) =>
        {
            this.nud_log_to.Value = this.nud_to.Value;
            OnToChanged(e);
        };
        this.nud_from.ValueChanged += (s, e) =>
        {
            this.nud_log_from.Value = this.nud_from.Value;
            OnFromChanged(e);
        };

        this.nud_log_from.ValueChanged += (s, e) =>
        {
            this.nud_from.Value = this.nud_log_from.Value;
            OnFromChanged(e);
        };
        this.nud_log_to.ValueChanged += (s, e) =>
        {
            this.nud_to.Value = this.nud_log_to.Value;
            OnToChanged(e);
        };

        this.cb_log = new()
        {
            Text = "Logarithmic",
            Size = new(100, 20),
        };
        this.cb_log.CheckedChanged += ChangeLogarithmic;
    } // ctor ()

    private void ChangeLogarithmic(object? sender, EventArgs e)
    {
        if (this.cb_log.Checked)
        {
            this.nud_log_from.Visible = this.nud_log_to.Visible = true;
            this.nud_from.Visible = this.nud_to.Visible = false;

            this.nud_log_from.Enabled = this.nud_log_to.Enabled = true;
            this.nud_from.Enabled = this.nud_to.Enabled = false;
        }
        else
        {
            this.nud_log_from.Visible = this.nud_log_to.Visible = false;
            this.nud_from.Visible = this.nud_to.Visible = true;

            this.nud_log_from.Enabled = this.nud_log_to.Enabled = false;
            this.nud_from.Enabled = this.nud_to.Enabled = true;
        }

        LogarithmicChanged?.Invoke(this, e);
    } // private void ChangeLogarithmic (object?, EventArgs)

    /// <summary>
    /// Raises the <see cref="FromChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnFromChanged(EventArgs e)
        => FromChanged?.Invoke(this, e);

    /// <summary>
    /// Raises the <see cref="ToChanged"/> event.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnToChanged(EventArgs e)
        => ToChanged?.Invoke(this, e);
} // internal class RangeSelector

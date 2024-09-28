
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

/// <summary>
/// Represents a numeric input box.
/// </summary>
[DesignerCategory("Code")]
internal sealed class NumericInputBox : Form
{
    private readonly LogarithmicNumericUpDown _numericUpDown;

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    internal decimal Value
    {
        get => this._numericUpDown.Value;
        set => this._numericUpDown.Value = value;
    }

    /// <summary>
    /// Gets or sets the minimum value.
    /// </summary>
    internal decimal Minimum
    {
        get => this._numericUpDown.Minimum;
        set => this._numericUpDown.Minimum = value;
    }

    /// <summary>
    /// Gets or sets the maximum value.
    /// </summary>
    internal decimal Maximum
    {
        get => this._numericUpDown.Maximum;
        set => this._numericUpDown.Maximum = value;
    }

    /// <summary>
    /// Gets or sets the decimal places.
    /// </summary>
    internal int DecimalPlaces
    {
        get => this._numericUpDown.DecimalPlaces;
        set => this._numericUpDown.DecimalPlaces = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NumericInputBox"/> class.
    /// </summary>
    internal NumericInputBox()
    {
        this.Size = this.MinimumSize = this.MaximumSize = new(200, 120);
        this.MinimizeBox = this.MaximizeBox = false;
        this.SizeGripStyle = SizeGripStyle.Hide;

        this._numericUpDown = new()
        {
            Location = new(10, 10),
            Width = 165,
            Parent = this,
        };

        this.AcceptButton = new Button()
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new(10, 40),
            Size = new(80, 25),
            Parent = this,
        };

        this.CancelButton = new Button()
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new(95, 40),
            Size = new(80, 25),
            Parent = this,
        };
    } // ctor ()
} // internal sealed class NumericInputBox : Form

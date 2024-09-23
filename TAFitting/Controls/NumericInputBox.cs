
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting.Controls;

[DesignerCategory("Code")]
internal sealed class NumericInputBox : Form
{
    private readonly LogarithmicNumericUpDown _numericUpDown;

    internal decimal Value
    {
        get => this._numericUpDown.Value;
        set => this._numericUpDown.Value = value;
    }

    internal decimal Minimum
    {
        get => this._numericUpDown.Minimum;
        set => this._numericUpDown.Minimum = value;
    }

    internal decimal Maximum
    {
        get => this._numericUpDown.Maximum;
        set => this._numericUpDown.Maximum = value;
    }

    internal int DecimalPlaces
    {
        get => this._numericUpDown.DecimalPlaces;
        set => this._numericUpDown.DecimalPlaces = value;
    }

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

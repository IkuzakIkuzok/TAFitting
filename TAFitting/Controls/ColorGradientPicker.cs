

// (c) 2024 Kazuki Kohzuki

using LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode;

namespace TAFitting.Controls;

/// <summary>
/// Represents a color gradient picker.
/// </summary>
[DesignerCategory("Code")]
internal class ColorGradientPicker : Form
{
    protected readonly ColorGradient colorGradient;

    protected readonly ColorButton start, end;

    protected readonly Label lb_gradient;

    /// <summary>
    /// Gets the start color.
    /// </summary>
    internal Color StartColor
        => this.colorGradient.StartColor;

    /// <summary>
    /// Gets the end color.
    /// </summary>
    internal Color EndColor
        => this.colorGradient.EndColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorGradientPicker"/> class.
    /// </summary>
    /// <param name="startColor">The default start color.</param>
    /// <param name="endColor">The default end color.</param>
    internal ColorGradientPicker(Color startColor, Color endColor)
    {
        this.Text = "Color gradient";
        this.Size = this.MinimumSize = this.MaximumSize = new(330, 150);
        this.MaximizeBox = false;

        this.colorGradient = new(startColor, endColor);

        this.start = new()
        {
            Top = 20,
            Left = 20,
            Width = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Popup,
            Parent = this,
        };
        this.start.Click += SelectStartColor;

        this.end = new()
        {
            Top = 20,
            Left = 220,
            Width = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            FlatStyle = FlatStyle.Popup,
            Parent = this,
        };
        this.end.Click += SelectEndColor;

        this.lb_gradient = new()
        {
            Top = 20,
            Left = 110,
            Width = 100,
            Parent = this,
        };

        this.AcceptButton = new Button()
        {
            Text = "OK",
            Top = 60,
            Left = 120,
            Size = new(80, 30),
            DialogResult = DialogResult.OK,
            Parent = this,
        };

        this.CancelButton = new Button()
        {
            Text = "Cancel",
            Top = 60,
            Left = 220,
            Size = new(80, 30),
            DialogResult = DialogResult.Cancel,
            Parent = this,
        };

        SetColor();
    } // ctor (Color, Color)

    private static Color SelectColor(Color color)
    {
        using var cd = new ColorDialog()
        {
            Color = color,
        };
        return cd.ShowDialog() == DialogResult.OK ? cd.Color : color;
    } // private static Color SelectColor (Color)

    private void SelectStartColor(object? sender, EventArgs e)
    {
        this.colorGradient.StartColor = SelectColor(this.colorGradient.StartColor);
        SetColor();
    } // private void SelectStartColor (object?, EventArgs)

    private void SelectEndColor(object? sender, EventArgs e)
    {
        this.colorGradient.EndColor = SelectColor(this.colorGradient.EndColor);
        SetColor();
    } // private void SelectEndColor (object?, EventArgs)

    protected virtual void SetColor()
    {
        this.start.Color = this.colorGradient.StartColor;
        this.end.Color = this.colorGradient.EndColor;
        this.colorGradient.FillRectangle(this.lb_gradient, LinearGradientMode.Horizontal);
    } // protected virtual void SetColor ()
} // internal class ColorGradientPicker

﻿
// (c) 2024 Kazuki KOHZUKI

using Timer = System.Windows.Forms.Timer;

namespace TAFitting.Controls;

/// <summary>
/// Displays a message box that fades out without blocking the main thread.
/// </summary>
[DesignerCategory("Code")]
internal partial class FadingMessageBox : Form
{
    private static FadingMessageBox? showing = null;

    private readonly Label label;
    private Timer? timer;
    private bool flag = false;
    private readonly Form parent;

    private int initialInterval;
    private int fadingInterval;
    private double fadeRate;

    override protected CreateParams CreateParams
    {
        get
        {
            const int WS_EX_TRANSPARENT = 0x20;
            var parms = base.CreateParams;
            parms.ExStyle |= WS_EX_TRANSPARENT;
            return parms;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FadingMessageBox"/> class.
    /// </summary>
    /// <param name="parent">The parent form.</param>
    private FadingMessageBox(Form parent)
    {
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.FormBorderStyle = FormBorderStyle.None;
        this.StartPosition = FormStartPosition.Manual;
        this.Top = parent.Top + (parent.Height - this.Height) / 2;
        this.Left = parent.Left + (parent.Width - this.Width) / 2;
        this.parent = parent;
        parent.FormClosed += OnParentClosed;

        this.label = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Gray,
            ForeColor = Color.White,
            Font = new Font(this.Font.Name, 22),
            TextAlign = ContentAlignment.MiddleCenter,
            Parent = this,
        };
    } // ctor (Form)

    /// <summary>
    /// Displays a message box that fades out without blocking the main thread.
    /// </summary>
    /// <param name="text">A <see cref="string"/> that specifies the text to display.</param>
    /// <param name="initialOpacity">The initial opacity of the message box.</param>
    /// <param name="initInterval">Time to maintain a initial opacity after the message box is displayed.</param>
    /// <param name="fadeInterval">Fading interval.</param>
    /// <param name="fadeRate">The fading rate for each <paramref name="fadeInterval"/>.</param>
    /// <param name="parentControl">The parent form of the message box.</param>
    /// <param name="width">The width of the message box.</param>
    internal static void Show(
        string text,
        double initialOpacity = 0.8,
        int initInterval = 2000,
        int fadeInterval = 75,
        double fadeRate = 0.05,
        Form? parentControl = null,
        int width = 500
    )
    {
        var form = new FadingMessageBox(parentControl ?? Program.MainWindow)
        {
            Opacity = initialOpacity,
        };
        form.label.Text = text;
        var height = CalcHeight(text, width, form.label.Font);
        form.Size = form.MinimumSize = form.MaximumSize = new Size(width, height + 20);

        form.initialInterval = initInterval;
        form.fadingInterval = fadeInterval;
        form.fadeRate = fadeRate;

        showing?.Close();

        form.Show();
    } // internal static void Show (string, [double], [int], [int], [double], [Form])

    private static int CalcHeight(string text, int width, Font font)
        => TextRenderer.MeasureText(text, font, new Size(width, int.MaxValue), TextFormatFlags.WordBreak).Height;

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        this.parent.FormClosed -= OnParentClosed;
        this.timer?.Dispose();

        showing = null;
    } // protected override void OnClosed (EventArgs)

    override protected void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        showing = this;

        this.timer = new Timer()
        {
            Interval = this.initialInterval,
            Enabled = true,
        };
        this.timer.Tick += OnTimerTick;
    } // override protected void OnLoad (EventArgs)

    virtual protected void OnTimerTick(object? sender, EventArgs e)
    {
        if (!this.flag && this.timer is not null)
        {
            this.timer.Interval = this.fadingInterval;
            this.flag = true;
        }
        else
        {
            if (this.Opacity >= this.fadeRate)
                this.Opacity -= this.fadeRate;
            else
                Close();
        }
    } // virtual protected void OnTimerTick (object?, EventArgs)

    private void OnParentClosed(object? sender, EventArgs e)
        => Close();
} // internal partial class FadingMessageBox : Form

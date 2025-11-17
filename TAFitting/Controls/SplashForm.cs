
// (c) 2025 Kazuki KOHZUKI

using System.Drawing.Text;
using System.Reflection;
using TAFitting.Properties;

namespace TAFitting.Controls;

/// <summary>
/// Represents a modal splash screen form displayed during application startup to indicate loading progress.
/// </summary>
/// <remarks>This form is intended for internal use to provide a visual cue while the main application is
/// initializing. It is displayed centered on the screen without window borders or a taskbar entry, and is automatically
/// closed when the application becomes idle.</remarks>
[DesignerCategory("code")]
internal sealed class SplashForm : Form
{
    private static SplashForm? _instance;

    private SplashForm()
    {
        this.TopMost = true;
        this.Size = this.MinimumSize = this.MaximumSize = new(300, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.None;
        this.ShowInTaskbar = false;
        this.BackgroundImage = Resources.SplashImage;

    } // ctor ()

    override protected void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Load the embedded subset font from resources
        var fontBuf = Resources.SegoeUISubset;
        using var pfc = new PrivateFontCollection();
        unsafe
        {
            fixed (byte* p = fontBuf)
            {
                pfc.AddMemoryFont((IntPtr)p, fontBuf.Length);
            }
        }

        var version = $"TA Fitting v{Assembly.GetExecutingAssembly().GetName().Version}";
        using var font = new Font(pfc.Families[0], 16);
        e.Graphics.DrawString(
            version,
            font,
            Brushes.White,
            new RectangleF(10, this.Height - 40, this.Width - 10, 40)
        );
    } // override protected void OnPaint (PaintEventArgs)

    /// <summary>
    /// Displays the application's splash screen if it is not already visible.
    /// </summary>
    /// <remarks>This method is intended to be called at application startup to show a splash screen during
    /// initialization. If the splash screen is already displayed, calling this method has no effect.</remarks>
    internal static void ShowSplash()
    {
        if (_instance is not null) return;

        _instance = new();
        _instance.Show();
        Application.DoEvents();
        Application.Idle += CloseSplash;
    } // internal static void ShowSplash ()

    private static void CloseSplash(object? sender, EventArgs e)
    {
        if (_instance is null) return;
        _instance.Close();
        _instance.Dispose();
        _instance = null;
        Application.Idle -= CloseSplash;
    } // private static void CloseSplash (object?, EventArgs)
} // internal sealed class SplashForm : Form


// (c) 2024 Kazuki KOHZUKI

using static TAFitting.Win32;

namespace TAFitting.Controls;

/// <summary>
/// Suspends drawing of the specified control.
/// </summary>
internal sealed partial class ControlDrawingSuspender : IDisposable
{
    private readonly Control _control;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlDrawingSuspender"/> class.
    /// </summary>
    /// <param name="control">The control.</param>
    internal ControlDrawingSuspender(Control control)
    {
        this._control = control;
        StopPainting(control);
    } // ctor (Control)

    /// <summary>
    /// Stops painting of the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    internal static void StopPainting(Control control)
    {
        SendMessage(new HandleRef(control, control.Handle), WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
    } // internal static void StopPainting (Control)

    /// <summary>
    /// Resumes painting of the specified control.
    /// </summary>
    /// <param name="control">The control.</param>
    internal static void ResumePainting(Control control)
    {
        SendMessage(new HandleRef(control, control.Handle), WM_SETREDRAW, 1, IntPtr.Zero);
        control.Refresh();
    } // internal static void ResumePainting (Control)

    public void Dispose()
    {
        Dispose(true);
    } // public void Dispose ()

    private void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                ResumePainting(this._control);
            }
            this._disposed = true;
        }
    } // private void Dispose (bool)
} // internal sealed partial class ControlDrawingSuspender : IDisposable

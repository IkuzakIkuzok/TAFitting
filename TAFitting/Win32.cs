
// (c) 2024 Kazuki KOHZUKI

namespace TAFitting;

/// <summary>
/// Provides Win32 API functions and constants.
/// </summary>
internal static class Win32
{
    internal const int WM_SETREDRAW = 0x000B;
    internal const int HWND_BROADCAST = 0xffff;

    /// <summary>
    /// Sends the specified message to a window or windows.
    /// The <see cref="SendMessage"/> function calls the window procedure for the specified window
    /// and does not return until the window procedure has processed the message.
    /// </summary>
    /// <param name="hWnd">A handle to the window whose window procedure will receive the message.
    /// If this parameter is <see cref="HWND_BROADCAST"/> ((HWND)0xffff), the message is sent to all top-level windows in the system,
    /// including disabled or invisible unowned windows, overlapped windows, and pop-up windows;
    /// but the message is not sent to child windows.</param>
    /// <param name="msg">The message to be sent.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>The return value specifies the result of the message processing; it depends on the message sent.</returns>
    [DllImport("user32.dll")]
    internal static extern IntPtr SendMessage(HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
} // internal static class Win32

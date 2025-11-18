
// (c) 2024 Kazuki Kohzuki

namespace TAFitting.Controls.Toast;

/// <summary>
/// A wrapper class for toast notification arguments.
/// </summary>
/// <remarks>
/// The <see cref="ToastNotification"/> class manages elements of toast notifications with the unique ID.
/// The ID has some key-value pairs, each of them is separated by the ampersand (&amp;), and they are separated by the equal sign (=).
/// This class wraps the key-value pairs as a dictionary, for better handling.
/// </remarks>
internal sealed partial class ToastArgument : Dictionary<string, string?>
{
    /// <summary>
    /// Gets or sets the ID of the toast notification, i.e., the root element of the notification.
    /// </summary>
    internal string? ToastId
    {
        get
        {
            if (TryGetValue("toastId", out var id)) return id;
            return default;
        }
        set => this["toastId"] = value;
    }

    public static explicit operator ToastArgument(string arg)
    {
        var ta = new ToastArgument();

        var pairs = arg.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var data = pair.Split('=');
            ta.Add(data[0], data[1]);
        }

        return ta;
    } // public static explicit operator ToastArgument (string)
} // internal sealed partial class ToastArgument : Dictionary<string, string?>

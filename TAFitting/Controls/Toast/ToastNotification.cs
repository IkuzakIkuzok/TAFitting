
// (c) 2024 Kazuki Kohzuki

using Microsoft.Toolkit.Uwp.Notifications;
using ToastCallback = System.Action<System.Collections.Generic.IDictionary<string, object>>;

namespace TAFitting.Controls.Toast;

/// <summary>
/// A wrapper class for toast notifications.
/// </summary>
internal class ToastNotification
{
    protected static int id = 0;

    internal int Id { get; }

    protected ToastContentBuilder contentBuilderInternal = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotification"/> class.
    /// </summary>
    internal ToastNotification()
    {
        this.Id = id++;
    } // ctor ()

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotification"/> class with the specified message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    internal ToastNotification(string message) : this(message, Program.AppName) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotification"/> class with the specified message and title.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title of the notification.</param>
    internal ToastNotification(string message, string title) : this()
    {
        AddText(title);
        AddText(message);
    } // ctor (string, string)

    /// <summary>
    /// Shows the toast notification.
    /// </summary>
    internal void Show()
        => this.contentBuilderInternal.Show();

    /// <summary>
    /// Shows the toast notification with the specified message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    internal static void Show(string message)
        => new ToastNotification(message)
        .AddButton("OK", null)
        .Show();

    /// <summary>
    /// Shows the toast notification with the specified message and title.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="title">The title of the notification.</param>
    internal static void Show(string message, string title)
        => new ToastNotification(message, title)
        .AddButton("OK", null)
        .Show();

    /// <summary>
    /// Adds a text element to the toast notification.
    /// </summary>
    /// <param name="text">The title of the notification.</param>
    /// <returns>The current instance of the <see cref="ToastNotification"/> class.</returns>
    internal ToastNotification AddText(string text)
    {
        this.contentBuilderInternal.AddText(text);
        return this;
    } // internal ToastNotification AddText (string)

    /// <summary>
    /// Adds a button to the toast notification.
    /// </summary>
    /// <param name="text">The text of the button.</param>
    /// <returns>The current instance of the <see cref="ToastNotification"/> class.</returns>
    internal ToastNotification AddButton(string text)
        => AddButton(text, null);

    /// <summary>
    /// Adds a button to the toast notification.
    /// </summary>
    /// <param name="text">The text of the button.</param>
    /// <param name="callback">The callback to invoke when the button is clicked.</param>
    /// <returns>The current instance of the <see cref="ToastNotification"/> class.</returns>
    internal ToastNotification AddButton(string text, ToastCallback? callback)
    {
        var buttonId = $"toastId={this.Id}&buttonId={id++}";
        if (callback != null)
            ToastNotificationCallbackManager.AddCallback(buttonId, callback);
        this.contentBuilderInternal.AddButton(new ToastButton(text, buttonId));
        return this;
    } // internal ToastNotification AddButton (string, ToastCallback?)
} // internal class ToastNotification

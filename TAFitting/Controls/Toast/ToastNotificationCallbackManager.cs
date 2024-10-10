
// (c) 2024 Kazuki Kohzuki

using Microsoft.Toolkit.Uwp.Notifications;
using ToastCallback = System.Action<System.Collections.Generic.IDictionary<string, object>>;

namespace TAFitting.Controls.Toast;

/// <summary>
/// Manages the callbacks of toast notifications.
/// </summary>
internal static class ToastNotificationCallbackManager
{
    private static readonly Dictionary<string, ToastCallback> callbacks = [];

    static ToastNotificationCallbackManager()
    {
        ToastNotificationManagerCompat.OnActivated += OnActivatedEventHandler;
    } // cctor ()

    private static void OnActivatedEventHandler(ToastNotificationActivatedEventArgsCompat e)
    {
        try
        {
            if (callbacks.TryGetValue(e.Argument, out var callback))
                callback.Invoke(e.UserInput);
        }
        catch
        {

        }
        finally
        {
            RemoveCallbacks((ToastArgument)e.Argument);
        }
    } // private static void OnActivatedEventHandler (ToastNotificationActivatedEventArgsCompat)

    /// <summary>
    /// Adds the specified callback to the callback list.
    /// </summary>
    /// <param name="id">The ID of the toast notification.</param>
    /// <param name="callback">The callback to add.</param>
    internal static void AddCallback(string id, ToastCallback callback)
    {
        callbacks.Add(id, callback);
    } // internal static void AddCallback (string, ToastCallback)

    /// <summary>
    /// Removes the specified callback from the callback list.
    /// </summary>
    /// <param name="args">The toast argument.</param>
    private static void RemoveCallbacks(ToastArgument args)
    {
        var id = args.ToastId;

        var l = new HashSet<string>();
        foreach (var arg in callbacks.Keys)
        {
            if (((ToastArgument)arg).ToastId == id)
                l.Add(arg);
        }

        foreach (var arg in l)
            callbacks.Remove(arg);
    } // private static void RemoveCallbacks (ToastArgument)

    internal static void Uninstall()
        => ToastNotificationManagerCompat.Uninstall();
} // internal static class ToastNotificationCallbackManager


// (c) 2024 Kazuki Kohzuki

using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TAFitting;

/// <summary>
/// Provides methods to launch an application with the specified file extension.
/// </summary>
internal class AppLauncher
{
    private const string OpenCommand = @"shell\open\command";

    private static readonly Dictionary<string, AppLauncher> launchers = [];

    private readonly string extension;
    protected string appCommand;

    /// <summary>
    /// Gets the file extension associated to this instance.
    /// </summary>
    internal string Extension
        => this.extension;

    /// <summary>
    /// Gets a value indicating whether the associated application is registered.
    /// </summary>
    internal bool IsRegistered
        => !string.IsNullOrEmpty(this.appCommand);

    /// <summary>
    /// Gets the command to run the associated application with the specified file extension.
    /// </summary>
    internal string AppCommand
        => this.appCommand;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLauncher"/> class with the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension associated to this instance.</param>
    private AppLauncher(string extension)
    {
        this.extension = extension;
        this.appCommand = SearchApp(extension);
    } // ctor (string)

    /// <summary>
    /// Searches the associated application with the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension to search.</param>
    /// <returns>The command to run the associated application with the specified file extension.
    /// If the associated application is not found, returns an empty string.</returns>
    [MemberNotNull(nameof(appCommand))]
    protected virtual string SearchApp(string extension)
    {
        if (this.appCommand is not null) return this.appCommand;

        var keyExt = Registry.ClassesRoot.OpenSubKey(extension);
        if (keyExt?.OpenSubKey(OpenCommand) is RegistryKey openKey)
        {
            if (openKey.GetValue(string.Empty) is string command)
                return this.appCommand = command;
        }

        if (keyExt?.GetValue(string.Empty) is not string appName)
            return this.appCommand = string.Empty;

        var appKey = Registry.ClassesRoot.OpenSubKey(appName);
        var appOpenKey = appKey?.OpenSubKey(OpenCommand);
        return this.appCommand = appOpenKey?.GetValue(string.Empty) as string ?? string.Empty;
    } // protected virtual string SearchApp (string)

    /// <summary>
    /// Gets the command to run the associated application with the specified file.
    /// </summary>
    /// <param name="filename">The name of the file to open.</param>
    /// <returns>The command to run the associated application with the specified file.</returns>
    protected virtual string GetRunCommand(string filename)
    {
        var command = this.appCommand.Replace("%1", filename);
        if (command.StartsWith('"')) return command;
        var parts = command.Split(' ', 2);
        var app = Path.GetFullPath(parts[0]);
        if (!app.StartsWith('"'))
            app = $"\"{app}\"";
        return parts.Length > 1 ? $"{app} {parts[1]}" : app;
    } // protected virtual string GetRunCommand (string)

    /// <summary>
    /// Opens the specified file with the associated application.
    /// </summary>
    /// <param name="filename">The name of the file to open.</param>
    /// <returns><see langword="true"/> if the file is opened successfully; otherwise, <see langword="false"/>.</returns>
    internal bool OpenFile(string filename)
    {
        if (!this.IsRegistered) return false;
        var psi = new ProcessStartInfo(GetRunCommand(filename));
        return Process.Start(psi) is not null;
    } // internal bool OpenFile (string)

    /// <summary>
    /// Gets the instance of the <see cref="AppLauncher"/> class with the specified file extension.
    /// </summary>
    /// <param name="extension">The file extension to search.</param>
    /// <returns>An instance of the <see cref="AppLauncher"/> class with the specified file extension if found; otherwise, <see langword="null"/>.</returns>
    internal static AppLauncher? GetInstance(string extension)
    {
        extension = extension.ToUpperInvariant();

        if (!launchers.TryGetValue(extension, out var launcher))
        {
            launcher = new AppLauncher(extension);
            launchers.Add(extension, launcher);
        }

        if (!launcher.IsRegistered) return null;
        return launcher;
    } // internal static AppLauncher? GetInstance (string)
} // internal class AppLauncher

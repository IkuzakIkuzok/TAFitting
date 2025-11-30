
// (c) 2024-2025 Kazuki KOHZUKI

using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using TAFitting.Config;
using TAFitting.Controls;
using TAFitting.Controls.Toast;
using TAFitting.Model;
using TAFitting.Properties;
using TAFitting.Sync;
using TAFitting.Update;

[assembly: NeutralResourcesLanguage("en-US")]

namespace TAFitting;

internal static partial class Program
{
    internal const string AppName = "TA Fitting";

    internal const string GitHub = @"https://github.com/IkuzakIkuzok/TAFitting";

    internal static readonly string AppLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

    internal static readonly Guid FileDialogCommonId = new("897E3944-14A7-4D69-80D2-A28C5BD0E7BF");
    internal static readonly Guid SaveDialogId = new("C81B4633-F930-4C49-BC19-BB516B195980");

    /// <summary>
    /// Gets the main window.
    /// </summary>
    internal static MainWindow MainWindow { get; }

    static Program()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        SplashForm.ShowSplash();

        Config = AppConfig.Load();
        SyncManager.Start();
        MainWindow = new();
    } // cctor ()

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "s_defaultIcon")]
        extern static ref Icon DefaultIcon(Form? _ = null);
        DefaultIcon() = Resources.Icon;

        NegativeSignHandler.SetMinusSign();

        if (args.Length > 0)
        {
            var path = args[0];
            if (File.Exists(path))
                MainWindow.LoadFemtosecondDecays(path);
            else if (Directory.Exists(path))
                MainWindow.LoadMicrosecondDecays(path);
        }

        _ = UpdateManager.GetLatestVersionAsync();

        Application.Run(MainWindow);
        ToastNotificationCallbackManager.Uninstall();
    } // private static void Main (string[])

    internal static LinearCombinationItem AddLinearCombination(Guid guid, string name, string category, IEnumerable<Guid> guids)
    {
        var item = new LinearCombinationItem()
        {
            Guid = guid,
            Name = name,
            Category = category,
            Components = [.. guids],
        };
        Config.ModelConfig.LinearCombinations.Add(item);
        SaveConfig();
        return item;
    } // internal static LinearCombinationItem AddLinearCombination (Guid, string, string, IEnumerable<Guid>)

    internal static void RemoveLinearCombination(Guid guid)
    {
        ModelManager.RemoveModel(guid);
        var item = Config.ModelConfig.LinearCombinations.FirstOrDefault(i => i.Guid == guid);
        if (item is null) return;
        Config.ModelConfig.LinearCombinations.Remove(item);
        SaveConfig();
    } // internal static void RemoveLinearCombination (Guid)

    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    internal static AppConfig Config { get; }

    private static void SaveConfig()
    {
        try
        {
            Config.Save();
        }
        catch
        {
            FadingMessageBox.Show("Failed to save the app configuration.", 0.8, 1000, 75, 0.1);
        }
    } // private static void SaveConfig ()

    /// <summary>
    /// Opens the GitHub repository.
    /// </summary>
    internal static void OpenGitHub()
        => OpenUrl(GitHub);

    /// <summary>
    /// Opens the specified URL in the system's default web browser.
    /// </summary>
    /// <param name="url">The URL to open. This should be a valid, well-formed URI string.</param>
    internal static void OpenUrl(string url)
    {
        try
        {
            using var _ = Process.Start("explorer", url);
        }
        catch
        {
            FadingMessageBox.Show(
                $"Failed to open the URL:\n{url}",
                0.8, 1000, 75, 0.1
            );
        }
    } // internal static void OpenUrl (string)
} // internal static partial class Program

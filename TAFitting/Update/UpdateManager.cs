
// (c) 2025 Kazuki KOHZUKI

using System.Diagnostics;
using System.Net.Http;
using System.Reflection;

namespace TAFitting.Update;

/// <summary>
/// Provides functionality for retrieving and tracking information about the latest available application release.
/// </summary>
internal static class UpdateManager
{
    private const string VersionInfoUrl = "https://api.github.com/repos/IkuzakIkuzok/TAFitting/releases/latest";

    private static readonly HttpClient client = new()
    {
        DefaultRequestHeaders =
        {
            // The server returns 403 Forbidden if User-Agent is not set.
            { "User-Agent", "TAFitting-App" }
        }
    };

    /// <summary>
    /// Gets information about the most recent available release.
    /// </summary>
    internal static ReleaseInfo? LatestRelease
    {
        get;
        private set
        {
            field = value;
            if (field?.IsNewerThan(CurrentVersion) ?? false)
                OnNewerVersionFound(field);
        }
    }

    /// <summary>
    /// Gets the release information for the currently executing assembly.
    /// </summary>
    internal static ReleaseInfo CurrentVersion
        => field ??= new(Assembly.GetExecutingAssembly().GetName().Version!, string.Empty, string.Empty);

    /// <summary>
    /// Occurs when a newer version of the application is detected.
    /// </summary>
    internal static event EventHandler<NewerVersionFoundEventArgs>? NewerVersionFound;

    /// <summary>
    /// Retrieves information about the latest available release version from the remote source.
    /// </summary>
    /// <param name="forceCheck">If set to <see langword="true"/>, forces a check for the latest version even if a cached value is available;
    /// otherwise, uses the cached release information if present.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.
    /// The task result contains a <see cref="ReleaseInfo"/> object describing the latest release.
    /// If the latest release cannot be determined, returns information about the current version.</returns>
    async internal static Task<ReleaseInfo> GetLatestVersionAsync(bool forceCheck = false)
    {
        if (LatestRelease is not null && !forceCheck)
            return LatestRelease;

        try
        {
            var response = await client.GetStringAsync(VersionInfoUrl);
            var releaseInfo = ReleaseInfoJson.LoadJson(response);
            if (releaseInfo is null)
                return CurrentVersion;

            Debug.WriteLine($"Fetched release info: Tag={releaseInfo.TagName}");

            var versionString = releaseInfo.TagName?.TrimStart('v', 'V');
            if (!Version.TryParse(versionString, out var version))
                return CurrentVersion;

            Debug.WriteLine($"Parsed version: {version}");

            var browserUrl = releaseInfo.HtmlUrl ?? string.Empty;

            var zipAsset = releaseInfo.Assets?.FirstOrDefault(a => a.Name?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true);
            var zipUrl = zipAsset?.BrowserDownloadUrl ?? string.Empty;

            return LatestRelease = new(version, browserUrl, zipUrl);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error fetching latest version: {e}");
            return CurrentVersion;
        }
    } // async internal static Task<ReleaseInfo> GetLatestVersionAsync ([bool])

    private static void OnNewerVersionFound(ReleaseInfo latest)
        => NewerVersionFound?.Invoke(null, new(latest));
} // internal static class UpdateManager


// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Update;

/// <summary>
/// Represents information about a specific software release, including its version and download locations.
/// </summary>
/// <param name="Version">The version number of the release.</param>
/// <param name="BrowserUrl">The URL where the release can be accessed or viewed in a web browser.</param>
/// <param name="ZipUrl">The URL to the ZIP archive containing the release files.</param>
internal sealed record ReleaseInfo(Version Version, string BrowserUrl, string ZipUrl)
{
    /// <summary>
    /// Determines whether the current release has a higher version than the specified release.
    /// </summary>
    /// <param name="other">The release to compare with the current instance. Cannot be null.</param>
    /// <returns><see langword="true"/> if the current release's version is greater than the specified release's version; otherwise, <see langword="false"/>.</returns>
    internal bool IsNewerThan(ReleaseInfo other)
        => this.Version > other.Version;
} // internal sealed record ReleaseInfo

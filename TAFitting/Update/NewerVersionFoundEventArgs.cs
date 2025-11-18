
// (c) 2025 Kazuki KOHZUKI

namespace TAFitting.Update;

/// <summary>
/// Represents the method that will handle an event when a newer version is found.
/// </summary>
/// <param name="sender">The source of the event. This is typically the object that raised the event.</param>
/// <param name="e">A <see cref="NewerVersionFoundEventArgs"/> that contains the event data.</param>
internal delegate void NewerVersionFoundEventHandler(object? sender, NewerVersionFoundEventArgs e);

/// <summary>
/// Provides data for an event that is raised when a newer version of the application is found.
/// </summary>
internal sealed class NewerVersionFoundEventArgs : EventArgs
{
    /// <summary>
    /// Gets the most recent release information available.
    /// </summary>
    internal ReleaseInfo LatestRelease { get; }

    /// <summary>
    /// Initializes a new instance of the NewerVersionFoundEventArgs class with information about the latest available
    /// release.
    /// </summary>
    /// <param name="latestRelease">The ReleaseInfo object that contains details about the latest available release.</param>
    internal NewerVersionFoundEventArgs(ReleaseInfo latestRelease)
    {
        this.LatestRelease = latestRelease;
    } // ctor (ReleaseInfo)
} // internal sealed class NewerVersionFoundEventArgs : EventArgs

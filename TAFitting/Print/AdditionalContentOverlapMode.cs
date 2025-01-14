
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Print;

/// <summary>
/// Represents the mode for handling the overlap of additional contents.
/// </summary>
internal enum AdditionalContentOverlapMode
{
    /// <summary>
    /// Allows the content to overlap.
    /// </summary>
    AllowOverlap,

    /// <summary>
    /// Throws an exception if the content overlaps.
    /// </summary>
    Error,

    /// <summary>
    /// Overwrites the existing content.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Ignores the new content and keeps the existing content.
    /// </summary>
    Ignore,
} // internal enum AdditionalContentOverlapMode

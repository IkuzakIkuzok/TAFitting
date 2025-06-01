
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Config;

/// <summary>
/// Represents the configuration for synchronization operations.
/// </summary>
[Serializable]
public sealed class SyncConfig
{
    /// <summary>
    /// Gets or sets the timeout for synchronization operations in milliseconds.
    /// </summary>
    public int Timeout { get; set; } = 100;

    /// <summary>
    /// Gets or sets the retry count for connection attempts.
    /// </summary>
    public int ConnectionRetryCount { get; set; } = 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncConfig"/> class.
    /// </summary>
    public SyncConfig() { }
} // public sealed class SyncConfig

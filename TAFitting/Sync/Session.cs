
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Sync;

/// <summary>
/// Represents a session with another instance of the application.
/// </summary>
internal sealed class Session
{
    private readonly Dictionary<string, string> properties = [];

    /// <summary>
    /// Gets the host name of the session.
    /// </summary>
    internal string HostName { get; }

    /// <summary>
    /// Gets the properties associated with the session.
    /// </summary>
    internal IReadOnlyDictionary<string, string> Properties => this.properties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Session"/> class with the specified host name.
    /// </summary>
    /// <param name="hostName">The host name.</param>
    internal Session(string hostName)
    {
        this.HostName = hostName;
    } // ctor (string)

    /// <summary>
    /// Gets the property value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the property.</param>
    /// <returns>The value of the property if it exists; otherwise, <see langword="null"/>.</returns>
    internal string? GetProperty(string key)
    {
        if (this.properties.TryGetValue(key, out var value))
            return value;
        return null;
    } // internal string? GetProperty (string)

    /// <summary>
    /// Sets the property value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the property.</param>
    /// <param name="value">The value to set for the property. If <see langword="null"/>, the property will be removed.</param>
    internal void SetProperty(string key, string value)
    {
        if (string.IsNullOrEmpty(key)) return;
        if (value is null)
            this.properties.Remove(key);
        else
            this.properties[key] = value;
    } // internal void SetProperty (string, string)
} // internal sealed class Session

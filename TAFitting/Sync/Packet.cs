
// (c) 2025 Kazuki Kohzuki

namespace TAFitting.Sync;

/// <summary>
/// Represents a packet for communication between instances of the application.
/// </summary>
internal sealed class Packet
{
    /*
     * Packet format:
     * source name + '\0' + destination name + '\0' + message
     */

    /// <summary>
    /// Gets the host name of the source of the packet.
    /// </summary>
    internal string SourceName { get; }

    /// <summary>
    /// Gets the host name of the destination of the packet.
    /// </summary>
    internal string DestinationName { get; }

    /// <summary>
    /// Gets the message contained in the packet.
    /// </summary>
    internal string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Packet"/> class.
    /// </summary>
    /// <param name="sourceName">The host name of the source of the packet.</param>
    /// <param name="destinationName">The host name of the destination of the packet.</param>
    /// <param name="message">The message contained in the packet.</param>
    internal Packet(string sourceName, string destinationName, string message)
    {
        this.SourceName = sourceName;
        this.DestinationName = destinationName;
        this.Message = message;
    } // ctor (string, string, string)

    /// <inheritdoc/>
    override public string ToString()
        => $"{this.SourceName}\0{this.DestinationName}\0{this.Message}";

    /// <summary>
    /// Parses a packet string into a <see cref="Packet"/> object.
    /// </summary>
    /// <param name="packetString">The string representation of the packet.</param>
    /// <returns>The parsed <see cref="Packet"/> object.</returns>
    /// <exception cref="FormatException">Invalid packet format.</exception>
    internal static Packet Parse(string packetString)
    {
        var parts = packetString.Split('\0', 3);
        if (parts.Length != 3)
            throw new FormatException("Invalid packet format.");

        var sourceName = parts[0];
        var destinationName = parts[1];

        var message = parts[2];

        return new Packet(sourceName, destinationName, message);
    } // static Packet Parse (string)
} // internal sealed class Packet

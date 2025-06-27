
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace TAFitting.Sync;

/// <summary>
/// Handles synchronization of messages between instances of the application.
/// </summary>
internal static class SyncServer
{
    private static string myName = string.Empty;

    private static bool receiving = false;
    private static readonly List<Session> sessions = [];

    /// <summary>
    /// Gets the list of connected hosts.
    /// </summary>
    internal static IEnumerable<string> ConnectedHosts
        => sessions.Select(s => s.HostName);

    /// <summary>
    /// Gets or sets the function to create a response message based on the received session and message.
    /// </summary>
    internal static Func<Session, string, string>? CreateResponseMessage { get; set; }

    /// <summary>
    /// Gets the name of this instance.
    /// </summary>
    internal static string MyName => myName;

    /// <summary>
    /// Starts listening for incoming messages from other instances of the application.
    /// </summary>
    internal static void StartListening()
    {
        if (receiving) return;
        receiving = true;
        var name = Environment.ProcessId.ToInvariantString("X");
        _ = Listen(name);
    } // internal static void StartListening ()

    /// <summary>
    /// Stops listening for incoming messages and clears the session list.
    /// </summary>
    internal static void StopListening()
    {
        receiving = false;
        sessions.Clear();
    } // internal static void StopListening ()

    private static async Task Listen(string hostName)
    {
        myName = hostName;
        var pipeName = GetPipeName(myName);
        await Task.Run(async () =>
        {
            while (receiving)
            {
                try
                {
                    using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 16);
                    await server.WaitForConnectionAsync();

                    using var reader = new StreamReader(server, Encoding.UTF8, leaveOpen: true);
                    var packetString = await reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(packetString)) continue;
                    var packet = Packet.Parse(packetString);
                    if (packet == null) continue;

                    var session = GetSession(packet.SourceName);
                    var receivedMessage = packet.Message;
                    Debug.WriteLine($"Received message from {session.HostName}: {receivedMessage}");

                    var responseMessage = CreateResponseMessage?.Invoke(session, receivedMessage) ?? string.Empty;
                    var responsePacket = new Packet(myName, packet.SourceName, responseMessage);
                    using var responseWriter = new StreamWriter(server, Encoding.UTF8);
                    await responseWriter.WriteLineAsync(responsePacket.ToString());
                    responseWriter.Flush();
                    server.WaitForPipeDrain();
                }
                catch (IOException)
                {
                    Debug.WriteLine("Session disconnected or error occurred while reading from the pipe.");
                }
            }
        });
    } // private static Task Listen (string hostName)

    /// <summary>
    /// Sends a message to the specified session and waits for a response.
    /// </summary>
    /// <param name="session">The session to which the message will be sent.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>The response message received from the session, or an empty string if no response is received.</returns>
    internal static async Task<string> SendMessage(Session session, string message)
    {
        if (!sessions.Contains(session))
            sessions.Add(session);

        for (var i = 0; i < Program.Config.SyncConfig.ConnectionRetryCount; i++)
        {
            try
            {
                var pipeName = GetPipeName(session.HostName);
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await client.ConnectAsync(Program.Config.SyncConfig.Timeout);
                using var writer = new StreamWriter(client, Encoding.UTF8);
                var packet = new Packet(myName, session.HostName, message);
                await writer.WriteLineAsync(packet.ToString());
                writer.Flush();
                Debug.WriteLine($"Sent message to {session.HostName}: {message}");
                client.WaitForPipeDrain();

                using var reader = new StreamReader(client, Encoding.UTF8, leaveOpen: true);
                var packetString = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(packetString)) return string.Empty;

                packet = Packet.Parse(packetString);
                Debug.Assert(packet.SourceName == session.HostName, "Packet source name does not match session host name.");
                var responseMessage = packet.Message;
                Debug.WriteLine($"Received response from {session.HostName}: {responseMessage}");
                return responseMessage;
            }
            catch (TimeoutException)
            {
                Debug.WriteLine($"Timeout while sending message to {session.HostName}.");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error sending message to {session.HostName}: {e.Message}");
            }
        }
        return string.Empty;
    } // internal static Task<string> SendMessage (Session, string)

    /// <summary>
    /// Gets or creates a session for the specified host name.
    /// </summary>
    /// <param name="hostName">The host name of the session to retrieve or create.</param>
    /// <returns>The session associated with the specified host name.</returns>
    internal static Session GetSession(string hostName)
    {
        var session = sessions.FirstOrDefault(s => s.HostName == hostName);
        if (session is null)
        {
            session = new(hostName);
            sessions.Add(session);
        }
        return session;
    } // internal static Session GetSession (string)

    private static string GetPipeName(string hostName)
        => $"TAFitting.Sync.{hostName}";

    /// <summary>
    /// Disconnects the specified session from the server.
    /// </summary>
    /// <param name="session"></param>
    internal static void Disconnect(Session session)
    {
        sessions.Remove(session);
    } // internal static void Disconnect (Session session)
} // internal static class SyncServer

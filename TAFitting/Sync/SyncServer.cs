
// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace TAFitting.Sync;

internal static class SyncServer
{
    private static string myName = string.Empty;

    private static bool receiving = false;
    private static readonly List<Session> sessions = [];

    internal static IEnumerable<string> ConnectedHosts
        => sessions.Select(s => s.HostName);

    internal static Func<Session, string, string>? CreateResponseMessage { get; set; }

    internal static string MyName => myName;

    internal static void StartListening()
    {
        if (receiving) return;
        receiving = true;
        var name = Environment.ProcessId.ToString("X");
        _ = Listen(name);
    } // internal static void StartListening ()

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

    internal static async Task<string> SendMessage(Session session, string message)
    {
        if (!sessions.Contains(session))
            sessions.Add(session);

        for (var i = 0; i < 4; i++)
        {
            try
            {
                var pipeName = GetPipeName(session.HostName);
                using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                await client.ConnectAsync(100);
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
            catch (Exception e)
            {
                Debug.WriteLine($"Error sending message to {session.HostName}: {e.Message}");
            }
        }
        return string.Empty;
    } // internal static Task<string> SendMessage (Session, string)

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

    internal static void Disconnect(Session session)
    {
        sessions.Remove(session);
    } // internal static void Disconnect (Session session)
} // internal static class SyncServer


// (c) 2025 Kazuki Kohzuki

using System.Diagnostics;
using TAFitting.Collections;

namespace TAFitting.Sync;

/// <summary>
/// Manages synchronization between different instances of the application.
/// </summary>
internal static class SyncManager
{
    private static readonly List<string> apps = [];

    // key: Window ID, value: host names
    private static readonly Dictionary<int, List<string>> spectraSyncList = [];

    /// <summary>
    /// Occurs when spectra data is received from another instance.
    /// </summary>
    internal static event SpectraReceivedEventHandler? SpectraReceived;

    /// <summary>
    /// Starts the synchronization server and sets up the message handler.
    /// </summary>
    internal static void Start()
    {
        SyncServer.CreateResponseMessage = HandleMessage;
        SyncServer.StartListening();
    } // internal static void Start ()

    /// <summary>
    /// Stops the synchronization server and clears the message handler.
    /// </summary>
    internal static void Stop()
    {
        SyncServer.StopListening();
        SyncServer.CreateResponseMessage = null;
    } // internal static void Stop ()

    /// <summary>
    /// Gets a list of applications that are currently running the TAFitting application.
    /// </summary>
    /// <returns>The dictionary where the key is the application ID and the value is the sample name.</returns>
    internal static async Task<IDictionary<string, string>> GetApps()
    {
        UpdateAppList();
        var message = new MessageObject { Type = "q:sampleName", }.ToString();
        var dict = new Dictionary<string, string>();
        foreach (var id in apps)
        {
            var session = SyncServer.GetSession(id);
            var res = await SyncServer.SendMessage(session, message);
            if (string.IsNullOrEmpty(res)) continue;
            var json = MessageObject.LoadJson(res);
            if (json is null || string.IsNullOrEmpty(json.Type) || json.Type != "r:sampleName") continue;
            var sampleName = json.Content;
            if (string.IsNullOrEmpty(sampleName)) continue;
            dict.Add(id, sampleName);
        }
        return dict;
    } // internal static Task<IDictionary<string, string>> GetApps ()

    /// <summary>
    /// Gets a list of spectra IDs that are currently registered for synchronization.
    /// </summary>
    /// <param name="hostName">The host name of the application instance to query.</param>
    /// <returns>The list of spectra IDs.</returns>
    internal static async Task<IEnumerable<int>> GetSpectra(string hostName)
    {
        var message = new MessageObject { Type = "q:spectraList", }.ToString();
        var session = SyncServer.GetSession(hostName);
        var res = await SyncServer.SendMessage(session, message);
        if (string.IsNullOrEmpty(res)) return [];
        var json = MessageObject.LoadJson(res);
        if (json is null || string.IsNullOrEmpty(json.Type) || json.Type != "r:spectraList") return [];
        return json.Content?.Split(',').Select<string, int>(int.TryParse) ?? [];
    } // internal static async Task<IEnumerable<int>> GetSpectra (string)

    private static void UpdateAppList()
    {
        apps.Clear();
        foreach (var proc in Process.GetProcessesByName("TAFitting"))
        {
            var id = proc.Id.ToInvariantString("X");
            if (id == SyncServer.MyName) continue;  // Skip self
            if (string.IsNullOrEmpty(id) || apps.Contains(id)) continue;
            apps.Add(id);
        }
    } // private static void UpdateAppList ()

    private static string HandleMessage(Session session, string message)
    {
        if (string.IsNullOrEmpty(message)) return string.Empty;

        var json = MessageObject.LoadJson(message);
        if (json is null || string.IsNullOrEmpty(json.Type)) return string.Empty;

        if (json.Type == "q:sampleName")
        {
            var mainWindow = Program.MainWindow;
            var sampleName = mainWindow?.SampleName;
            var res = new MessageObject
            {
                Type = "r:sampleName",
                Content = sampleName,
            };
            return res.ToString();
        }
        
        if (json.Type == "q:spectraSync")
        {
            if (!int.TryParse(json.Content, out var spectraId))
                return new MessageObject
                {
                    Type = "r:spectraSync",
                    Content = null,
                }.ToString();

            var mainWindow = Program.MainWindow;
            if (!mainWindow.SpectraIds.Contains(spectraId))
                return new MessageObject
                {
                    Type = "r:spectraSync",
                    Content = null,
                }.ToString();

            RegisterSpectraSync(session.HostName, spectraId);
            var res = new MessageObject
            {
                Type = "r:spectraSync",
                Content = Program.MainWindow.GetSyncSpectra(spectraId)?.ToString(),
            };
            return res.ToString();
        }

        if (json.Type == "q:stopSpectraSync")
        {
            if (!int.TryParse(json.Content, out var spectraId)) return string.Empty;
            UnregisterSpectraSync(session.HostName, spectraId);
            return new MessageObject
            {
                Type = "r:stopSpectraSync",
                Content = "OK", // Acknowledge the stop request
            }.ToString();
        }

        if (json.Type == "q:spectraList")
        {
            var res = new MessageObject
            {
                Type = "r:spectraList",
                Content = string.Join(',', Program.MainWindow.SpectraIds),
            };
            return res.ToString();
        }

        if (json.Type == "q:spectraUpdated")
        {
            var content = json.Content;
            if (string.IsNullOrEmpty(content)) return string.Empty;
            OnSpectraReceived(content);
            var res = new MessageObject
            {
                Type = "r:spectraUpdated",
                Content = "OK", // Acknowledge the update
            };
            return res.ToString();
        }

        return string.Empty;
    } // private static string HandleMessage (Session, string)

    private static void RegisterSpectraSync(string hostName, int spectraId)
    {
        var list = spectraSyncList.GetValueOrDefault(spectraId, []);
        if (list.Contains(hostName)) return; // Already registered
        list.Add(hostName);
        spectraSyncList[spectraId] = list;
    } // private static void RegisterSpectraSync (string, int)

    private static void UnregisterSpectraSync(string hostName, int spectraId)
    {
        if (!spectraSyncList.TryGetValue(spectraId, out var list)) return; // No registered hosts
        if (!list.Remove(hostName)) return; // Host not found
        if (list.Count == 0) spectraSyncList.Remove(spectraId); // Remove entry if no hosts left
    } // private static void UnregisterSpectraSync (string, int)

    /// <summary>
    /// Checks if there are any registered hosts for the given window ID.
    /// </summary>
    /// <param name="windowId">The ID of the window to check.</param>
    /// <returns><see langword="true"/> if there are registered hosts; otherwise, <see langword="false"/>.</returns>
    internal static bool CheckRegistered(int windowId)
    {
        if (!spectraSyncList.TryGetValue(windowId, out var targetHosts)) return false;
        return targetHosts.Count > 0; // Return true if there are any registered hosts
    } // internal static bool CheckRegistered (int)

    /// <summary>
    /// Send an update to all registered hosts for the given spectra synchronization object.
    /// </summary>
    /// <param name="syncObject">The spectra synchronization object containing the updated data.</param>
    internal static void UpdateSpectra(SpectraSyncObject syncObject)
    {
        var targetHosts = spectraSyncList.GetValueOrDefault(syncObject.SpectraId, []);
        if (targetHosts.Count == 0) return; // No hosts to update
        Task.Run(async () =>
        {
            var message = new MessageObject
            {
                Type = "q:spectraUpdated",
                Content = syncObject.ToString(),
            }.ToString();

            var failed = new List<string>();
            foreach (var host in targetHosts)
            {
                var session = SyncServer.GetSession(host);
                var response = await SyncServer.SendMessage(session, message);

                var json = string.IsNullOrEmpty(response) ? null : MessageObject.LoadJson(response);
                if (json is null || string.IsNullOrEmpty(json.Type) || json.Type != "r:spectraUpdated")
                {
                    Debug.WriteLine($"Failed to update spectra for {host}: Invalid response");
                    failed.Add(host);
                    continue; // Skip if response is invalid
                }

                Debug.WriteLine($"Sent spectra update to {host}: {response}");
            }

            foreach (var host in failed)
                UnregisterSpectraSync(host, syncObject.SpectraId);
        });
    } // internal static void UpdateSpectra (int, IList<double>, IDictionary<double, IList<double>>)

    private static void OnSpectraReceived(string content)
    {
        var syncObject = SpectraSyncObject.Parse(content);
        if (syncObject is null) return; // Invalid data
        var e = new SpectraReceivedEventArgs(syncObject);
        SpectraReceived?.Invoke(null, e);
    } // private static void OnSpectraReceived (string)

    /// <summary>
    /// Sends a request to synchronize spectra data from another instance.
    /// </summary>
    /// <param name="hostName">The host name of the application instance to request synchronization from.</param>
    /// <param name="spectraId">The ID of the spectra to synchronize.</param>
    /// <returns>The synchronized spectra data or <see langword="null"/> if the request failed.</returns>
    internal static async Task<SpectraSyncObject?> RequestSyncSpectra(string hostName, int spectraId)
    {
        var message = new MessageObject
        {
            Type = "q:spectraSync",
            Content = spectraId.ToInvariantString(),
        }.ToString();
        var session = SyncServer.GetSession(hostName);
        var response = await SyncServer.SendMessage(session, message);
        if (string.IsNullOrEmpty(response)) return null;
        var json = MessageObject.LoadJson(response);
        if (json is null || string.IsNullOrEmpty(json.Type) || json.Type != "r:spectraSync") return null;
        return SpectraSyncObject.Parse(json.Content);
    } // internal static async Task<SpectraSyncObject?> RequestSyncSpectra (string, int)

    /// <summary>
    /// Stops the synchronization of spectra data for a given spectra ID on a specified host.
    /// </summary>
    /// <param name="hostName">The host name of the application instance to stop synchronization on.</param>
    /// <param name="spectraId">The ID of the spectra to stop synchronization for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal static async Task StopSyncSpectra(string hostName, int spectraId)
    {   
        var message = new MessageObject
        {
            Type = "q:stopSpectraSync",
            Content = spectraId.ToInvariantString(),
        }.ToString();
        var session = SyncServer.GetSession(hostName);
        var response = await SyncServer.SendMessage(session, message);
        Debug.WriteLine($"Stopped spectra sync for {hostName}: {response}");
    } // internal static async Task StopSyncSpectra (string, int)
} // internal static class SyncManager

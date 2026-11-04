using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// Class in charge of the browser clients: launches the Node server, connects to it via
/// WebSocket and relays signaling between it and Unity
/// </summary>
public class WebSocketServerRTC : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// IP of the device running the Node server (assumed to be this same machine)
    /// </summary>
    public string nodeHost = "192.168.1.45";

    /// <summary>
    /// Port where the Node server listens for WebSocket connections
    /// </summary>
    [SerializeField]
    private int nodePort = 8080;

    /// <summary>
    /// Port where the web page is served for the browsers
    /// </summary>
    [SerializeField]
    private int browserPort = 3000;

    /// <summary>
    /// Whether the server is running or not
    /// </summary>
    private bool running = false;

    /// <summary>
    /// Whether it accepts new connections or not
    /// </summary>
    public bool acceptsConnections = true;

    /// <summary>
    /// Socket connected to the Node server
    /// </summary>
    private ClientWebSocket ws;

    /// <summary>
    /// PID of the launched bat process, so it can be closed directly instead of
    /// searching by window title (unreliable)
    /// </summary>
    private Process launchedBatProcess;

    /// <summary>
    /// Path to the bat from the root folder of the build or of the project
    /// </summary>
    [SerializeField]
    private string batRelativePath = "start-server.bat";

    /// <summary>
    /// Absolute path to the bat, resolved on Start
    /// </summary>
    private string batPath;

    /// <summary>
    /// Used to cancel the asynchronous ReceiveLoop task
    /// </summary>
    private CancellationTokenSource _cts;

    /// <summary>
    /// Address of the Node server
    /// </summary>
    private Uri nodeUri;

    #endregion

    #region Bat

    /// <summary>
    /// Unblocks a file, removing the Zone.Identifier mark Windows adds to downloaded files
    /// </summary>
    /// <param name="path">Path to the file to unblock</param>
    public static void UnblockFile(string path)
    {
        string zoneIdentifierPath = path + ":Zone.Identifier";
        try
        {
            if (System.IO.File.Exists(zoneIdentifierPath))
            {
                System.IO.File.Delete(zoneIdentifierPath);
                UnityEngine.Debug.Log("[ServerLauncher] Archivo desbloqueado correctamente.");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[ServerLauncher] No se pudo desbloquear el archivo: {ex.Message}");
        }
    }

    /// <summary>
    /// Looks up which process is listening on a given TCP port using netstat, and kills it by PID with taskkill
    /// </summary>
    /// <param name="port">Port to free</param>
    public static void KillProcessOnPort(int port)
    {
        try
        {
            ProcessStartInfo netstatPsi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c netstat -ano | findstr :{port}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process netstat = Process.Start(netstatPsi))
            {
                string output = netstat.StandardOutput.ReadToEnd();
                netstat.WaitForExit();

                int selfPid = Process.GetCurrentProcess().Id;

                var seenPids = new HashSet<string>();
                foreach (string line in output.Split('\n'))
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;

                    // netstat -ano TCP: Proto | Local | Foreign | State | PID
                    string[] parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 5) continue;
                    if (!parts[1].EndsWith(":" + port)) continue;    // the port must be the LOCAL one, not the remote one
                    if (!parts[3].Equals("LISTENING", StringComparison.OrdinalIgnoreCase)) continue;

                    string pid = parts[parts.Length - 1];
                    if (!int.TryParse(pid, out int pidInt)) continue;
                    if (pidInt == selfPid || pidInt <= 4) continue; // never kill ourselves nor System/Idle
                    if (!seenPids.Add(pid)) continue;   // avoid killing the same PID several times

                    ProcessStartInfo killPsi = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {pid} /F /T",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(killPsi)?.WaitForExit();
                    UnityEngine.Debug.Log($"[ServerLauncher] Proceso en puerto {port} (PID {pid}) cerrado.");
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[ServerLauncher] No se pudo cerrar el puerto {port}: {ex.Message}");
        }
    }

    #endregion

    #region Activation/Deactivation

    /// <summary>
    /// Launches the Node server by running the .bat file
    /// </summary>
    public void LaunchServer()
    {
        running = true;
        UnblockFile(batPath);
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = batPath,
            Arguments = $"{nodePort} {browserPort}",
            WorkingDirectory = Application.streamingAssetsPath,
            UseShellExecute = true,
            CreateNoWindow = false
        };

        try
        {
            launchedBatProcess = Process.Start(psi);
            UnityEngine.Debug.Log($"[WebSocketServerRTC] Server launched correctly. PID: {launchedBatProcess?.Id}");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[WebSocketServerRTC] Error running .bat: {ex.Message}");
        }

    }

    /// <summary>
    /// Stops the Node server, the HTML server and the bat window
    /// </summary>
    public void StopServer()
    {
        running = false;
        KillProcessOnPort(nodePort);
        KillProcessOnPort(browserPort);

        string windowTitle = "Streaming server";

        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/F /T /FI \"WINDOWTITLE eq {windowTitle}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process p = Process.Start(psi))
            {
                string output = p.StandardOutput.ReadToEnd();
                string error = p.StandardError.ReadToEnd();
                p.WaitForExit();
                UnityEngine.Debug.Log($"[WebSocketServerRTC] taskkill by title '{windowTitle}': {output.Trim()}{error.Trim()}");
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[WebSocketServerRTC] Couldn0t close window '{windowTitle}': {ex.Message}");
        }

        launchedBatProcess = null;
    }

    #endregion

    #region Connection

    /// <summary>
    /// Starts the connection to the Node server (with retries in case it runs before LaunchServer has finished)
    /// </summary>
    public async void ConnectToNode()
    {
        int maxRetries = 10;
        int delayMs = 1500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                ws = new ClientWebSocket(); // recreate it, a failed ClientWebSocket cannot be reused
                await ws.ConnectAsync(nodeUri, CancellationToken.None);
                UnityEngine.Debug.Log($"[WebSocketServerRTC] Connected to Node: {nodeUri}");
                _ = ReceiveLoop(_cts);
                return; // success, exit
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[WebSocketServerRTC] Failed attemp {attempt}/{maxRetries}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        UnityEngine.Debug.LogError("[WebSocketServerRTC] Couldn't connect to Node after the maximum attemps.");
    }

    /// <summary>
    /// Closes the connection to the Node server, saying goodbye to every browser client first
    /// </summary>
    public async Task DisconnectToNode()
    {
        var browserClients = StreamManager.Instance.GetClients()
            .Where(c => c.type == ClientConnectionType.WEB_SOCKET).ToList();

        if (ws?.State == WebSocketState.Open && browserClients.Count > 0)
        {
            SignalingMessage byeMsg = new SignalingMessage(ConnectionEvent.DISCONNECT, null);

            foreach (var client in browserClients)
            {
                await SendToNode(client.clientID, byeMsg);
                StreamManager.Instance.RemovePeer(client.clientID);
            }
        }

        _cts?.Cancel();
        if (ws?.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected", CancellationToken.None);

        _cts.Dispose();
        ws.Dispose();
        _cts = new CancellationTokenSource();
        ws = new ClientWebSocket();

        UnityEngine.Debug.Log("[WebSocketServerRTC] Node clients deleted.");
    }

    #endregion

    #region Communication

    /// <summary>
    /// Reception loop for the WebSocket messages
    /// </summary>
    /// <param name="token">Token that acts as a FLAG to be able to cancel the process</param>
    async Task ReceiveLoop(CancellationTokenSource token)
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();

        try
        {
            while (!token.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await ws.ReceiveAsync(
                            new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close) return;
                        sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                    }
                    while (!result.EndOfMessage);

                    string json = sb.ToString();
                    sb.Clear();

                    UnityMainThreadDispatcher.Instance().Enqueue(() => HandleWebIncoming(json));
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[WebSocketServerRTC] ReceiveLoop: {ex.Message}");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            UnityEngine.Debug.LogError($"[WebSocketServerRTC] ReceiveLoop: Se detuvo el proceso de recepcion de informacion del servidor de Node");
        }
    }

    /// <summary>
    /// Handles the information received from the Node server
    /// </summary>
    /// <param name="rawJson">Raw WSMessage JSON received</param>
    void HandleWebIncoming(string rawJson)
    {
        WSMessage msg = JsonUtility.FromJson<WSMessage>(rawJson);

        if (msg.type == 99 && acceptsConnections) // newClient
        {
            string clientKey = msg.clientId.ToString();
            ClientData client = ClientData.ForBrowser(clientKey);

            UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.CreatePeer(client));
            UnityEngine.Debug.Log($"[WebSocketServerRTC] Browser registered: {clientKey}");
        }
        else if (msg.type == (int)ConnectionEvent.DISCONNECT) // a browser disconnected on Node's side
        {
            string clientKey = msg.clientId.ToString();

            StreamManager.Instance?.RemovePeer(clientKey);
            UnityEngine.Debug.Log($"[WebSocketServerRTC] Browser deleted from register: {clientKey}");
        }
        else // SDP or ICE from an existing browser
        {
            SignalingMessage sigMsg = new SignalingMessage((ConnectionEvent)msg.type, msg.body);
            string clientKey = msg.clientId.ToString();
            StreamManager.Instance?.HandleIncomingSignaling(clientKey, sigMsg);
        }
    }

    /// <summary>
    /// Sends data to Node's server.
    /// </summary>
    /// <param name="clientId">Client the data is addressed to.</param>
    /// <param name="msg">Signaling message.</param>
    public async Task SendToNode(string clientId, SignalingMessage msg)
    {
        if (ws?.State != WebSocketState.Open) return;

        if (!int.TryParse(clientId, out int idInt))
        {
            UnityEngine.Debug.LogError($"[WebSocketServerRTC] Invalid clientId: {clientId}");
            return;
        }

        try
        {
            WSMessage wsmes = new WSMessage { type = (int)msg.type, clientId = idInt, body = msg.body };
            byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(wsmes));
            await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[WebSocketServerRTC] Error sending message to Node (clientId={clientId}): {ex.Message}");
        }
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Returns the IP of the device running the Node server
    /// </summary>
    public string GetNodeHost()
    {
        return nodeHost;
    }

    /// <summary>
    /// Returns the port where the web page is served for the browsers
    /// </summary>
    public int GetBrowserPort()
    {
        return browserPort;
    }

    #endregion

    #region Monobehaviour

    public void Start()
    {
        nodeHost = NetworkUtils.GetIP();
        batPath = System.IO.Path.Combine(Application.streamingAssetsPath, batRelativePath);
        ws = new ClientWebSocket();
        nodeUri = new Uri($"ws://{nodeHost}:{nodePort}?type=unity&id={StreamManager.Instance.GetSessionID()}");
        _cts = new CancellationTokenSource();
    }

    void OnDestroy()
    {
        if (!running) return;
        try
        {
            ws?.Abort();
            ws?.Dispose();
            _cts?.Cancel();
            StopServer();
        }
        catch { }
    }

    #endregion

}
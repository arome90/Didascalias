using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Class in charge to establish ADB connections
/// </summary>
public class ADBConnectionServer : MonoBehaviour
{

    #region Variables
      
    /// <summary>
    /// Defines whether we print information or not
    /// </summary>
    [SerializeField]
    private bool debug = false;

    /// <summary>
    /// Port the mobile app connects to on its own localhost
    /// </summary>
    [SerializeField]
    private int remotePort = 7778;

    /// <summary>
    /// First PC-side port available for reverse tunnels
    /// </summary>
    [SerializeField]
    private int localPortRangeStart = 7780;

    /// <summary>
    /// Last PC-side port available. Defines how many ADB devices can be handled at once
    /// </summary>
    [SerializeField]
    private int localPortRangeEnd = 7879;

    /// <summary>
    /// How often (ms) n"adb devices" is executed on background thread to notice changes
    /// </summary>
    [SerializeField]
    private int devicePollIntervalMs = 2000;

    /// <summary>
    /// Indicates whether the script is running or not
    /// </summary>
    private bool running;

    /// <summary>
    /// Whether it accepts new connections or not
    /// </summary>
    public bool acceptsConnections = true;

    /// <summary>
    /// Path to adb.exe
    /// </summary>
    private string adbPath = "";

    /// <summary>
    /// Thread that executes "adb devices" command to notice if the status changes
    /// </summary>
    private Thread deviceWatcherThread;

    /// <summary>
    /// Structure that contains which ports are being actively used by clients
    /// </summary>
    private readonly HashSet<int> usedLocalPorts = new HashSet<int>();

    /// <summary>
    /// Active sessions keyed by ADB serial (one per physical device).
    /// </summary>
    private readonly ConcurrentDictionary<string, WiredDeviceData> sessionsByDeviceId = new ConcurrentDictionary<string, WiredDeviceData>();

    /// <summary>
    /// Same sessions, keyed by the clientID StreamManager knows them by
    /// </summary>
    private readonly ConcurrentDictionary<string, WiredDeviceData> sessionsByClientID = new ConcurrentDictionary<string, WiredDeviceData>();

    #endregion

    #region ADB

    /// <summary>
    /// Checks common SDK install location trying to allocate adb.exe
    /// </summary>
    private string FindAdbPath()
    {
        string localAppData = System.Environment.GetFolderPath(
            System.Environment.SpecialFolder.LocalApplicationData);

        string[] candidates = {
            System.IO.Path.Combine(
                System.Environment.GetEnvironmentVariable("ANDROID_HOME") ?? "",
                "platform-tools", "adb.exe"),
            System.IO.Path.Combine(
                System.Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT") ?? "",
                "platform-tools", "adb.exe"),
            System.IO.Path.Combine(localAppData, "Android", "Sdk", "platform-tools", "adb.exe"),
            @"C:\Android\sdk\platform-tools\adb.exe",
            @"C:\android-sdk\platform-tools\adb.exe",
        };

        foreach (string path in candidates)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
            {
                if (debug) UnityEngine.Debug.Log($"[ADBServer] adb found at: {path}");
                return path;
            }
        }

        if (debug) UnityEngine.Debug.LogError("[ADBServer] adb.exe not found. Install Android Studio or the Android SDK.");
        return null;
    }

    /// <summary>
    /// Execute adb commands and returns their output
    /// </summary>
    private string RunAdbCommand(string arguments)
    {
        var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = adbPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ADBServer] Failed to run 'adb {arguments}': {ex.Message}");
            return "";
        }

        if (error.Length > 0)
            UnityEngine.Debug.LogWarning($"[ADBServer] adb {arguments} -> stderr: {error}");

        return output.ToString();
    }

    #endregion

    #region Port administration

    /// <summary>
    /// Assigns an available port to a new client. If there is no room for more ports, throws exception
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private int AllocateLocalPort()
    {
        lock (usedLocalPorts)
        {
            for (int port = localPortRangeStart; port <= localPortRangeEnd; port++)
            {
                if (usedLocalPorts.Add(port))
                    return port;
            }
        }

        throw new Exception("[ADBServer] No free local ports left for adb reverse (range exhausted).");
    }

    /// <summary>
    /// Frees an used port
    /// </summary>
    /// <param name="port"></param>
    private void ReleaseLocalPort(int port)
    {
        lock (usedLocalPorts)
            usedLocalPorts.Remove(port);
    }

    #endregion

    #region Device watcher

    /// <summary>
    /// Processes "adb devices" output into a list of available devices (ignoring "unauthorized"/"offline" devices)
    /// </summary>
    private List<string> GetConnectedDeviceIds()
    {
        var ids = new List<string>();
        string output = RunAdbCommand("devices");
        if (string.IsNullOrEmpty(output)) return ids;

        foreach (string rawLine in output.Split('\n'))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("List of devices")) continue;

            string[] parts = line.Split('\t');
            if (parts.Length >= 2 && parts[1].Trim() == "device")
                ids.Add(parts[0].Trim());
        }

        return ids;
    }

    /// <summary>
    /// Background thread that executes "adb devices" repeteadly and reacts to changes in devices status
    /// </summary>
    private void DeviceWatcherLoop()
    {
        while (running)
        {
            try
            {
                var current = new HashSet<string>(GetConnectedDeviceIds());

                foreach (string id in current)
                    if (!sessionsByDeviceId.ContainsKey(id))
                        OnDeviceConnected(id);

                foreach (string id in new List<string>(sessionsByDeviceId.Keys))
                    if (!current.Contains(id))
                        OnDeviceDisconnected(id);
            }
            catch (Exception ex)
            {
                if (debug) UnityEngine.Debug.LogWarning($"[ADBServer] Device watcher error: {ex.Message}");
            }

            Thread.Sleep(devicePollIntervalMs);
        }
    }

    /// <summary>
    /// Reacts to a new device connection
    /// </summary>
    /// <param name="deviceId">Device connected</param>
    private void OnDeviceConnected(string deviceId)
    {
        if (!acceptsConnections) return;
        var session = new WiredDeviceData { deviceId = deviceId };

        try
        {
            // We assign a new port for this client and start the reverse tcp tunnel
            session.localPort = AllocateLocalPort();
            RunAdbCommand($"-s {deviceId} reverse tcp:{remotePort} tcp:{session.localPort}");
            session.listener = new TcpListener(IPAddress.Loopback, session.localPort);
            session.listener.Start();

            // In case the watcher loop double-fired for this device and it has already been handled
            if (!sessionsByDeviceId.TryAdd(deviceId, session))
            {
                session.listener.Stop();
                ReleaseLocalPort(session.localPort);
                return;
            }

            session.acceptThread = new Thread(() => AcceptLoop(session))
            {
                IsBackground = true,
                Name = $"ADB Accept {deviceId}"
            };
            session.acceptThread.Start();

            if (debug) UnityEngine.Debug.Log($"[ADBServer] Device connected: {deviceId} (tcp:{remotePort} -> tcp:{session.localPort})");
        }
        catch (Exception ex)
        {
            if (debug) UnityEngine.Debug.LogError($"[ADBServer] Failed to set up device {deviceId}: {ex.Message}");
            ReleaseLocalPort(session.localPort);
        }
    }

    /// <summary>
    /// Reacts to a device disconnection
    /// </summary>
    /// <param name="deviceId">Device disconnected</param>
    private void OnDeviceDisconnected(string deviceId)
    {
        if (!sessionsByDeviceId.TryRemove(deviceId, out WiredDeviceData session)) return;

        if (debug) UnityEngine.Debug.Log($"[ADBServer] Device disconnected: {deviceId}");

        try { session.listener?.Stop(); } catch { }
        try { session.tcpClient?.Close(); } catch { }

        // Even if the device is already disconnected this would fail silently (there is nothing left to un-reverse on its side)
        RunAdbCommand($"-s {deviceId} reverse --remove tcp:{remotePort}");
        ReleaseLocalPort(session.localPort);

        if (!string.IsNullOrEmpty(session.clientID))
        {
            sessionsByClientID.TryRemove(session.clientID, out _);
            string clientID = session.clientID;
            UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.RemovePeer(clientID));
        }
    }

    #endregion

    #region Communication

    /// <summary>
    /// Waits for the phone app to connect through this device's tunnel. 
    /// If the connection drops (app closed/crashed) it keeps listening in case the app reconnects (only while the device stays connected)
    /// </summary>
    private void AcceptLoop(WiredDeviceData session)
    {
        while (running && sessionsByDeviceId.ContainsKey(session.deviceId))
        {
            try
            {
                TcpClient tcp = session.listener.AcceptTcpClient();
                session.tcpClient = tcp;
                if (debug) UnityEngine.Debug.Log($"[ADBServer] App connected through ADB tunnel for device {session.deviceId}.");
                HandleClient(session, tcp);
            }
            catch (SocketException)
            {
                break; // listener was stopped (device disconnected or server stopping)
            }
            catch (Exception ex)
            {
                if (running && debug)
                    UnityEngine.Debug.LogWarning($"[ADBServer] Accept error for {session.deviceId}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Stablish connection between hosta nd client. First Handshake, then messages for WebRTC connection
    /// </summary>
    private void HandleClient(WiredDeviceData session, TcpClient tcp)
    {
        NetworkStream stream = tcp.GetStream();
        string clientID = "";

        try
        {
            if (!NetworkUtils.TryReadFramedMessage(stream, out string message))
            {
                if (debug) UnityEngine.Debug.Log($"[ADBServer] Empty/probe connection closed for {session.deviceId} (no handshake sent).");
                return;
            }

            ConnectionData decodedData = JsonUtility.FromJson<ConnectionData>(message);

            if (decodedData.connEvent != ConnectionEvent.HANDSHAKE)
            {
                UnityEngine.Debug.LogError("[ADBServer] Did not receive a valid handshake ConnectionData");
                return;
            }

            string clientIP = decodedData.ipAddress;
            if (!SendHandshake(stream, clientIP))
            {
                UnityEngine.Debug.LogError($"[ADBServer] Failed to send HANDSHAKE_ACK to {session.deviceId}");
                return;
            }

            clientID = Guid.NewGuid().ToString();
            session.clientID = clientID;
            sessionsByClientID.TryAdd(clientID, session);

            ClientData newClient = ClientData.ForADB(clientID);
            UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.CreatePeer(newClient));

            UnityEngine.Debug.Log($"[ADBServer] Client registered: {session.deviceId} (id: {clientID})");

            while (running)
            {
                if (!NetworkUtils.TryReadFramedMessage(stream, out string incoming)) break;

                var sigMsg = JsonUtility.FromJson<SignalingMessage>(incoming);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    StreamManager.Instance?.HandleIncomingSignaling(clientID, sigMsg));
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ADBServer] Exception handling client {session.deviceId}: {ex}");
        }
        finally
        {
            try { tcp.Close(); } catch { }
            session.tcpClient = null;

            if (!string.IsNullOrEmpty(clientID))
            {
                sessionsByClientID.TryRemove(clientID, out _);
                UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.RemovePeer(clientID));
            }

            session.clientID = null;
        }
    }

    /// <summary>
    /// Replies to the client's handshake with a HANDSHAKE_ACK. Returns whether it was sent successfully
    /// </summary>
    /// <param name="stream">Stream of the client's tunnel</param>
    /// <param name="clientIP">Client's IP</param>
    private bool SendHandshake(NetworkStream stream, string clientIP)
    {
        try
        {
            ConnectionData ack = ConnectionData.ForHandshake(clientIP, ClientConnectionType.ADB);
            NetworkUtils.WriteFramedMessage(stream, JsonUtility.ToJson(ack));
            return true;
        }
        catch (Exception ex)
        {
            if (debug) UnityEngine.Debug.LogWarning($"[ADBServer] Error sending HANDSHAKE_ACK: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Sends a signaling message to a specific client
    /// </summary>
    public bool SendMessage(string clientID, SignalingMessage msg)
    {
        if (!sessionsByClientID.TryGetValue(clientID, out WiredDeviceData session) || session.tcpClient == null)
            return false;

        try
        {
            NetworkStream stream = session.tcpClient.GetStream();
            NetworkUtils.WriteFramedMessage(stream, JsonUtility.ToJson(msg), syncRoot: stream);
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ADBServer] Error sending message to {clientID}: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Activation/Deactivation

    /// <summary>
    /// Starts the server, launching the background thread that watches for ADB devices
    /// </summary>
    public void StartServer()
    {
        if (running) return;

        if (string.IsNullOrEmpty(adbPath))
        {
            UnityEngine.Debug.LogError("[ADBServer] adb.exe not found; ADB connections will not be available.");
            return;
        }

        running = true;
        deviceWatcherThread = new Thread(DeviceWatcherLoop) { IsBackground = true, Name = "ADB Device Watcher" };
        deviceWatcherThread.Start();

        if (debug) UnityEngine.Debug.Log("[ADBServer] ADB server launched.");
    }

    /// <summary>
    /// Stops the server, ending the watcher thread and closing every active session
    /// </summary>
    public void StopServer()
    {
        if (!running) return;
        running = false;

        deviceWatcherThread?.Join(devicePollIntervalMs + 500);

        foreach (string deviceId in new List<string>(sessionsByDeviceId.Keys))
            OnDeviceDisconnected(deviceId);

        if (debug) UnityEngine.Debug.Log("[ADBServer] ADB server stopped.");
    }

    #endregion

    #region Monobehaviour

    private void Start()
    {
        adbPath = FindAdbPath();
    }

    void OnDestroy()
    {
        try { StopServer(); } catch { }
    }

    #endregion

}
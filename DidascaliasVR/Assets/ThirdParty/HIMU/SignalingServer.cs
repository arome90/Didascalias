using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Class incharge of establishing and handlening WiFi connections, making Unity take the role of the server in an signaling enviroment
/// </summary>
public class SignalingServer : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// Wether the server is running or not
    /// </summary>
    private bool running;

    /// <summary>
    /// Wether if the server is searching for new devieces or not
    /// </summary>
    public bool searchingDevices = true;

    /// <summary>
    /// Whether it accepts new connections or not
    /// </summary>
    public bool acceptsConnections = true;

    /// <summary>
    /// Port where the server will listen to upcoming network data
    /// </summary>
    private int listenPort = 7777;

    /// <summary>
    /// Port from where the multicast is going to be made
    /// </summary>
    private int multicastPort = 8053;

    /// <summary>
    /// Listener that waits for messages from the clients
    /// </summary>
    private TcpListener listener;

    /// <summary>
    /// Thread that listens for incoming messages
    /// </summary>
    private Thread listenThread;
    
    /// <summary>
    /// Clients structure.
    /// </summary>
    private readonly ConcurrentDictionary<string, TcpClient> tcpSockets = new ConcurrentDictionary<string, TcpClient>();

    /// <summary>
    /// Multicast IP group for specific broadcasting
    /// </summary>
    private const string MulticastGroup = "239.0.0.1";

    #endregion

    #region Activation/Deactivation

    /// <summary>
    /// Stops the TCP server.
    /// </summary>
    public void StopServer()
    {
        running = false;
        searchingDevices = false;

        foreach (var s in tcpSockets.Values)
            try { s.Close(); } catch { }
        tcpSockets.Clear();

        try { listener?.Stop(); } catch { }

        listenThread?.Join(500); // closes the thread within 500ms
        Debug.Log("[SignalingServer] TCP server stopped.");
    }

    /// <summary>
    /// Initializes the TCP server.
    /// </summary>
    public void StartServer()
    {
        running = true;

        listener = new TcpListener(IPAddress.Parse(NetworkUtils.GetIP()), listenPort);
        listener.Start();
        listenThread = new Thread(ListenLoop) { IsBackground = true, Name = "TCP Listen" };
        listenThread.Start(); 

        searchingDevices = true;
        StartCoroutine(SendMulticast()); 
        Debug.Log("[SignalingServer] TCP server launched.");
    }

    #endregion

    #region Connection discovery

    /// <summary>
    /// Sends a MULTICAST through the network for other devices to find this server
    /// </summary>
    /// <returns></returns>
    IEnumerator SendMulticast()
    {
        try
        {
            string ip = NetworkUtils.GetIP();
            ConnectionData connectionData = ConnectionData.ForMulticast(ip, listenPort);
            connectionData.sessionName = StreamManager.Instance.GetSessionName();
            connectionData.sessionID = StreamManager.Instance.GetSessionID();
            string json = JsonUtility.ToJson(connectionData);
            byte[] data = Encoding.UTF8.GetBytes(json);

            using (UdpClient sender = new UdpClient())
            {
                sender.Client.Bind(new IPEndPoint(IPAddress.Parse(ip), 0));
                sender.Ttl = 4;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(MulticastGroup), multicastPort);
                sender.Send(data, data.Length, endpoint);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SignalingServer] Error sending multicast: {e.Message}");
        }

        yield return new WaitForSeconds(2f);

        if (searchingDevices)
            StartCoroutine(SendMulticast());
    }

    /// <summary>
    /// Loop for listening to new clients and establishing their connection
    /// </summary>
    private void ListenLoop()
    {
        while (running)
        {
            try
            {
                if (!acceptsConnections) continue;
                TcpClient tcp = listener.AcceptTcpClient();
                Debug.Log($"[SignalingServer] TCP connection from: {((IPEndPoint)tcp.Client.RemoteEndPoint).Address}");

                // Each client gets its own thread for reading
                Thread clientThread = new Thread(() => HandleClient(tcp))
                {
                    IsBackground = true,
                    Name = "TCP Client"
                };
                clientThread.Start();
            }
            catch (SocketException)
            {
                // Thrown when listener.Stop() is called, expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalingServer] AcceptLoop error: {ex.Message}");
            }
        }
    }

    #endregion

    #region Communication

    /// <summary>
    /// Sends the signaling message of this project to the TCP server.
    /// </summary>
    /// <param name="clientID">ID of the client from where the message is going to be sent.</param>
    /// <param name="msg">Signaling message content.</param>
    public bool SendMessage(string clientID, SignalingMessage msg)
    {
        if (!tcpSockets.TryGetValue(clientID, out TcpClient tcp)) return false;

        try
        {
            NetworkStream stream = tcp.GetStream();
            NetworkUtils.WriteFramedMessage(stream, JsonUtility.ToJson(msg), syncRoot: stream);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SignalingServer] Error sending message to {clientID}: {ex.Message}");
            return false;
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
            ConnectionData ack = ConnectionData.ForHandshake(clientIP, ClientConnectionType.TCP);
            NetworkUtils.WriteFramedMessage(stream, JsonUtility.ToJson(ack));
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SignalingServer] Error sending HANDSHAKE_ACK: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Handles a client: handshake first, then the message reception loop
    /// </summary>
    /// <param name="tcp">Client's TCP connection</param>
    private void HandleClient(TcpClient tcp)
    {
        NetworkStream stream = tcp.GetStream();
        string clientID = "";

        try
        {
            // Handshake ---
            if (!NetworkUtils.TryReadFramedMessage(stream, out string message))
            {
                Debug.LogError("[SignalingServer] Client clossed connection before handsake.");
                return;
            }

            ConnectionData decodedData = JsonUtility.FromJson<ConnectionData>(message);

            // Check if the data recieved is truly a ConnectionData class
            if (decodedData.connEvent != ConnectionEvent.HANDSHAKE)
            {
                Debug.LogError("[SignalingServer] Not a Connection Data recieved during handshake.");
                return;
            }

            string clientIP = decodedData.ipAddress;
            // Confirm the handshake BEFORE registering the client as active
            if (!SendHandshake(stream, clientIP))
            {
                Debug.LogError($"[SignalingServer] Failed to send HANDSHAKE to {decodedData.ipAddress}");
                return;
            }
            clientID = Guid.NewGuid().ToString();

            ClientData newClient = ClientData.ForDevice(decodedData, clientID);
            tcpSockets.TryAdd(clientID, tcp);
            UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.CreatePeer(newClient));

            Debug.Log($"[SignalingServer] Client connected: {decodedData.ipAddress}");

            // Data process loop
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
            Debug.LogError("[SignalingServer] Exception thrown: " + ex.ToString());
            return;
        }
        finally
        {
            tcp.Close();

            if (!string.IsNullOrEmpty(clientID))
            {
                tcpSockets.TryRemove(clientID, out _);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    StreamManager.Instance?.RemovePeer(clientID));
            }
        }
    }

    #endregion

    #region Monobehaviour

    void OnDestroy()
    {
        if (!running) return;
        try { StopServer(); } catch { }
    }

    #endregion

}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.tvOS;

public class SignalingServer : MonoBehaviour {

    #region Variables

    bool running;

    int port = 8053;

    string ipAddress;

    TcpListener listener;

    Thread listenThread;

    int bufferSize;

    /// <summary>
    /// All currently connected clients, keyed by their IP.
    /// ConcurrentDictionary is used instead of Dictionary + lock because multiple background
    /// threads (one per client) may add or remove entries simultaneously. All individual
    /// operations (TryAdd, TryRemove, TryGetValue) are atomic, and its enumerator works on
    /// a snapshot so Broadcast iteration is safe without an external lock.
    /// </summary>
    readonly ConcurrentDictionary<string, ClientWebRTC> clients = new ConcurrentDictionary<string, ClientWebRTC>();

    #endregion

    #region Conection
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateInstance()
    {
        var obj = new GameObject("SignalingServer");
        obj.AddComponent<SignalingServer>();
        UnityEngine.Object.DontDestroyOnLoad(obj);
    }

    private void StartServer()
    {
        running = true;
        GetIpAddress();
        InvokeRepeating(nameof(SendBroadcast), 0f, 2f);
        
        listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        listener.Start();
        listenThread = new Thread(ListenLoop) { IsBackground = true, Name = "TCP Listen" };
        listenThread.Start();
    }

    private void ListenLoop()
    {
        while (running)
        {
            try
            {
                TcpClient tcp = listener.AcceptTcpClient();

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
                // Thrown when listener.Stop() is called — expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalingServer] AcceptLoop error: {ex.Message}");
            }
        }
    }

    private void HandleClient(TcpClient tcp)
    {
        NetworkStream stream = tcp.GetStream();

        try
        {
            // Handshake ---
            byte[] data = new byte[bufferSize];
            int bytesRead = stream.Read(data, 0, data.Length);
            string message = Encoding.UTF8.GetString(data, 0, bytesRead);

            ConnectionData decodedData = JsonUtility.FromJson<ConnectionData>(message);

            // Check if the data recieved is truly a ConnectionData class
            if (decodedData.connEvent != ConnectionEvent.HANDSHAKE)
            {
                Debug.LogError("[Signaling Server] Not a Connection Data recieved during Handshake.");
                return;
            }

            ClientWebRTC newClient = new ClientWebRTC(decodedData, stream);
            clients.TryAdd(decodedData.ipAddress, newClient);

            Debug.Log($"[SignalingServer] Client connected: {decodedData.ipAddress}");
        }
        catch (Exception ex)
        {
            Debug.LogError("[Signaling Server] Exception thrown: " + ex.ToString());
            return;
        }
    }

    void SendBroadcast()
    {
        //if (mobileConnected) return; // no envía si ya hay conexión

        try
        {
            string json = JsonUtility.ToJson(new ConnectionData(ipAddress, port, ConnectionEvent.BROADCAST));
            byte[] data = Encoding.UTF8.GetBytes(json);

            using (var sender = new UdpClient())
            {
                sender.EnableBroadcast = true;
                var endpoint = new IPEndPoint(IPAddress.Broadcast, port);
                sender.Send(data, data.Length, endpoint);
            }

            UnityEngine.Debug.Log($"[Host] Broadcast enviado -> {json}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"[Host] Error al enviar broadcast: {e.Message}");
        }
    }

    private void GetIpAddress()
    {
        ipAddress = "No disponible";
        try
        {
            foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ipAddress = ip.ToString();
        }
        catch (System.Exception e) {
            Debug.LogError(e);
        }
    }

    #endregion

    public void Start()
    {
        bufferSize = 1024;
        StartServer();
    }
}

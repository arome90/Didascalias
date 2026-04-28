using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.tvOS;

public class SignalingServer : MonoBehaviour {

    #region Variables

    bool running;
    bool searchingDevices;

    int listenPort = 443;
    int broadcastPort = 8053;

    string ipAddress;

    TcpListener listener;

    Thread listenThread;

    int bufferSize;

    #endregion

    #region Conection

    private void StartServer()
    {
        running = true;
        GetIpAddress();
        
        listener = new TcpListener(IPAddress.Parse(ipAddress), listenPort);
        listener.Start();
        listenThread = new Thread(ListenLoop) { IsBackground = true, Name = "TCP Listen" };
        listenThread.Start();

        //InvokeRepeating(nameof(SendBroadcast), 0f, 2f);
        searchingDevices = true;
        StartCoroutine(SendBroadcast());
    }

    IEnumerator SendBroadcast()
    {
        try
        {
            string json = JsonUtility.ToJson(new ConnectionData(ipAddress, listenPort, ConnectionEvent.BROADCAST));
            byte[] data = Encoding.UTF8.GetBytes(json);

            using (var sender = new UdpClient())
            {
                sender.EnableBroadcast = true;
                var endpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);
                sender.Send(data, data.Length, endpoint);
            }

            UnityEngine.Debug.Log($"[Host] Broadcast enviado -> {json}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogWarning($"[Host] Error al enviar broadcast: {e.Message}");
        }

        yield return new WaitForSeconds(2f);

        if (searchingDevices)
            StartCoroutine(SendBroadcast());
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
            StreamManager.Instance?.addClient(decodedData.ipAddress, newClient);

            Debug.Log($"[SignalingServer] Client connected: {decodedData.ipAddress}");

            // Data process loop ---
            while (running)
            {
                int bytes = stream.Read(data, 0, data.Length);
                if (bytes == 0) break;
                 
                string incoming = Encoding.UTF8.GetString(data, 0, bytes);
                // Procesar mensaje...
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Signaling Server] Exception thrown: " + ex.ToString());
            return;
        }
    }

    #endregion

    #region Getters

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

    #region Monobehaviour

    /// <summary>
    /// Creates an GameObject and attaches this component to ensure it is initialized
    /// before any scene is loaded. The object is marked as DontDestroyOnLoad so it
    /// persists across scene transitions.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void CreateInstance()
    {
        var obj = new GameObject("SignalingServer");
        obj.AddComponent<SignalingServer>();
        DontDestroyOnLoad(obj);
    }

    public void Start()
    {
        bufferSize = 1024;
        StartServer();
    }

    #endregion
}

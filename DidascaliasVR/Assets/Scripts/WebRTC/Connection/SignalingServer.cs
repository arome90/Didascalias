using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.WebRTC;
using UnityEngine;

public class SignalingServer : MonoBehaviour {

    #region Variables
    /// <summary>
    /// Wether if the server is running or not
    /// </summary>
    bool running;

    /// <summary>
    /// Wether if the server is searching for new devieces or not
    /// </summary>
    bool searchingDevices;

    /// <summary>
    /// Port where the server will listen to upcoming network data
    /// </summary>
    int listenPort = 7777;

    /// <summary>
    /// Port from where the broadcast is going to be made
    /// </summary>
    int broadcastPort = 8053;

    /// <summary>
    /// The IP address of the server
    /// </summary>
    public static string ipAddress { get; private set; }

    TcpListener listener;

    Thread listenThread;

    int bufferSize;

    /// <summary>
    /// Multicast IP group for specific broadcasting
    /// </summary>
    private const string MulticastGroup = "239.0.0.1";

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

            using (UdpClient sender = new UdpClient())
            {
                sender.Client.Bind(new IPEndPoint(IPAddress.Parse(ipAddress), 0));
                sender.Ttl = 4;
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(MulticastGroup), broadcastPort);
                sender.Send(data, data.Length, endpoint);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Host] Error al enviar multicast: {e.Message}");
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
                Debug.Log($"[Server] TCP connection from: {((IPEndPoint)tcp.Client.RemoteEndPoint).Address}");

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
            if (decodedData.connType != ConnectionEvent.HANDSHAKE)
            {
                Debug.LogError("[Signaling Server] Not a Connection Data recieved during Handshake.");
                return;
            }

            ClientData newClient = new ClientData(decodedData, stream);
            UnityMainThreadDispatcher.Instance().Enqueue(() => StreamManager.Instance?.CreatePeerForClient(newClient));
            
            Debug.Log($"[SignalingServer] Client connected: {decodedData.ipAddress}");

            // Data process loop ---
            while (running)
            {
                // Leer header de 4 bytes con el tamaño del mensaje
                byte[] header = new byte[4];
                int headerBytes = stream.Read(header, 0, 4);
                if (headerBytes == 0) break;

                int size = BitConverter.ToInt32(header, 0);
                byte[] body = new byte[size];
                int total = 0;
                while (total < size)
                    total += stream.Read(body, total, size - total);

                string incoming = Encoding.UTF8.GetString(body);
                var sigMsg = JsonUtility.FromJson<SignalingMessage>(incoming);

                // Ejecutar en el hilo principal de Unity (los peers WebRTC lo necesitan)
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    StreamManager.Instance?.HandleIncomingSignaling(decodedData.ipAddress, sigMsg));
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
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

                // Excluir adaptadores virtuales (VirtualBox, VMware, Hyper-V, etc.)
                string name = ni.Name.ToLower();
                string desc = ni.Description.ToLower();
                if (name.Contains("virtual") || desc.Contains("virtual") ||
                    name.Contains("vmware") || desc.Contains("vmware") ||
                    name.Contains("vbox") || desc.Contains("vbox")) continue;

                IPInterfaceProperties props = ni.GetIPProperties();
                if (props.GatewayAddresses.Count == 0) continue;

                foreach (UnicastIPAddressInformation addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    ipAddress = addr.Address.ToString();
                    Debug.Log($"[Network] Adaptador: {ni.Name} — IP: {ipAddress}");
                    return;
                }
            }
#elif UNITY_ANDROID
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect(MulticastGroup, 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ipAddress = endPoint.Address.ToString();
            }
            Debug.Log($"[Network] IP seleccionada: {ipAddress}");
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Error obteniendo IP: {e}");
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
        StartCoroutine(WebRTC.Update());
        StartServer();
        StreamManager.Instance?.ConnectToNode();
    }

    void OnDestroy()
    {
        running = false;
        searchingDevices = false;

        try { listener?.Stop(); } catch { }

        listenThread?.Join(500); // espera max 500ms a que el hilo termine
    }

    #endregion
}

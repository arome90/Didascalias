using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// Callback structure created for methods that return/create gameobjects for clients
/// </summary>
/// <param name="client"></param>
/// <returns>Gameobject that represents the new client in project</returns>
public delegate GameObject CreateClient(ClientData client);

/// <summary>
/// Callback structure created for methods that return/create textures for clients
/// </summary>
/// <returns>Texture to be streamed to new client</returns>
public delegate RenderTexture TextureAssignmentCallback();

/// <summary>
/// Orquestates 'streaming to other devices' feature. Each type (WebSocket, TCP, ADB) has its own component, flags, callbacks
/// </summary>
public class StreamManager : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// Instance of StreamManager (Singleton)
    /// </summary>
    public static StreamManager Instance { get; private set; }

    /// <summary>
    /// Defines whether we print information or not
    /// </summary>
    [SerializeField]
    private bool debug = false;

    /// <summary>
    /// Defines whether this scripts object persists between scenes or not
    /// </summary>
    [SerializeField]
    private bool shouldPersist = false;

    /// <summary>
    /// Name of this game session 
    /// </summary>
    [SerializeField]
    private string sessionName = "Placeholder";

    /// <summary>
    /// ID of this game session, used to differ from other games that could be playing in the 
    /// same network
    /// </summary>
    private int sessionID = 1234;

    /// <summary>
    /// All currently connected clients, keyed by their clientID (GUID for TCP clients,
    /// session key for browser clients).
    /// ConcurrentDictionary is used instead of Dictionary + lock because multiple background
    /// threads (one per client) may add or remove entries simultaneously
    /// </summary>
    readonly ConcurrentDictionary<string, ClientData> clients = new ConcurrentDictionary<string, ClientData>();

    /// <summary>
    /// Server that works through a WebSocket. It connects to an external siganling server.
    /// </summary>
    private WebSocketServerRTC webSocketServer;

    /// <summary>
    /// An embeded Signaling Server in the game.
    /// </summary>
    private SignalingServer signalingServer;

    /// <summary>
    /// Handles phones connected by USB cable through ADB.
    /// </summary>
    private ADBConnectionServer adbServer;

    /// <summary>
    /// Flag that allows WebSocket connections (browser)
    /// </summary>
    [SerializeField]
    private bool acceptWebSocketConnection = true;

    /// <summary>
    /// Indicates if WebSocket connections feature is activated or not
    /// </summary>
    private bool webSocketConnectionOn = false;

    /// <summary>
    /// Flag that allows TCP connections (devices)
    /// </summary>
    [SerializeField]
    private bool acceptTCPConnection = true;

    /// <summary>
    /// Indicates if TCP connections feature is activated or not
    /// </summary>
    private bool tcpConnectionOn = false;

    /// <summary>
    /// Flag that allows USB connections (cabled devices)
    /// </summary>
    [SerializeField]
    private bool acceptADBConnection = true;

    /// <summary>
    /// Indicates if ADB connections feature is activated or not
    /// </summary>
    private bool adbConnectionOn = false;

    /// <summary>
    /// Frame's width
    /// </summary>
    [SerializeField]
    private uint streamFrameWidth = 1280;

    /// <summary>
    /// Frame's heigth
    /// </summary>
    [SerializeField]
    private uint streamFrameHeight = 720;

    /// <summary>
    /// Frame's depth
    /// </summary>
    [SerializeField]
    private uint streamFrameDepth = 24;

    /// <summary>
    /// Callback that is executed to retrieve/create texture for WebSocket clients
    /// </summary>
    private TextureAssignmentCallback browserTextureCallback;
    /// <summary>
    /// Callback that is executed to retrieve/create texture for TCP clients
    /// </summary>
    private TextureAssignmentCallback tcpTextureCallback;
    /// <summary>
    /// Callback that is executed to retrieve/create texture for ADB clients
    /// </summary>
    private TextureAssignmentCallback adbTextureCallback;
    /// <summary>
    /// Callback that is executed to retrieve/create client gameobject for WebSocket clients
    /// </summary>
    private CreateClient browserClientCallback;
    /// <summary>
    /// Callback that is executed to retrieve/create client gameobject for TCP clients
    /// </summary>
    private CreateClient tcpClientCallback;
    /// <summary>
    /// Callback that is executed to retrieve/create client gameobject for ADB clients
    /// </summary>
    private CreateClient adbClientCallback;

    #endregion

    #region SharedMethods

    /// <summary>
    /// Creates the client object and completes the WebRTC connection exchange
    /// </summary>
    /// <param name="client"></param>
    public void CreatePeer(ClientData client)
    {
        string clientID = client.clientID;
        if (!clients.TryAdd(clientID, client)) return;

        GameObject go;
        TextureAssignmentCallback textureCallback;
        HIMUClient peer;
        switch (client.type)
        {
            case ClientConnectionType.WEB_SOCKET:
                {
                    go = browserClientCallback(client);
                    textureCallback = browserTextureCallback;
                    peer = go.AddComponent<HIMUClient>(); 
                    peer.Initialize(clientID, textureCallback(), msg => _ = webSocketServer.SendToNode(clientID, msg), true);
                }
                break;
            case ClientConnectionType.TCP:
                {
                    go = tcpClientCallback(client);
                    textureCallback = tcpTextureCallback;
                    peer = go.AddComponent<HIMUClient>(); 
                    peer.Initialize(clientID, textureCallback(), msg => signalingServer.SendMessage(clientID, msg), true);
                }
                break;
            case ClientConnectionType.ADB:
                {
                    go = adbClientCallback(client);
                    textureCallback = adbTextureCallback;
                    peer = go.AddComponent<HIMUClient>(); 
                    peer.Initialize(clientID, textureCallback(), msg => adbServer.SendMessage(clientID, msg), true);
                }
                break;
            default:
                {
                    go = BaseCreateClient(client);
                    textureCallback = BaseCreateTexture;
                    peer = go.AddComponent<HIMUClient>();
                    peer.Initialize(clientID, textureCallback(), msg => signalingServer.SendMessage(clientID, msg), true);
                }
                break;
        }
        clients[clientID].himuClient = peer;
        StartCoroutine(peer.CreateOffer());
    }

    /// <summary>
    /// Tries to remove a client given its clientID
    /// </summary>
    /// <param name="clientID"></param>
    public void RemovePeer(string clientID)
    {
        if (!clients.TryRemove(clientID, out var data))
        {
            if (debug) Debug.LogWarning($"[StreamManager] Tried to remove unknown peer: {clientID}");
            return;
        }

        if (data.himuClient != null)
            Destroy(data.himuClient.gameObject);

        if (HostInputManager.Instance != null)
            HostInputManager.Instance.RemoveClient(clientID);

        if (debug) Debug.Log($"[StreamManager] Destroyed {data.type} peer: {clientID}");
    }

    /// <summary>
    /// Processes the signaling message from the server
    /// </summary>
    /// <param name="clientID"></param>
    /// <param name="msg"></param>
    public void HandleIncomingSignaling(string clientID, SignalingMessage msg)
    {
        if (!clients.TryGetValue(clientID, out var peer)) return;

        if (peer.himuClient == null)
        {
            if (debug) Debug.LogWarning($"[StreamManager] Signaling {msg.type} for {clientID} arrived before its HIMUClient existed. Dropping.");
            return;
        }

        if (string.IsNullOrEmpty(msg.body))
        {
            if (debug) Debug.LogWarning($"[StreamManager] Signaling {msg.type} for {clientID} arrived with an empty body. Dropping.");
            return;
        }

        if (msg.type == ConnectionEvent.ICE)
        {
            IceCandidateData data = JsonUtility.FromJson<IceCandidateData>(msg.body);
            if (data == null)
            {
                if (debug) Debug.LogWarning($"[StreamManager] Signaling {msg.type} for {clientID} couldn't be converted to a valid JSON format. Dropping.");
                return;
            }

            RTCIceCandidateInit init = new RTCIceCandidateInit
            {
                candidate = data.candidate,
                sdpMid = data.sdpMid,
                sdpMLineIndex = data.sdpMLineIndex
            };
            peer.himuClient.AddIceCandidate(init);
        }
        else if (msg.type == ConnectionEvent.SDP)
        {
            SessionDescriptionData data = JsonUtility.FromJson<SessionDescriptionData>(msg.body);
            if (data == null)
            {
                if (debug) Debug.LogWarning($"[StreamManager] Signaling {msg.type} for {clientID} couldn't be converted to a valid JSON format. Dropping.");
                return;
            }

            RTCSessionDescription answer = data.ToRTCDesc();
            StartCoroutine(peer.himuClient.SetRemoteAnswer(answer));
        }
        else if (msg.type == ConnectionEvent.DISCONNECT)
        {
            RemovePeer(clientID);
        }
        else
        {
            if (debug) Debug.LogWarning($"[StreamManager] Unhandled signaling type " + msg.type + " from " + clientID + ".");
        }
    }

    #endregion

    #region FlaggedMethods

    /// <summary>
    /// Starts or stops the WebSocket server wether its checkbox is checked
    /// </summary>
    public async void FlagWebSocketServer()
    {
        if (!acceptWebSocketConnection) return;
        if (webSocketConnectionOn)
        {
            if (debug) Debug.Log("[StreamManager] Stopping WebSocket server (Node).");
            await webSocketServer.DisconnectToNode();
            webSocketServer.StopServer();
            webSocketConnectionOn = false;
        }
        else
        {
            if (debug) Debug.Log("[StreamManager] Launching WebSocket server (Node).");
            webSocketServer.LaunchServer();
            webSocketServer.ConnectToNode();
            webSocketConnectionOn = true;
        }
    }

    /// <summary>
    /// Starts or stops the TCP signaling server wether its checkbox is checked
    /// </summary>
    public void FlagSignalingServer()
    {
        if (!acceptTCPConnection) return;
        if (tcpConnectionOn)
        {
            if (debug) Debug.Log("[StreamManager] Stopping TCP signaling server.");
            signalingServer.StopServer();
            tcpConnectionOn = false;
        }
        else
        {
            if (debug) Debug.Log("[StreamManager] Launching TCP signaling server.");
            signalingServer.StartServer();
            tcpConnectionOn = true;
        }
    }

    /// <summary>
    /// Starts or stops the WebSocket server wether its checkbox is checked
    /// </summary>
    public void FlagADBConnection()
    {
        if (!acceptADBConnection) return;
        if (adbConnectionOn)
        {
            if (debug) Debug.Log("[StreamManager] Stopping ADB connection.");
            adbServer.StopServer();
            adbConnectionOn = false;
        }
        else
        {
            if (debug) Debug.Log("[StreamManager] Launching ADB connection.");
            adbServer.StartServer();
            adbConnectionOn = true;
        }
    }

    #endregion

    #region Getters & Setters

    /// <summary>
    /// Returns the session name
    /// </summary>
    /// <returns></returns>
    public string GetSessionName()
    {
        return sessionName;
    }

    /// <summary>
    /// Retruns the session id
    /// </summary>
    /// <returns></returns>
    public int GetSessionID()
    {
        return sessionID;
    }

    /// <summary>
    /// Returns the URL to the Node signaling server
    /// </summary>
    /// <returns></returns>
    public string GetNodeServerData()
    {
        return webSocketServer.GetNodeHost() + ":" + webSocketServer.GetBrowserPort().ToString();
    }

    /// <summary>
    /// Copies the current clients to the returned list.
    /// </summary>
    /// <returns>Copy of the clients list.</returns>
    public List<ClientData> GetClients()
    {
        List<ClientData> copy = new List<ClientData>();
        foreach (var client in clients)
            copy.Add(client.Value);

        return copy;
    }

    /// <summary>
    /// Copies the only the current WEB_SOCKET clients to the returned list.
    /// </summary>
    /// <returns>Copy of the WEB_SOCKET clients list.</returns>
    public List<ClientData> GetBrowserClients()
    {
        List<ClientData> browserClients = new List<ClientData>();
        foreach(var client in clients.Where(c => c.Value.type == ClientConnectionType.WEB_SOCKET))
        {
            browserClients.Add(client.Value);
        }
        return browserClients;
    }

    /// <summary>
    /// Copies the only the current TCP clients to the returned list.
    /// </summary>
    /// <returns>Copy of the TCP clients list.</returns>
    public List<ClientData> GetTCPClients()
    {
        List<ClientData> tcpClients = new List<ClientData>();
        foreach (var client in clients.Where(c => c.Value.type == ClientConnectionType.TCP))
        {
            tcpClients.Add(client.Value);
        }
        return tcpClients;
    }

    /// <summary>
    /// Copies the only the current ADB clients to the returned list.
    /// </summary>
    /// <returns>Copy of the ADB clients list.</returns>
    public List<ClientData> GetADBClients()
    {
        List<ClientData> adbClients = new List<ClientData>();
        foreach (var client in clients.Where(c => c.Value.type == ClientConnectionType.ADB))
        {
            adbClients.Add(client.Value);
        }
        return adbClients;
    }

    public CreateClient GetBrowserClientCallback()
    {
        return browserClientCallback;
    }

    public void SetBrowserClientCallback(CreateClient newCallback)
    {
        browserClientCallback = newCallback;
    }

    public TextureAssignmentCallback GetBrowserTextureCallback()
    {
        return browserTextureCallback;
    }

    public void SetBrowserTextureCallback(TextureAssignmentCallback newCallback)
    {
        browserTextureCallback = newCallback;
    }

    public void FlagBrowserAcceptConnections(bool active)
    {
        if (webSocketServer == null) return;
        webSocketServer.acceptsConnections = active;
    }

    public CreateClient GetTCPClientCallback()
    {
        return tcpClientCallback;
    }

    public void SetTCPClientCallback(CreateClient newCallback)
    {
        tcpClientCallback = newCallback;
    }

    public TextureAssignmentCallback GetTCPTextureCallback()
    {
        return tcpTextureCallback;
    }

    public void SetTCPTextureCallback(TextureAssignmentCallback newCallback)
    {
        tcpTextureCallback = newCallback;
    }

    public void FlagTCPAcceptConnections(bool active)
    {
        if (signalingServer == null) return;
        signalingServer.acceptsConnections = active;
        signalingServer.searchingDevices = active;
    }

    public CreateClient GetADBClientCallback()
    {
        return adbClientCallback;
    }

    public void SetADBClientCallback(CreateClient newCallback)
    {
        adbClientCallback = newCallback;
    }

    public TextureAssignmentCallback GetADBTextureCallback()
    {
        return adbTextureCallback;
    }

    public void SetADBTextureCallback(TextureAssignmentCallback newCallback)
    {
        adbTextureCallback = newCallback;
    }

    public void FlagADBAcceptConnections(bool active)
    {
        if (adbServer == null) return;
        adbServer.acceptsConnections = active;
    }

    #endregion

    #region BaseMethods

    /// <summary>
    /// Basic method for clients gameobject creation
    /// </summary>
    /// <param name="client"></param>
    /// <returns>Gameobject that represents the new client</returns>
    private GameObject BaseCreateClient(ClientData client)
    {
        switch(client.type)
        {
            case ClientConnectionType.WEB_SOCKET: return new GameObject($"{client.type.ToString()}-Bsw-Peer_{client.clientID}");
            case ClientConnectionType.TCP: return new GameObject($"{client.type.ToString()}-Dvc-Peer_{client.clientID}");
            default: return new GameObject($"{client.type.ToString()}-Adb-Peer_{client.clientID}");
        }
    }

    /// <summary>
    /// Basic method for clients texture creation
    /// </summary>
    /// <returns>Texturer to be streamed to new client</returns>
    private RenderTexture BaseCreateTexture()
    {
        RenderTexture rt;
        rt = new RenderTexture((int)streamFrameWidth, (int)streamFrameHeight, (int)streamFrameDepth, RenderTextureFormat.BGRA32);
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.antiAliasing = 1;
        rt.Create();
        return rt;
    }

    #endregion

    #region Monobehaviour

    void Awake()
    {
        if (Instance) {
            Instance.gameObject.SetActive(false);
            Destroy(Instance.gameObject);
        }
        Instance = this;
        if (shouldPersist) DontDestroyOnLoad(gameObject);

        StartCoroutine(WebRTC.Update());
        NetworkUtils.GetIP();

        System.Random rnd = new System.Random();
        sessionID = rnd.Next(1000, 10000);

        if (acceptWebSocketConnection)
        {
            browserClientCallback = BaseCreateClient;
            browserTextureCallback = BaseCreateTexture;
            webSocketServer = gameObject.AddComponent<WebSocketServerRTC>();
        }
        if (acceptTCPConnection)
        {
            tcpClientCallback = BaseCreateClient;
            tcpTextureCallback = BaseCreateTexture;
            signalingServer = gameObject.AddComponent<SignalingServer>();
        }
        if (acceptADBConnection)
        {
            adbClientCallback = BaseCreateClient;
            adbTextureCallback = BaseCreateTexture;
            adbServer = gameObject.AddComponent<ADBConnectionServer>();
        }
    }

    #endregion

}
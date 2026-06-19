using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using TMPro;
using Unity.Android.Gradle;
using Unity.WebRTC;
using UnityEditor.PackageManager;
using UnityEngine;
using WebSocketSharp.Server;

public class StreamManager : MonoBehaviour
{

    #region Variables
    /// <summary>
    /// Instance of StreamManager (Singleton)
    /// </summary>
    public static StreamManager Instance { get; private set; }

    /// <summary>
    /// All currently connected clients, keyed by their IP.
    /// ConcurrentDictionary is used instead of Dictionary + lock because multiple background
    /// threads (one per client) may add or remove entries simultaneously. All individual
    /// operations (TryAdd, TryRemove, TryGetValue) are atomic, and its enumerator works on
    /// a snapshot so Broadcast iteration is safe without an external lock.
    /// </summary>
    readonly ConcurrentDictionary<string, ClientData> clients = new ConcurrentDictionary<string, ClientData>();
    
    /// <summary>
    /// URP Camera attached to the VR Camera in order to stream the frames rendered without the
    /// perks of VR.
    /// </summary>
    [SerializeField]
    private GameObject VRCameraObject;

    /// <summary>
    /// Server that works through a WebSocket. It connects to an external siganling server.
    /// </summary>
    WebSocketServerRTC webSocketServer;

    /// <summary>
    /// An embeded Signaling Server in the game.
    /// </summary>
    SignalingServer signalingServer;

    /// <summary>
    /// Component for connection visual representation
    /// </summary>
    UIConnectionComponent connectionUI;

    /// <summary>
    /// Frame's width
    /// </summary>
    [SerializeField]
    uint frameWidth = 1280;

    /// <summary>
    /// Frame's heigth
    /// </summary>
    [SerializeField]
    uint frameHeight = 720;

    /// <summary>
    /// Frame's depth
    /// </summary>
    [SerializeField]
    uint frameDepth = 24;
    #endregion

    #region Methods
    public void SetUIComponent(UIConnectionComponent ui)
    {
        connectionUI = ui;
    }

    public void CreateSignalingServer()
    {
        GameObject obj = new GameObject("SignalingServer");
        signalingServer = obj.AddComponent<SignalingServer>();
        DontDestroyOnLoad(obj);
    }

    private void CreateWebSocketServer()
    {
        GameObject obj = new GameObject("WebSocketServer");
        webSocketServer = obj.AddComponent<WebSocketServerRTC>();
        DontDestroyOnLoad(obj);
    }
    #endregion

    #region SharedMethods

    private void CreateAnchoredCamera(string ip, ref RenderTexture rt)
    {
        GameObject streamGo = new GameObject($"StreamCamera-Peer_{ip}");
        streamGo.transform.position = VRCameraObject.transform.position;
        streamGo.transform.rotation = VRCameraObject.transform.rotation;
        streamGo.transform.SetParent(VRCameraObject.transform);
        Camera cam = streamGo.AddComponent<Camera>();
        cam.targetTexture = rt;
        //cam.enabled = false -> TO-DO: que se active o desactive segun si es Streamer o Player;
    }

    /// <summary>
    /// Adds a client to the dictionary
    /// </summary>
    /// <param name="str">IP of the client</param>
    /// <param name="client">Client data</param>
    public bool addClient(string ip, ClientData client)
    {
        return clients.TryAdd(ip, client);
    }

    /// <summary>
    /// Removes a client form the dictionary
    /// </summary>
    /// <param name="str">IP of the client</param>
    public bool removeClient(string ip)
    {
        return clients.TryRemove(ip, out var client);
    }

    #endregion

    #region WebSocket
    // Creacion de objeto en escena que representa un cliente Navegador
    public void CreatePeerForBrowser(ClientData client)
    {
        // Si ese navegador ya esta conectado, se ignora
        string ip = client.ipAddress;
        if (!addClient(ip, client)) return;

        GameObject go = new GameObject($"{client.type.ToString()}-Peer_{ip}");
        WebRTCPeer peer = go.AddComponent<WebRTCPeer>();

        RenderTexture rt;
        rt = new RenderTexture((int)frameWidth, (int)frameHeight, (int)frameDepth, RenderTextureFormat.BGRA32);
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.antiAliasing = 1;
        rt.Create();
        CreateAnchoredCamera(ip, ref rt);
        
        peer.Initialize(ip, rt, msg => webSocketServer.SendToNode(msg, ip));
        StartCoroutine(peer.CreateOffer());
        clients[ip].webRtcPeer = peer;

        Debug.Log($"[StreamManager] Created browser peer: {ip}");
    }

    #endregion

    #region TCP

    /// <summary>
    /// Creates the client object and completes the WebRTC connection exchange
    /// </summary>
    /// <param name="ip">IP of the client</param>
    public void CreatePeerForClient(ClientData client)
    {
        // Add client to the dictionary
        string ip = client.ipAddress;
        if (!addClient(ip, client)) return;

        // Create GameObject
        GameObject go = new GameObject($"{client.type.ToString()}-Peer_{ip}");
        go.GetComponent<Transform>().position = Vector3.zero;

        RenderTexture rt;
        rt = new RenderTexture((int)frameWidth, (int)frameHeight, (int)frameDepth, RenderTextureFormat.BGRA32);
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.antiAliasing = 1;
        rt.Create();
        // Player
        if (client.type == ClientType.PLAYER)
        {
            Camera cam = go.AddComponent<Camera>();
            cam.targetTexture = rt;
            go.AddComponent<PeerMovementComponent>();
        }
        // Streaming
        else
        {
            CreateAnchoredCamera(ip, ref rt);
        }

        // Create RTC connection Peer
        WebRTCPeer peer = go.AddComponent<WebRTCPeer>();
        peer.Initialize(ip, rt, msg => SendSignalingMessage(ip, msg));
        clients[ip].webRtcPeer = peer;
        StartCoroutine(peer.CreateOffer());

        connectionUI?.CreateUIRepresentation(ip);

        Debug.Log($"[StreamManager] Created device peer: {ip}");
    }

    void SendSignalingMessage(string ip, SignalingMessage msg)
    {
        if (!clients.TryGetValue(ip, out var client)) return;
        string json = JsonUtility.ToJson(msg);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        byte[] header = System.BitConverter.GetBytes(data.Length);
        client.stream.Write(header, 0, 4);
        client.stream.Write(data, 0, data.Length);
        client.stream.Flush();
    }

    public void HandleIncomingSignaling(string fromIp, SignalingMessage msg)
    {
        if (!clients.TryGetValue(fromIp, out var peer)) return;

        if (msg.type == ConnectionEvent.ICE)
        {
            IceCandidateData data = JsonUtility.FromJson<IceCandidateData>(msg.body);
            RTCIceCandidateInit init = new RTCIceCandidateInit
            {
                candidate = data.candidate,
                sdpMid = data.sdpMid,
                sdpMLineIndex = data.sdpMLineIndex
            };
            peer.webRtcPeer.AddIceCandidate(init);
        }
        else if (msg.type == ConnectionEvent.SDP)
        {
            SessionDescriptionData data = JsonUtility.FromJson<SessionDescriptionData>(msg.body);
            RTCSessionDescription answer = data.ToRTCDesc();
            StartCoroutine(peer.webRtcPeer.SetRemoteAnswer(answer));
        }
        else if (msg.type == ConnectionEvent.DISCONNECT)
        {
            Destroy(peer.webRtcPeer.gameObject);
            removeClient(peer.ipAddress);
            Debug.Log($"[StreamManager] Removed peer: {peer.ipAddress}");
        }
    }
    #endregion

    #region Monobehaviour
    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        CreateWebSocketServer();
    }
    #endregion
}

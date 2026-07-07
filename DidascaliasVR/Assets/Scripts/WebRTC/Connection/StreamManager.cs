using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using TMPro;
using Unity.Android.Gradle;
using Unity.WebRTC;
using Unity.XR.CoreUtils;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private RenderTexture streamingTexture;

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
        if (!signalingServer)
        {
            GameObject obj = new GameObject("SignalingServer");
            signalingServer = obj.AddComponent<SignalingServer>();
            DontDestroyOnLoad(obj);
        }
        else Debug.LogError("[StreamManager] Ya existe un SignalingServer en la escena.");
    }

    public void CreateWebSocketServer()
    {
        if (!webSocketServer)
        {
            GameObject obj = new GameObject("WebSocketServer");
            webSocketServer = obj.AddComponent<WebSocketServerRTC>();
            DontDestroyOnLoad(obj);
        }
        else Debug.LogError("[StreamManager] Ya existe un WebSocketServer en la escena.");   
    }
    #endregion

    #region SharedMethods

    private void CreateAnchoredCamera(string ip)
    {
        GameObject streamGo = new GameObject($"StreamCamera-Peer_{ip}");
        streamGo.transform.position = VRCameraObject.transform.position;
        streamGo.transform.rotation = VRCameraObject.transform.rotation;
        streamGo.transform.SetParent(VRCameraObject.transform);
        Camera cam = streamGo.AddComponent<Camera>();
        cam.targetTexture = streamingTexture;
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

    public void ReassignCameraTextures(string previousSceneName, Scene newScene)
    {
        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogWarning("No se encontró XROrigin en la escena.");
            return;
        }

        VRCameraObject = xrOrigin.Camera.gameObject;

        foreach (KeyValuePair<string, ClientData> kvp in clients)
        {
            string clientId = kvp.Key;
            ClientData data = kvp.Value;

            if (data.type == ClientType.STREAM)
                CreateAnchoredCamera(clientId);
        }

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
        DontDestroyOnLoad(go);
        WebRTCPeer peer = go.AddComponent<WebRTCPeer>();
        
        CreateAnchoredCamera(ip);
        
        peer.Initialize(ip, streamingTexture, msg => webSocketServer.SendToNode(msg, ip));
        StartCoroutine(peer.CreateOffer());
        clients[ip].webRtcPeer = peer;

        connectionUI?.CreateUIRepresentation(ip);

        Debug.Log($"[StreamManager] Created browser peer: {ip}");
    }

    /// <summary>
    /// Elimina lo creado para representar al cliente navegador
    /// </summary>
    /// <param name="clientID"></param>
    public void RemovePeerForBrowser(string clientID)
    {
        Destroy(clients[clientID].webRtcPeer.gameObject);
        clients.TryRemove(clientID, out var data);
        Debug.Log($"[StreamManager] Destroyed browser peer: {clientID}");
    }
    #endregion

    #region TCP

    /// <summary>
    /// Creates the client object and completes the WebRTC connection exchange
    /// </summary>
    /// <param name="ip">IP of the client</param>
    public void CreatePeerForClient(ClientData client)
    {
        if (ClassManager.Instance.Settings.NumStudents <= clients.Count)
        {
            Debug.LogWarning("[StreamManager] No puede haber más clientes que alumnos configurados");
            return;
        }

        // Add client to the dictionary
        string ip = client.ipAddress;
        if (!addClient(ip, client)) return;

        // Create GameObject
        GameObject go = new GameObject($"{client.type.ToString()}-Peer_{ip}");
        DontDestroyOnLoad(go);
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
            CreateAnchoredCamera(ip);
            rt = streamingTexture;
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
        DontDestroyOnLoad(gameObject);

        streamingTexture = new RenderTexture((int)frameWidth, (int)frameHeight, (int)frameDepth, RenderTextureFormat.BGRA32);
        streamingTexture.enableRandomWrite = true;
        streamingTexture.useMipMap = false;
        streamingTexture.antiAliasing = 1;
        streamingTexture.Create();

        SceneChanger.Instance.OnSceneChanged.AddListener(ReassignCameraTextures);
    }

    private void OnDestroy()
    {
        SceneChanger.Instance.OnSceneChanged.RemoveListener(ReassignCameraTextures);
    }

    #endregion
}

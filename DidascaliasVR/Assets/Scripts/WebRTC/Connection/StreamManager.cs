using Didascalia;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

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

    [SerializeField]
    private GameObject VRCameraObject;

    /// <summary>
    /// Mapa de conexiones de navegador
    /// </summary>
    Dictionary<int, WebRTCPeer> browserPeers = new Dictionary<int, WebRTCPeer>();
    // Debe ser la IP del dispositivo que corre el servidor de Node
    [SerializeField] string nodeHost = "192.168.1.21";
    [SerializeField] int nodePort = 8080;

    ClientWebSocket ws;
    #endregion

    #region SharedMethods

    private RenderTexture CreateAnchoredCamera(string ip)
    {
        RenderTexture rt;
        rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.BGRA32);
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.antiAliasing = 1;
        rt.Create();

        GameObject streamGo = new GameObject($"StreamCamera-Peer_{ip}");
        streamGo.transform.position = VRCameraObject.transform.position;
        streamGo.transform.rotation = VRCameraObject.transform.rotation;
        streamGo.transform.SetParent(VRCameraObject.transform);
        Camera cam = streamGo.AddComponent<Camera>();
        cam.targetTexture = rt;
        //cam.enabled = false -> TO-DO: que se active o desactive segun si es Streamer o Player;
        return rt;
    }

    #endregion

    #region WebSocket
    // Inicia la conexion al servidor de Node
    public async void ConnectToNode()
    {
        ws = new ClientWebSocket();
        Uri uri = new Uri($"ws://{nodeHost}:{nodePort}?type=unity");

        try
        {
            await ws.ConnectAsync(uri, CancellationToken.None);
            Debug.Log($"[StreamManager] Conectado a Node: {uri}");
            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StreamManager] Error conectando a Node: {ex.Message}");
        }
    }

    // Bucle de recepcion de mensajes WebSockets
    async Task ReceiveLoop()
    {
        var buffer = new byte[8192];
        var sb = new StringBuilder();

        while (ws.State == WebSocketState.Open)
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

                UnityMainThreadDispatcher.Instance().Enqueue(() => HandleIncoming(json));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StreamManager] ReceiveLoop: {ex.Message}");
                break;
            }
        }
    }

    // Manejo de informacion recibida del servidor de Node
    void HandleIncoming(string rawJson)
    {
        WSBaseMessage baseMsg = JsonUtility.FromJson<WSBaseMessage>(rawJson);

        if (baseMsg.type == 99) // newClient
        {
            WSNewClientMessage newClient = JsonUtility.FromJson<WSNewClientMessage>(rawJson);
            CreatePeerForBrowser(newClient.clientId);
        }
        else // SDP o ICE de un browser existente
        {
            WSTaggedMessage taggedMsg = JsonUtility.FromJson<WSTaggedMessage>(rawJson);
            ProcessSignaling(taggedMsg.clientId, taggedMsg.type, taggedMsg.body);
        }
    }

    // Creacion de objeto en escena que representa un cliente Navegador
    void CreatePeerForBrowser(int clientId)
    {
        // Si ese navegador ya esta conectado, se ignora
        if (browserPeers.ContainsKey(clientId)) return;

        GameObject go = new GameObject($"WS-Peer_Browser_{clientId}");
        WebRTCPeer peer = go.AddComponent<WebRTCPeer>();
        RenderTexture rt = CreateAnchoredCamera(clientId.ToString());
        peer.Initialize("browser", rt, msg => SendToNode(msg, clientId));
        browserPeers[clientId] = peer;
        StartCoroutine(peer.CreateOffer());
        Debug.Log($"[StreamManager] Peer creado para browser {clientId}");
    }

    // Manejo de mensajes de conexion
    void ProcessSignaling(int clientId, int type, string body)
    {
        if (!browserPeers.TryGetValue(clientId, out var peer)) return;

        if (type == (int)ConnectionEvent.ICE)
        {
            IceCandidateData data = JsonUtility.FromJson<IceCandidateData>(body);
            peer.AddIceCandidate(new RTCIceCandidateInit
            {
                candidate = data.candidate,
                sdpMid = data.sdpMid,
                sdpMLineIndex = data.sdpMLineIndex
            });
        }
        else if (type == (int)ConnectionEvent.SDP)
        {
            SessionDescriptionData data = JsonUtility.FromJson<SessionDescriptionData>(body);
            StartCoroutine(peer.SetRemoteAnswer(data.ToRTCDesc()));
        }
        else if (type == (int)ConnectionEvent.DISCONNECT)
        {
            Destroy(peer.gameObject);
            browserPeers.Remove(clientId);
            Debug.Log($"[StreamManager] Peer eliminado para browser {clientId}");
        }
    }

    // Envia informacion al servidor de node
    async void SendToNode(SignalingMessage msg, int clientId)
    {
        if (ws?.State != WebSocketState.Open) return;
        string escapedBody = msg.body.Replace("\\", "\\\\").Replace("\"", "\\\"");
        string json = $"{{\"type\":{(int)msg.type},\"body\":\"{escapedBody}\",\"clientId\":{clientId}}}";
        byte[] data = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(data),
            WebSocketMessageType.Text, true, CancellationToken.None);
    }
    #endregion

    #region TCP

    /// <summary>
    /// Adds a client to the dictionary
    /// </summary>
    /// <param name="str">IP of the client</param>
    /// <param name="client">Client data</param>
    public void addClient(string ip, ClientData client)
    {
        clients.TryAdd(ip, client);
    }

    /// <summary>
    /// Removes a client form the dictionary
    /// </summary>
    /// <param name="str">IP of the client</param>
    public void removeClient(string ip)
    {
        clients.TryRemove(ip, out var client);
    }

    /// <summary>
    /// Creates the client object and completes the WebRTC connection exchange
    /// </summary>
    /// <param name="ip">IP of the client</param>
    public void CreatePeerForClient(ClientData client)
    {
        // Add client to the dictionary
        string ip = client.ipAddress;
        addClient(ip, client);

        // Create GameObject
        GameObject go = new GameObject($"{client.type.ToString()}-Peer_{ip}");
        go.GetComponent<Transform>().position = Vector3.zero;
        
        RenderTexture rt = null;
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
            rt = CreateAnchoredCamera(ip);
        }

        // Create RTC connection Peer
        WebRTCPeer peer = go.AddComponent<WebRTCPeer>();
        peer.Initialize(ip, rt, msg => SendSignalingMessage(ip, msg));
        clients[ip].webRtcPeer = peer;
        StartCoroutine(peer.CreateOffer());
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

    async void OnDestroy()
    {
        if (ws?.State == WebSocketState.Open)
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Bye", CancellationToken.None);
        ws?.Dispose();
    }
    #endregion
}

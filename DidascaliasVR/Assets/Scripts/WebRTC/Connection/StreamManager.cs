using Didascalia;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Unity.WebRTC;
using UnityEngine;

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
    /// 
    /// </summary>
    [SerializeField]
    private GameObject VRCameraObject;
    #endregion

    #region Methods

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
        
        // Create texture
        RenderTexture rt;
        rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.BGRA32);
        rt.enableRandomWrite = true;
        rt.useMipMap = false;
        rt.antiAliasing = 1;
        rt.Create();
        
        Camera cam;
        // Player
        if (client.type == ClientType.PLAYER)
        {
            cam = go.AddComponent<Camera>();
            go.AddComponent<PeerMovementComponent>();
        }
        // Streaming
        else
        {
            GameObject streamGo = new GameObject($"StreamCamera-Peer_{ip}");
            streamGo.transform.position = VRCameraObject.transform.position;
            streamGo.transform.rotation = VRCameraObject.transform.rotation;
            streamGo.transform.SetParent(VRCameraObject.transform);
            cam = streamGo.AddComponent<Camera>();
        }
        cam.targetTexture = rt;

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
    #endregion
}

using Didascalia;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Unity.WebRTC;
using UnityEngine;

public class StreamManager : MonoBehaviour
{
    public static StreamManager Instance { get; private set; }

    /// <summary>
    /// All currently connected clients, keyed by their IP.
    /// ConcurrentDictionary is used instead of Dictionary + lock because multiple background
    /// threads (one per client) may add or remove entries simultaneously. All individual
    /// operations (TryAdd, TryRemove, TryGetValue) are atomic, and its enumerator works on
    /// a snapshot so Broadcast iteration is safe without an external lock.
    /// </summary>
    readonly ConcurrentDictionary<string, ClientWebRTC> clients = new ConcurrentDictionary<string, ClientWebRTC>();

    public void addClient(string str, ClientWebRTC client)
    {
        clients.TryAdd(str, client);
    }

    public void removeClient(string str)
    {
        clients.TryRemove(str, out var client);
    }

    void createClients()
    {
        foreach (var cl in clients)
        {
            CreatePeerForClient(cl.Value.ipAddress, FrameCaptureFeature.Instance?.GetFrame());
        }
    }

    readonly ConcurrentDictionary<string, WebRTCPeer> peers = new();

    public void CreatePeerForClient(string ip, RenderTexture source)
    {
        var go = new GameObject($"Peer_{ip}");
        var peer = go.AddComponent<WebRTCPeer>();
        peer.RemoteIp = ip;
        peer.OnSignalingMessage = msg => SendSignalingMessage(ip, msg);
        peer.Initialize(source);
        peers[ip] = peer;
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
        if (!peers.TryGetValue(fromIp, out var peer)) return;

        if (msg.type == ConnectionEvent.ICE)
        {
            var init = JsonUtility.FromJson<RTCIceCandidateInit>(msg.body);
            peer.AddIceCandidate(init);
        }
        else if (msg.type == ConnectionEvent.SDP)
        {
            var answer = JsonUtility.FromJson<RTCSessionDescription>(msg.body);
            StartCoroutine(peer.SetRemoteAnswer(answer));
        }
    }

    void Awake()
    {
        if (Instance)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

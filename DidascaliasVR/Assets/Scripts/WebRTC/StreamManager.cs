using Didascalia;
using System.Collections.Concurrent;
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
            // Instanciar gb

            // Guardar referencia -> cl.Value.cam 
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

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.WebRTC;
using UnityEngine;

public class WebSocketServerRTC : MonoBehaviour
{

    #region Variables
    // Debe ser la IP del dispositivo que corre el servidor de Node
    [SerializeField] string nodeHost = "192.168.1.45";
    [SerializeField] int nodePort = 8080;

    ClientWebSocket ws;

    [SerializeField] string batRelativePath = "start-server.bat";
    #endregion

    #region Methods
    public void LaunchServer()
    {
        string batPath = System.IO.Path.Combine(Application.dataPath, "..", batRelativePath);

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = batPath,
            WorkingDirectory = System.IO.Path.GetDirectoryName(batPath),
            UseShellExecute = true, // necesario para que abra las ventanas cmd visibles
            CreateNoWindow = false
        };

        try
        {
            Process.Start(psi);
            UnityEngine.Debug.Log("[ServerLauncher] Script lanzado correctamente.");
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError($"[ServerLauncher] Error al lanzar el bat: {ex.Message}");
        }
    }

    // Inicia la conexion al servidor de Node
    public async void ConnectToNode()
    {
        ws = new ClientWebSocket();
        Uri uri = new Uri($"ws://{nodeHost}:{nodePort}?type=unity&id={ConnectionManager.Instance.SessionID}");

        try
        {
            await ws.ConnectAsync(uri, CancellationToken.None);
            UnityEngine.Debug.Log($"[StreamManager] Conectado a Node: {uri}");
            _ = ReceiveLoop();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[StreamManager] Error conectando a Node: {ex.Message}");
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
                UnityEngine.Debug.LogError($"[StreamManager] ReceiveLoop: {ex.Message}");
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
            string clientKey = newClient.clientId.ToString();
            ConnectionData connData = new ConnectionData(clientKey, nodePort, ConnectionEvent.HANDSHAKE, ClientType.STREAM);
            ClientData client = new ClientData(connData, null);
            StreamManager.Instance?.CreatePeerForBrowser(client);
        }
        else // SDP o ICE de un browser existente
        {
            WSTaggedMessage tagged = JsonUtility.FromJson<WSTaggedMessage>(rawJson);
            string clientKey = tagged.clientId.ToString();
            SignalingMessage sigMsg = new SignalingMessage(clientKey, nodeHost, (ConnectionEvent)tagged.type, tagged.body);
            StreamManager.Instance?.HandleIncomingSignaling(clientKey, sigMsg);
        }
    }

    // Envia informacion al servidor de node
    public async void SendToNode(SignalingMessage msg, string clientId)
    {
        if (ws?.State != WebSocketState.Open) return;

        if (!int.TryParse(clientId, out int idInt))
        {
            UnityEngine.Debug.LogError($"[WebSocketServerRTC] clientId inválido: {clientId}");
            return;
        }

        WSTaggedMessage tagged = new WSTaggedMessage { type = (int)msg.type, clientId = idInt, body = msg.body };
        byte[] data = Encoding.UTF8.GetBytes(JsonUtility.ToJson(tagged));
        await ws.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, CancellationToken.None);
    }
    #endregion

    #region Monobehaviour
    public void Start()
    {
        StartCoroutine(WebRTC.Update());
        LaunchServer();
        ConnectToNode();
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

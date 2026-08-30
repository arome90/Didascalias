using Newtonsoft.Json;
using Renci.SshNet;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[Serializable]
public class LLMResponseData
{
    public string Answer;
    public string Action;
    // Recibimos los argumentos como una lista de pares clave/valor si usas un Json parser flexible,
    // o deserializamos Args directamente con Newtonsoft.Json.
    // public Dictionary<string, object> Args; // Guardará el bloque JSON de los argumentos
}

public class LLMNetworkManager : Singleton<LLMNetworkManager>
{
    [Header("General Params")]
    [SerializeField] private bool _startConnectionOnAwake = true;

    [Header("SSH Settings")]
    [SerializeField] private string _sshHost = "IP_DE_TU_SERVIDOR";
    [SerializeField] private string _sshUser = "tu_usuario";
    [SerializeField] private int _sshPort = 22;

    [Tooltip("Path to SSH private key")]
    [SerializeField] private string _privateKeyPath = null;

    [Tooltip("Private key Passphrase")]
    [SerializeField] private string _privateKeyPassphrase = "";

    [Header("Python Script Settings")]
    [SerializeField] private string _remotePythonPath = "python3 /home/usuario/script.py";
    [SerializeField] private int _remoteSocketPort = 65432;

    [Header("LLM Settings")]
    [SerializeField, Range(0.0f, 1.0f)] private float _temperature = 0.5f;

    private SshClient _sshClient;
    private ForwardedPortLocal _forwardedPort;
    private TcpClient _tcpClient;
    private NetworkStream _stream;

    private const string LLM_MESSAGE_ID = "99";
    private const string CONNECTION_MESSAGE_ID = "00";

    private bool _isListening = false;
    private bool _isClosing = false;

    // Cola thread-safe para enlazar peticiones TaskCompletionSource en orden secuencial
    private readonly ConcurrentQueue<TaskCompletionSource<string>> _pendingRequests = new ConcurrentQueue<TaskCompletionSource<string>>();
    private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

    protected override void Awake()
    {
        base.Awake();
        if (_startConnectionOnAwake) StartLLMConnection();
    }

    public void StartLLMConnection() => Task.Run(() => ConnectToLLM());

    private void ConnectToLLM()
    {
        try
        {
            if (!File.Exists(_privateKeyPath))
            {
                Debug.LogError($"[SSH] No se encontró la clave privada en: {_privateKeyPath}");
                return;
            }

            PrivateKeyFile keyFile = string.IsNullOrEmpty(_privateKeyPassphrase)
                ? new PrivateKeyFile(_privateKeyPath)
                : new PrivateKeyFile(_privateKeyPath, _privateKeyPassphrase);

            var keyAuthMethod = new PrivateKeyAuthenticationMethod(_sshUser, new[] { keyFile });
            var connectionInfo = new ConnectionInfo(_sshHost, _sshPort, _sshUser, keyAuthMethod);

            _sshClient = new SshClient(connectionInfo);
            _sshClient.KeepAliveInterval = TimeSpan.FromSeconds(10);
            _sshClient.Connect();

            _forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)_remoteSocketPort, "127.0.0.1", (uint)_remoteSocketPort);
            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();

            var sshCmd = _sshClient.CreateCommand($"nohup python3 {_remotePythonPath} > /home/ucm-guest/python_log.txt 2>&1 &");
            sshCmd.BeginExecute();
            Debug.Log("[SSH] Script de Python lanzado.");

            Thread.Sleep(1500);

            _tcpClient = new TcpClient("127.0.0.1", _remoteSocketPort);
            _stream = _tcpClient.GetStream();

            _tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _stream.ReadTimeout = 3000;
            _stream.WriteTimeout = 3000;

            _isListening = true;

            Task.Run(() => ListenLoop());

            SendData(CONNECTION_MESSAGE_ID + "Establishing connection...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Error SSH/Socket]: {ex.Message}");
        }
    }

    /// <summary>
    /// Consulta asíncrona al LLM mediante Socket. Devuelve la respuesta en formato string directamente con await.
    /// </summary>
    public async Task<string> QueryLLMAsync(string query, object schema = null, float temperature = -1)
    {
        if (_stream == null || !_stream.CanWrite)
        {
            Debug.LogError("[LLMNetworkManager] No se puede enviar la consulta: El socket no está disponible.");
            return string.Empty;
        }

        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingRequests.Enqueue(tcs);

        // Empacar prompt y esquema en un único objeto
        var payloadObject = new
        {
            prompt = query,
            schema = schema,
            temperature = temperature == -1 ? _temperature : Mathf.Clamp(temperature, 0.0f, 1.0f)
        };

        string serializedPayload = JsonConvert.SerializeObject(payloadObject);

        await _sendLock.WaitAsync();
        try
        {
            SendData(LLM_MESSAGE_ID + serializedPayload);
        }
        finally
        {
            _sendLock.Release();
        }

        return await tcs.Task;
    }

    private async void ListenLoop()
    {
        try
        {
            while (_isListening && _tcpClient != null)
            {
                string response = ReadData();

                if (!string.IsNullOrEmpty(response))
                {
                    ProcessData(response);
                }
                else
                {
                    await Task.Delay(70);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LLMNetworkManager] Excepción escuchando socket: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void ProcessData(string data)
    {
        if (string.IsNullOrEmpty(data) || data.Length < 2) return;

        string id = data.Substring(0, 2);

        if (id == CONNECTION_MESSAGE_ID)
        {
            string finalMessage = data.Substring(2);
            Debug.Log($"[LLMNetworkManager] Conexión LLM: {finalMessage}");
        }
        else
        {
            // Desencolamos el TaskCompletionSource correspondiente y resolvemos la Promesa
            if (_pendingRequests.TryDequeue(out var tcs))
            {
                tcs.TrySetResult(data);
            }
            else
            {
                Debug.LogWarning("[LLMNetworkManager] Se recibió respuesta del socket pero no había peticiones pendientes.");
            }
        }
    }

    private void SendData(string message)
    {
        if (_stream == null || !_stream.CanWrite) return;

        try
        {
            byte[] body = Encoding.UTF8.GetBytes(message);
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));

            _stream.Write(header, 0, header.Length);
            _stream.Write(body, 0, body.Length);
            _stream.Flush();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LLMNetworkManager] Error al enviar datos: {ex.Message}");
        }
    }

    private string ReadData()
    {
        if (_stream == null || !_stream.CanRead) return null;

        try
        {
            byte[] headerBuffer = ReadExactBytes(4);
            if (headerBuffer == null) return null;

            int messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBuffer, 0));
            if (messageLength <= 0) return null;

            byte[] bodyBuffer = ReadExactBytes(messageLength);
            if (bodyBuffer == null) return null;

            return Encoding.UTF8.GetString(bodyBuffer);
        }
        catch (IOException)
        {
            // Timeout normal de lectura de socket
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LLMNetworkManager] Error al leer datos: {ex.Message}");
        }

        return null;
    }

    private byte[] ReadExactBytes(int totalBytesToRead)
    {
        byte[] buffer = new byte[totalBytesToRead];
        int bytesReadSoFar = 0;

        while (bytesReadSoFar < totalBytesToRead)
        {
            int read = _stream.Read(buffer, bytesReadSoFar, totalBytesToRead - bytesReadSoFar);
            if (read == 0) return null;
            bytesReadSoFar += read;
        }

        return buffer;
    }

    private void OnDestroy() => CloseLLMConnection();

    public void CloseLLMConnection()
    {
        if (_isClosing) return;
        _isClosing = true;
        _isListening = false;

        // 1. Mandar señal de desconexión al servidor Python
        try
        {
            if (_stream != null && _stream.CanWrite)
            {
                Debug.Log("[LLMNetworkManager] Sending 'DISCONNECT' signal to Python server...");
                SendData("DISCONNECT");
                // Leemos la confirmación opcional de Python
                ReadData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LLMNetworkManager] 'DISCONNECT' signal was not delivered: {ex.Message}");
        }

        // 2. Cerrar Stream y Socket TCP
        try
        {
            _stream?.Close();
            _tcpClient?.Close();
            Debug.Log("[LLMNetworkManager] TCP socket closed.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LLMNetworkManager] Error while closing TCP socket: {ex.Message}");
        }

        // 3. Detener Túnel SSH y cerrar Cliente SSH
        try
        {
            if (_forwardedPort != null && _forwardedPort.IsStarted)
            {
                _forwardedPort.Stop();
            }

            if (_sshClient != null && _sshClient.IsConnected)
            {
                _sshClient.Disconnect();
                _sshClient.Dispose();
                Debug.Log("[LLMNetworkManager] SSH Session succesfully closed.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LLMNetworkManager] Error closing SSH session: {ex.Message}");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LLMNetworkManager))]
public class LLMNetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Referencia al script original
        LLMNetworkManager script = (LLMNetworkManager)target;

        // Botón para refrescar la lista de GameObjects en tiempo real (Runtime / Editor)
        if (GUILayout.Button("TestLLMConnection"))
        {
            script.StartLLMConnection();
        }

        // Dibuja el resto de variables públicas por defecto si las hubiera
        DrawDefaultInspector();
    }
}
#endif

using Renci.SshNet;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

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

    private SshClient _sshClient;
    private ForwardedPortLocal _forwardedPort;
    private TcpClient _tcpClient;
    private NetworkStream _stream;

    const string LLM_MESSAGE_ID         = "99";
    const string CONNECTION_MESSAGE_ID  = "00";

    private bool _isClosing = false;

    private bool _isListening = true;

    ConcurrentQueue<string> _messagesFromSocket = null;
    Queue<Student> _studentMessageOrder = null;

    public UnityEvent<string> OnLLMResponseReceived = new UnityEvent<string>();

    protected override void Awake()
    {
        base.Awake();

        _messagesFromSocket = new ConcurrentQueue<string>();

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
            _sshClient.Connect();
            Debug.Log("[SSH] Connection succesful.");

            _forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)_remoteSocketPort, "127.0.0.1", (uint)_remoteSocketPort);
            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();

            var sshCmd = _sshClient.CreateCommand($"nohup python3 {_remotePythonPath} > /home/ucm-guest/python_log.txt 2>&1 &");
            sshCmd.BeginExecute();
            Debug.Log("[SSH] Script de Python lanzado.");

            System.Threading.Thread.Sleep(1500);

            _tcpClient = new TcpClient("127.0.0.1", _remoteSocketPort);
            _stream = _tcpClient.GetStream();
            _stream.ReadTimeout = 30000;
            _stream.WriteTimeout = 3000;

            _isListening = true;

            Task.Run(() => ListenLoop());

            OnLLMResponseReceived.AddListener(OnLLMAnswer);

            SendData(CONNECTION_MESSAGE_ID+"Establishing connection...");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Error SSH/Socket]: {ex.Message}");
        }
    }

    /// <summary>
    /// Listening to LLM socket - Loop
    /// </summary>
    private void ListenLoop()
    {
        while (_isListening && _tcpClient != null && _tcpClient.Connected)
        {
            string response = ReadData();
            if (!string.IsNullOrEmpty(response))
            {
                // Encolamos el mensaje para que Unity lo procese en Update()
                _messagesFromSocket.Enqueue(response);
            }
            else Thread.Sleep(25);
        }
    }

    private void FixedUpdate()
    {
        // Desencolamos las respuestas en el hilo principal de Unity para disparar el evento de forma segura
        while (_messagesFromSocket.TryDequeue(out string message))
        {
            ProcessData(message);
        }
    }

    private void OnLLMAnswer(string answer)
    {
        if (_studentMessageOrder == null || _studentMessageOrder.Count == 0) 
        { 
            Debug.LogError("LLM was queried without a student assigned");
            return;
        }

        Student st = _studentMessageOrder.Dequeue();
        st.Speak(answer);
        st.AddStudentInteractionContext(answer);
    }

    private void ProcessData(string data)
    {
        string id = data.Substring(0, 2);
        string finalMessage = data.Substring(2);

        if (id == LLM_MESSAGE_ID)
        {
            OnLLMResponseReceived?.Invoke(finalMessage);
            Didascalia.Utils.Log.Message($"LLM SAYS: {finalMessage}", this);
        }
        else if (id == CONNECTION_MESSAGE_ID)
        {
            Didascalia.Utils.Log.Message($"LLM CONNECTION SAYS: {finalMessage}", this);
        }
    }

    public void QueryLLM(string query, Student st)
    {
        if (_studentMessageOrder == null) _studentMessageOrder = new Queue<Student>();

        // since the socket first reads a message and then waits until the LLM processes it, we know for sure
        // that each message will be processed and received in order, so we can enqueue the messages that
        // correspond to each Student using this queue. 
        // When a message is received, the first item in the queue will be selected as the student who is answering that
        _studentMessageOrder.Enqueue(st);

        SendData(LLM_MESSAGE_ID + query);
    }

    public void SendData(string message)
    {
        if (_stream == null || !_stream.CanWrite) return;

        try
        {
            // 1. Convertir el mensaje a bytes en UTF-8
            byte[] body = Encoding.UTF8.GetBytes(message);

            // 2. Convertir la longitud del array a un entero de 4 bytes en formato de red (Big-Endian)
            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(body.Length));

            // 3. Enviar el encabezado (4 bytes) y luego el cuerpo
            _stream.Write(header, 0, header.Length);
            _stream.Write(body, 0, body.Length);

            // 4. Asegurar que los datos salen del búfer local inmediatamente
            _stream.Flush();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Socket] Error enviando datos: {ex.Message}");
        }
    }

    public string ReadData()
    {
        if (_stream == null || !_stream.CanRead) return null;

        try
        {
            // 1. Leer los 4 bytes del encabezado para conocer la longitud total del texto
            byte[] headerBuffer = ReadExactBytes(4);
            if (headerBuffer == null) return null;

            // Convertir de formato de red (Big-Endian) a entero local
            int messageLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(headerBuffer, 0));

            if (messageLength <= 0) return null;

            // 2. Leer exactamente la cantidad de bytes indicada en el encabezado
            byte[] bodyBuffer = ReadExactBytes(messageLength);
            if (bodyBuffer == null) return null;

            // 3. Convertir el array de bytes completo a string UTF-8
            return Encoding.UTF8.GetString(bodyBuffer);
        }
        catch (IOException)
        {
            // Timeout de lectura expirado
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Socket] Error leyendo datos: {ex.Message}");
        }

        return null;
    }

    // Garantiza leer N bytes exactos antes de continuar
    private byte[] ReadExactBytes(int totalBytesToRead)
    {
        byte[] buffer = new byte[totalBytesToRead];
        int bytesReadSoFar = 0;

        while (bytesReadSoFar < totalBytesToRead)
        {
            int read = _stream.Read(buffer, bytesReadSoFar, totalBytesToRead - bytesReadSoFar);
            if (read == 0) return null; // El socket se cerró inesperadamente
            bytesReadSoFar += read;
        }

        return buffer;
    }

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
                Debug.Log("[Socket] Sending 'DISCONNECT' signal to Python server...");
                SendData("DISCONNECT");
                // Leemos la confirmación opcional de Python
                ReadData();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Socket] 'DISCONNECT' signal was not delivered: {ex.Message}");
        }

        // 2. Cerrar Stream y Socket TCP
        try
        {
            _stream?.Close();
            _tcpClient?.Close();
            Debug.Log("[Socket] TCP socket closed.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[Socket] Error closing TCP socket: {ex.Message}");
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
                Debug.Log("[SSH] SSH Session succesfully closed.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SSH] Error closing SSH session: {ex.Message}");
        }
    }

    // Evento: Se ejecuta cuando se cierra la aplicación de Unity
    private void OnApplicationQuit()    => CloseLLMConnection();
    private void OnDestroy()            => CloseLLMConnection();

    // Cierra todas las conexiones limpiamente en orden
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

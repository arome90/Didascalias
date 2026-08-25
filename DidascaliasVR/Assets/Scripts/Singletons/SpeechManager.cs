using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class SpeechManager : Singleton<SpeechManager>
{
    [Header("Detected Names Event")]
    public UnityEvent<List<string>> OnNamesDetected; // Evento que devuelve los nombres encontrados

    private HashSet<string> _fastVocabularySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    [Header("General Params")]
    [SerializeField] private bool _startConnectionOnAwake = true;
    [SerializeField] private string _pythonExecutablePath = null;
    [SerializeField] private string _scriptPath = null;

    private System.Diagnostics.Process _pythonProcess;

    private bool _sendTranscriptions = false; // to start sending transcriptions once we have the students set up
    private bool _ready = false; // to know when the system is set up to start the session
    public bool IsReadyForTranscription => _ready;

    [Header("Local Socket Settings")]
    [SerializeField] private string _serverIP = "127.0.0.1";
    [SerializeField] private int _serverPort = 65433;

    [Header("Events")]
    public UnityEvent<string> OnTranscriptionReceived;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private StreamReader _reader;
    private StreamWriter _writer;

    private bool _isListening = false;
    private ConcurrentQueue<string> _messagesFromSocket = new ConcurrentQueue<string>();

    private ConcurrentQueue<string> _debugLogQueue = new ConcurrentQueue<string>();
    private ConcurrentQueue<string> _debugErrorQueue = new ConcurrentQueue<string>();

    const string CONNECTION_MESSAGE_ID =        "00";
    const string TRANSCRIPTION_MESSAGE_ID =     "01";
    const string VOCABULARY_MESSAGE_ID =        "02";
    const string READY_FOR_TRANSCRIPTION_ID =   "80";
    const string CLOSE_CONNECTION_ID =          "99";

    protected override void Awake()
    {
        base.Awake();
        if (_startConnectionOnAwake) StartConnection();

        OnTranscriptionReceived.AddListener(TranscriptionDebug);
    }

    private void Update()
    {
        while (_debugLogQueue.TryDequeue(out string logMsg))
            Debug.Log(logMsg);

        while (_debugErrorQueue.TryDequeue(out string errorMsg))
            Debug.LogError(errorMsg);

        while (_messagesFromSocket.TryDequeue(out string rawMessage))
            ProcessIncomingMessage(rawMessage);
    }

    public IEnumerator SendVocabularyList()
    {
        yield return new WaitUntil(() => StudentManager.Exists);
        yield return new WaitUntil(() => StudentManager.Instance.StudentsGenerated);

        List<string> vocabulary = StudentManager.Instance.GetStudentNames();

        if (vocabulary == null || vocabulary.Count == 0) yield break;
        string joinedList = string.Join(", ", vocabulary);
        SendData($"{VOCABULARY_MESSAGE_ID}{joinedList}");

        // building the fast-checking vocabulary
        _fastVocabularySet = new HashSet<string>(vocabulary, StringComparer.OrdinalIgnoreCase);

        _sendTranscriptions = true;
    }

    /// <summary>
    /// Extrae todos los nombres presentes en la frase de forma ultra rápida O(N)
    /// </summary>
    public List<string> DetectNamesInSentence(string sentence)
    {
        if (string.IsNullOrEmpty(sentence) || _fastVocabularySet.Count == 0)
            return new List<string>();

        // 1. Limpiar signos de puntuación comunes que Whisper suele añadir
        char[] punctuation = new char[] { ' ', '.', ',', ';', '!', '?', '¿', '¡', '"', ':', '-' };
        string[] words = sentence.Split(punctuation, StringSplitOptions.RemoveEmptyEntries);

        List<string> detectedNames = new List<string>();

        // 2. Comprobación O(1) por cada palabra en la frase
        foreach (string word in words)
        {
            if (_fastVocabularySet.Contains(word) && !detectedNames.Contains(word, StringComparer.OrdinalIgnoreCase))
                detectedNames.Add(word);
        }

        return detectedNames;
    }

    private void TranscriptionDebug(string transcription)
    {
        _debugLogQueue.Enqueue($"[SpeechManager] Transcription from python: {transcription}");
    }

    private void StartConnection() => Task.Run(() => ConnectToPythonServer());

    private async void ConnectToPythonServer()
    {
        try
        {
            _debugLogQueue.Enqueue("[Python] Iniciando proceso...");
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = _pythonExecutablePath,
                Arguments = $"\"{_scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            };

            _pythonProcess = System.Diagnostics.Process.Start(startInfo);

            int maxRetries = 15;
            int delayMs = 1000;
            bool connected = false;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await Task.Delay(delayMs);
                    _tcpClient = new TcpClient(_serverIP, _serverPort);
                    connected = true;
                    _debugLogQueue.Enqueue("[Socket] Connected to python.");
                    break;
                }
                catch (SocketException)
                {
                    _debugLogQueue.Enqueue($"[Socket] Esperando a Python... ({i + 1}/{maxRetries})");
                }
            }

            if (!connected)
            {
                _debugErrorQueue.Enqueue("No conectado...");
                throw new Exception("Timeout: Python no abrió el servidor a tiempo.");
            }

            _debugLogQueue.Enqueue($"[Socket] Conectando a Python en {_serverIP}:{_serverPort}...");
            _tcpClient = new TcpClient(_serverIP, _serverPort);
            _stream = _tcpClient.GetStream();

            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

            _isListening = true;
            Task.Run(() => ListenLoop());

            SendData($"{CONNECTION_MESSAGE_ID}Unity Connected");
        }
        catch (Exception ex)
        {
            _debugErrorQueue.Enqueue($"[Error Socket]: No se pudo conectar con Python: {ex.Message}");
        }
    }

    private void ListenLoop()
    {
        try
        {
            while (_isListening && _tcpClient != null)
            {
                _debugLogQueue.Enqueue("[Speech] Listening...");
                string line = _reader.ReadLine();
                if (line != null)
                {
                    _messagesFromSocket.Enqueue(line);
                }
            }
        }
        catch (Exception ex)
        {
            if (_isListening) _debugErrorQueue.Enqueue($"[Socket Reader]: {ex.Message}");
        }
    }

    private void ProcessIncomingMessage(string rawMessage)
    {
        if (string.IsNullOrEmpty(rawMessage) || rawMessage.Length < 2) return;

        string id = rawMessage.Substring(0, 2);
        string content = rawMessage.Substring(2);

        switch (id)
        {
            case CONNECTION_MESSAGE_ID:
                _debugLogQueue.Enqueue($"[Status Python]: {content}");
                break;
            case TRANSCRIPTION_MESSAGE_ID:
                if (_sendTranscriptions) TranscriptionDetected(content);
                break;
            case READY_FOR_TRANSCRIPTION_ID:
                _debugLogQueue.Enqueue($"[Ready Python]: {content}");
                StartCoroutine(SendVocabularyList());
                _ready = true;
                break;
        }
    }

    private void TranscriptionDetected(string content)
    {
        List<string> names = DetectNamesInSentence(content);
        if (names.Count > 0) OnNamesDetected?.Invoke(names);

        OnTranscriptionReceived?.Invoke(content);
    }

    public void SendData(string data)
    {
        if (_writer != null && _tcpClient.Connected)
        {
            _writer.WriteLine(data);
        }
    }

    private void OnDestroy() => CloseConnection();
    private void OnApplicationQuit() => CloseConnection();

    // En el método de cierre:
    private void CloseConnection()
    {
        _isListening = false;

        try
        {
            if (_tcpClient != null && _tcpClient.Connected)
            {
                SendData(CLOSE_CONNECTION_ID+"Close"); // Ordena el cierre limpio a Python
                System.Threading.Thread.Sleep(200); // Pequeña pausa para asegurar el envío
            }
        }
        catch { }

        _writer?.Close();
        _reader?.Close();
        _stream?.Close();
        _tcpClient?.Close();

        // 3. Matar el proceso Python de forma segura
        if (_pythonProcess != null)
        {
            try
            {
                // Intentamos comprobar si sigue vivo de forma segura
                if (!_pythonProcess.HasExited) _pythonProcess.Kill();
            }
            catch (InvalidOperationException)
            {
                // El proceso ya se cerró completamente por su cuenta (os._exit) o no se pudo asociar el Handle.
                // Es el comportamiento normal cuando Python recibe el ID "99".
            }
            catch (Exception ex)
            {
                _debugErrorQueue.Enqueue($"[Error cerrando Python]: {ex.Message}");
            }
            finally
            {
                _pythonProcess.Dispose();
                _pythonProcess = null;
            }
        }
    }
}
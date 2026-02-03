using Meta.WitAi.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using WebSocketSharp;

// -------------------------
// *************************
//        DISCLAIMER
//        DISCLAIMER
// *************************
// -------------------------

// A ver, aquí hay mil cosas que están extremadamente feas en general.
// Todas estas estructuras de aquí no tendrían que tener los nombres
// que tiene o directamente ni deberían existir.
// La web está hecha para que funcione como funcionaba antes,
// así que hasta que no consigamos acceso para modificarla a nuestro gusto y cambiar
// los parámetros que se reciben y tal, no podemos hacer mucho.
// De momento, muchas de estas clases deben quedar así por mucho que pese.

// Un saludo

[Serializable]
// Decide qué botones queremos crear en la web
public struct WebOptions
{
    public string[] opcionesGlobales; // no cambiar nombre
}

[Serializable]
// Un Vector2 serializable, que cualquiera pensaría que ya se podría pero no lolazo
public struct VectorJson
{
    public float x;
    public float y;

    /// <summary>
    /// Constructor para un vector en formato JSON.
    /// </summary>
    /// <param name="xValue">El valor en X</param>
    /// <param name="yValue">El valor en Y</param>
    public VectorJson(float xValue, float yValue)
    {
        x = xValue;
        y = yValue;
    }
}

[Serializable]
// Mensaje enviado al iniciar la clase, que sirve para
// colocar a los alumnos en la web y añadir el catálogo de botones
public struct InitialMessageData
{
    public WebStudent[] alumnosPosiciones; // no cambiar nombre
    //public string horaClase;
    //public long tiempoSesion;
    public WebOptions catalogo; // no cambiar nombre
    public string deviceID;

    /// <summary>
    /// Constructor para el mensaje inicial con los datos de los alumnos y el cat�logo de opciones.
    /// </summary>
    /// <param name="students">Alumnos represntados para formato web</param>
    /// <param name="cat">El cat�logo de opciones disponibles</param>
    public InitialMessageData(WebStudent[] students, WebOptions options)
    {
        //horaClase = hora;
        //tiempoSesion = sesion;
        this.alumnosPosiciones = students;
        this.catalogo = options;
        deviceID = ConnectionManager.Instance.ClientID;
    }
}

[Serializable]
// Estudiante representado en la web
public struct WebStudent
{
    public string nombre; // no cambiar nombre
    public int id; // no sirve para nada (?
    public VectorJson posicion; // no cambiar nombre
}

/// <summary>
/// Esto es para los eventos que se llaman
/// al pulsar los botones en web
/// </summary>
public enum WebEventType
{
    Message = -1,
    Disrespect = 0,
    SitTogether = 1,
    StandUp = 2,
    // ...
    Restart = 3
}

[Serializable]
// Mensaje recibido
public struct ReceivedWebMessage
{
    public string type;
    public WebEventType id;
    public string data;
    public string studentName;
}

// Tipo de mensaje a enviar
public enum SentWebMessageType
{
    NewSpectator = 0,
    CreateSession = 1,
    Init = 2,
    Info = 3,
    Action = 4,
    Resume = 5,
    JsonData = 6
}

[Serializable]
// Mensaje para mandar a la web
// Contiene su tipo, la sesión a la que pertenece,
// los datos a enviar y el identificador de dispositivo
public struct WebMessage
{
    public int type; // no cambiar nombre
    public string session; // no cambiar nombre
    public object data; // no cambiar nombre
    public string deviceID; // no cambiar nombre

    /// <summary>
    /// Constructor para enviar un mensaje con tipo, sesi�n y datos.
    /// </summary>
    /// <param name="messageType">El tipo de mensaje</param>
    /// <param name="sessionId">El ID de la sesi�n</param>
    /// <param name="dat">Los datos del mensaje</param>
    public WebMessage(SentWebMessageType messageType, string sessionId, object dat)
    {
        session = sessionId;
        type = (int)messageType;
        data = dat;
        deviceID = ConnectionManager.Instance.ClientID;
    }
}

/// <summary>
/// Realiza la conexión con la web mediante un WebSocket. Se encargará de mandar
/// y recibir todos los mensajes necesarios. Cualquier mensaje a ser enviado
/// debe pasar por esta clase.
/// </summary>
public class ConnectionManager : Singleton<ConnectionManager>
{
    [SerializeField]
    string _url = "wss://cyclops-dev.uab.cat/game/";

    WebSocket _socket = null;

    /// <summary>
    /// ID del cliente. Se representa como la DeviceUniqueIdentifier
    /// </summary>
    private string _clientID;
    /// <summary>
    /// ID del cliente.
    /// </summary>
    public string ClientID { get { return _clientID; } }

    /// <summary>
    /// Si se ha establecido o no la conexión. True cuando se abre el Socket, False cuando se cierra
    /// </summary>
    private bool _connectionEstablished = false;
    /// <summary>
    /// Si se ha establecido o no la conexión.
    /// </summary>
    public bool ConnectionEstablished { get { return _connectionEstablished; } }

    /// <summary>
    /// ID de la sesión creada por este ConnectionManager.
    /// </summary>
    private string _sessionID = null;

    /// <summary>
    /// ID de la sesión de la web creada.
    /// </summary>
    public string SessionID { get { return _sessionID; } }

    [SerializeField]
    private UnityEvent<ReceivedWebMessage> _onWebEventCalled;

    private bool _newWebMessage = false;
    private ReceivedWebMessage _currentMessage;

    private void Start()
    {
        _socket = new WebSocket(_url);

        _socket.OnOpen += OnOpen;
        _socket.OnMessage += OnMessage;
        _socket.OnClose += OnClose;

        _clientID = SystemInfo.deviceUniqueIdentifier;
    }

    private void OnEnable()
    {
        StartCoroutine(CheckForWebEvent());
    }

    /// <summary>
    /// Lanza el mensaje de configuración de sesión al servidor
    /// Se debe llamar cuando comience la clase y los estudiantes ya estén colocados
    /// </summary>
    public void ClassStarted()
    {
        SendInitialMessage();
    }

    /// <summary>
    /// Llamado cuando la conexión con el WebSocket se abre
    /// </summary>
    /// <param name="sender"> no utilizado </param>
    /// <param name="e"> no utilizado </param>
    private void OnOpen(object sender, System.EventArgs e)
    {
        _connectionEstablished = true;
        RequestNewSession();
    }

    /// <summary>
    /// Petición al servidor de una nueva sesión virtual
    /// </summary>
    private void RequestNewSession()
    {
        _sessionID = null;
        WebMessage message = new WebMessage(
            SentWebMessageType.CreateSession,
            _sessionID,
            _clientID);
        SendWebMessage(message);
    }

    /// <summary>
    /// Llamado cuando el WebSocket recibe un mensaje desde el servidor
    /// </summary>
    /// <param name="sender"> objeto que manda el mensaje </param>
    /// <param name="e"> Argumentos con los que se mandó el mensaje </param>
    private void OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
    {
        Debug.Log("MESSAGE DATA: " + e.Data);

        ReceivedWebMessage message = new ReceivedWebMessage();
        TryDeserializeMessage(e.Data, ref message);

        /// CASOS
        /// 1 - Sesión recibida
        /// callEvent - Con una ID asociada, para llamar a uno de los eventos descritos por el botón pulsado en la web
        /// ...
        switch (message.type) {
            case "1":
                OnSessionReceived(message);
                break;
            case "error":
                Debug.LogError("Error reported from web. Data: " + message.data);
                break;
            case "callEvent":
                WebEventReceived(message);
                break;
            default:
                Debug.LogWarning('\'' + message.type + "' message type received from WebSocket not recognized.\n" +
                    "Message Data:"+e.Data);
                break;
        }
    }

    private void WebEventReceived(ReceivedWebMessage message)
    {
        _currentMessage = message;
        _newWebMessage = true;
    }

    IEnumerator CheckForWebEvent()
    {
        _newWebMessage = false;
        _currentMessage = new ReceivedWebMessage();

        while (gameObject.activeSelf)
        {
            yield return new WaitUntil(
                () => _newWebMessage
                );

            _onWebEventCalled.Invoke(_currentMessage);

            _newWebMessage = false;
        }
    }

    private WebStudent[] GenerateWebStudentsInfo()
    {
        List<Student> students = StudentManager.Instance.GetStudents();
        WebStudent[] webStudents = new WebStudent[students.Count];

        int i = 0;
        // Cogemos la posición del estudiante actual
        Vector2 firstPosition = new Vector2 { x = students[i].transform.position.x, y = -students[i].transform.position.z };

        foreach (Student st in students)
        {
            // Setteamos el nombre
            webStudents[i].nombre = st.name;

            // Posición del estudiante actual
            Vector2 currentPos;
            currentPos.x = st.transform.position.x;
            currentPos.y = -st.transform.position.z;

            // Calculamos la posición en función de la primera posición
            // Dividimos entre un factor para escalarlo a la página web
            Vector2 newPos = (firstPosition - currentPos) / 10.0f;
            webStudents[i].posicion = new VectorJson(newPos.x, newPos.y);

            ++i;
        }

        return webStudents;
    }

    /// <summary>
    /// Manda un mensaje al servidor con la información de los alumnos,
    /// además de hacer una request para crear los botones de la interfaz
    /// </summary>
    private void SendInitialMessage()
    {
        WebStudent[] webStudents = GenerateWebStudentsInfo();

        WebOptions options;
        options = new WebOptions
        {
            opcionesGlobales = new string[] { "Faltar el respeto", "Sentarse juntos", "Levantarse", "Restart" }
        };
        InitialMessageData data = new InitialMessageData(webStudents, options);

        SendWebMessage(new WebMessage(SentWebMessageType.Init, SessionID, data));
    }

    /// <summary>
    /// Llamado cuando se recibe una sesión
    /// </summary>
    /// <param name="message"> Información de la sesión </param>
    private void OnSessionReceived(ReceivedWebMessage message)
    {
        _sessionID = message.data;
    }

    /// <summary>
    /// Si la sesión está disponible o no
    /// </summary>
    public bool IsSessionAvaliable() { return _sessionID != null && _sessionID != string.Empty; }

    /// <summary>
    /// Llamado al cerrar el WebSocket
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClose(object sender, WebSocketSharp.CloseEventArgs e)
    {
        _connectionEstablished = false;
        _sessionID = null;

        Debug.Log("WEB SOCKET WAS CLOSED: " + e.Reason);
    }

    /// <summary>
    /// Manda un mensaje al WebSocket
    /// </summary>
    /// <param name="message"> Mensaje a enviar </param>
    public void SendWebMessage(WebMessage message)
    {
        if(_socket != null && _socket.IsAlive)
        {
            var jsonData = JsonConvert.SerializeObject(message);
            _socket.SendAsync(jsonData, null);
        }
        else
        {
            Debug.LogError("Tried sending messaged to closed WebSocket.");
        }
    }

    /// <summary>
    /// Manda una UnityWebRequest a la URL que tenga asignada el ConnectionManager
    /// </summary>
    /// <param name="jsonData"> Datos a enviar en formato JSON </param>
    public void SendWebRequest(string jsonData)
    {
        StartCoroutine(SendWebRequestCoroutine(jsonData));
    }

    /// <summary>
    /// Corrutina que manda una Web Request a la URL designad
    /// </summary>
    /// <param name="jsonData"> Datos a enviar en formato JSON </param>
    private IEnumerator SendWebRequestCoroutine(string jsonData)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(_url, jsonData, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                // EN CASO DE ERROR DEBERÍAMOS DEVOLVER EL JSON Y SUMARLO A LO QUE TENGAMOS ANTERIORMENTE (?)
                Debug.Log("ERROR enviando json: " + www.error);
                // GameManager.Instance.LostSessionConnection();
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// Intenta deserializar el mensaje recibido en un objeto ReceivedWebMessage.
    /// </summary>
    /// <param name="data">Datos del mensaje en formato JSON.</param>
    /// <param name="message">Referencia al objeto MessageReceived donde se almacenar� el mensaje deserializado.</param>
    /// <returns>True si el mensaje se deserializ� correctamente, False en caso contrario.</returns>
    private bool TryDeserializeMessage(string data, ref ReceivedWebMessage message)
    {
        try
        {
            message = JsonConvert.DeserializeObject<ReceivedWebMessage>(data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al procesar el mensaje Web. Excepción: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Comienza la conexión con el WebSocket
    /// </summary>
    public void StartConnection()
    {
        _socket.ConnectAsync();
    }

    /// <summary>
    /// Para la conexión con el WebSocket
    /// </summary>
    public void StopConnection()
    {
        _socket.CloseAsync();
    }
}

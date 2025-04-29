using WebSocketSharp;
using UnityEngine;
using System;
using System.Collections;
using Newtonsoft.Json;
using ClassRoomVR;

/// <summary>
/// Clase que maneja la conexi�n WebSocket y la comunicaci�n con el servidor.
/// Implementa un Singleton gen�rico para ser accesible globalmente.
/// </summary>
public class WsClient : GenericSingleton<WsClient>
{
    // Indicador de acci�n pendiente.
    private bool actionFlag = false;

    // Conexi�n WebSocket.
    private static WebSocket ws = null;

    // Mensaje recibido.
    private MessageReceived receivedMessage;

    // Identificador del dispositivo.
    public string _deviceId;

    // Propiedad que indica si la conexi�n est� activa.
    public bool IsConnected { get; private set; }

    // Sesi�n actual.
    public string Session { get; private set; }

    /// <summary>
    /// Inicializa el estado de conexi�n y el identificador del dispositivo.
    /// </summary>
    private void Start()
    {
        IsConnected = false;
        _deviceId = SystemInfo.deviceUniqueIdentifier;
        /////Debug.Log("Invoke timeOut");
        /////InvokeRepeating("TimeOut", 3.0f, 20.0f);
    }

    /// <summary>
    /// Inicia la conexi�n WebSocket al servidor especificado.
    /// Maneja eventos de apertura, mensajes y cierre.
    /// </summary>
    public void StartConnection()
    {
        try
        {
            if (ws != null && ws.IsAlive) return;
            Debug.Log("Creating new WebSocket");
            /////GameManager.Instance.SetWsTryingToConnect(true);
            //GameManager.Instance.ChangeWsTxt("Trying to connect ws...");
            //ws = new WebSocket("wss://cyclops.uab.cat/game/");
            ws = new WebSocket("wss://cyclops-dev.uab.cat/game/");
            SubscribeAndConnectWS(ws);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error en la conexi�n WebSocket: " + ex.Message);
        }
    }

    void TimeOut()
    {
        if (!GameManager.Instance.GetWsConnection())
        {
            Debug.Log("TimeOut to connect, try again.");
            GameManager.Instance.Pause(true, true);
            //GameManager.Instance.ChangeWsTxt("TimeOut, we try again.");
            GameManager.Instance.SetWsTryingToConnect(false);
            Disconnect();
            StartConnection();
        }
    }

    IEnumerator WaitSecondsToSubscribe(float seconds, WebSocket ws)
    {
        yield return new WaitForSeconds(seconds);
        SubscribeAndConnectWS(ws);
    }

    void SubscribeAndConnectWS(WebSocket ws)
    {
        ws.OnOpen += HandleOnOpen;
        ws.OnMessage += HandleSessionMessage;
        ws.OnClose += HandleOnClose;

        ws.ConnectAsync();
    }

    public bool IsAlive()
    {
        return ws.IsAlive;
    }

    /// <summary>
    /// Maneja el evento cuando se cierra la conexi�n WebSocket.
    /// Actualiza el estado de conexi�n y pausa el juego si no fue una desconexi�n limpia.
    /// </summary>
    /// <param name="sender">El objeto que env�a el evento.</param>
    /// <param name="e">Informaci�n del evento de cierre.</param>
    private void HandleOnClose(object sender, CloseEventArgs e)
    {
        IsConnected = false;
        Session = e.WasClean ? null : Session;

        if (!e.WasClean)
        {
            Debug.Log("Conexi�n perdida. No hay Internet.");
            /////GameManager.Instance.Pause(true);
        }
    }

    /// <summary>
    /// Maneja el evento cuando se abre la conexi�n WebSocket.
    /// Env�a un mensaje al servidor con el tipo de mensaje "CreateSession".
    /// </summary>
    /// <param name="sender">El objeto que env�a el evento.</param>
    /// <param name="e">Informaci�n del evento de apertura.</param>
    private void HandleOnOpen(object sender, EventArgs e)
    {
        IsConnected = true;
        var message = new MessageSent(MessageType.CreateSession, Session, _deviceId);
        SendWebSocketMessage(message);
    }

    /// <summary>
    /// Maneja los mensajes iniciales de sesi�n recibidos del servidor.
    /// Si se recibe un mensaje v�lido, actualiza la sesi�n y cambia el manejador de mensajes a "HandleGeneralMessage".
    /// </summary>
    /// <param name="sender">El objeto que env�a el evento.</param>
    /// <param name="e">El mensaje recibido.</param>
    private void HandleSessionMessage(object sender, MessageEventArgs e)
    {
        if (TryDeserializeMessage(e.Data, ref receivedMessage))
        {
            Session = receivedMessage.data?.ToString();
            Debug.Log(Session);
            //GameManager.Instance.ChangeWsTxt("Session created: " + Session);
            if(Session == "" || Session == null)
            {
                Debug.LogError("Connection with websocket failed");
                return;
            }
            else
            {
                /////GameManager.Instance.SetWsConnection(true);
                /////GameManager.Instance.SetWsTryingToConnect(false);
                ServerMessage.SendInfoInitial();
            }
            ws.OnMessage -= HandleSessionMessage;
            ws.OnMessage += HandleGeneralMessage;
        }
    }

    /// <summary>
    /// Maneja los mensajes generales recibidos del servidor una vez establecida la sesi�n.
    /// </summary>
    /// <param name="sender">El objeto que env�a el evento.</param>
    /// <param name="e">El mensaje recibido.</param>
    private void HandleGeneralMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("Action Message Recieved");
        if (TryDeserializeMessage(e.Data, ref receivedMessage))
        {
            Debug.Log("Message was deserialized correctly, proceeded to take action");
            actionFlag = true;
        }
    }

    /// <summary>
    /// Env�a un mensaje al servidor a trav�s del WebSocket.
    /// </summary>
    /// <param name="message">El mensaje a enviar.</param>
    public void SendWebSocketMessage(ClassRoomVR.MessageSent message)
    {
        if (ws != null && ws.IsAlive)
        {
            Debug.Log("MESSAGE: " + message);
            var jsonData = JsonConvert.SerializeObject(message);
            Debug.Log("DATA: " + jsonData);
            ws.SendAsync(jsonData, null);
        }
        else
        {
            Debug.LogWarning("La conexi�n WebSocket no est� activa.");
        }
    }

    /// <summary>
    /// Si se recibe una acci�n del servidor, se procesa.
    /// </summary>
    private void Update()
    {
        if (actionFlag)
        {
            actionFlag = false;
            HandleAction();
        }
    }

    /// <summary>
    /// Procesa la acci�n recibida desde el servidor.
    /// </summary>
    private void HandleAction()
    {
        Debug.Log("Handling Action from server!");
        var studentController = ClassManager.Instance.GetStudentsController();
        if (int.TryParse(receivedMessage.id, out int studentId) && studentId >= 0)
        {
            // Faltar al respeto/Sentarse juntos/Levantarse

            //Restart
            if (studentId == 3){
                GameManager.Instance.LoadMainMenu();
            }
            else if(!GameManager.Instance.IsPause)
            {
                Debug.Log(studentId + " student is doing something disruptive!");
                studentController.DoSomethingDisruptive(studentId);
            }
        }
        else if (!GameManager.Instance.IsPause)
        {
            // Mensajes escritos desde servidor
            studentController.PlaySentence(receivedMessage.data.ToString());
        }
    }

    /// <summary>
    /// Intenta deserializar el mensaje recibido en un objeto MessageReceived.
    /// </summary>
    /// <param name="data">Datos del mensaje en formato JSON.</param>
    /// <param name="message">Referencia al objeto MessageReceived donde se almacenar� el mensaje deserializado.</param>
    /// <returns>True si el mensaje se deserializ� correctamente, False en caso contrario.</returns>
    private bool TryDeserializeMessage(string data, ref MessageReceived message)
    {
        try
        {
            message = JsonConvert.DeserializeObject<MessageReceived>(data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al procesar el mensaje: {ex.Message}");
            return false;
        }
    }
        
    /// <summary>
    /// Desconecta el WebSocket cerrando la conexi�n de manera segura.
    /// </summary>
    public void Disconnect()
    {
        if (ws == null) return;
        /////GameManager.Instance.SetWsConnection(false);
        if (ws.IsAlive) ws?.Close();
        ws = null;
    }

    /// <summary>
    /// Maneja la desconexi�n cuando se cierra la aplicaci�n.
    /// </summary>
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}

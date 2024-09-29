using WebSocketSharp;
using UnityEngine;
using System;
using Newtonsoft.Json;
using ClassRoomVR;

/// <summary>
/// Clase que maneja la conexión WebSocket y la comunicación con el servidor.
/// Implementa un Singleton genérico para ser accesible globalmente.
/// </summary>
public class WsClient : GenericSingleton<WsClient>
{
    // Indicador de acción pendiente.
    private bool actionFlag = false;

    // Conexión WebSocket.
    private static WebSocket ws;

    // Mensaje recibido.
    private MessageReceived receivedMessage;

    // Identificador del dispositivo.
    private string _deviceId;

    // Propiedad que indica si la conexión está activa.
    public bool IsConnected { get; private set; }

    // Sesión actual.
    public string Session { get; private set; }

    /// <summary>
    /// Inicializa el estado de conexión y el identificador del dispositivo.
    /// </summary>
    private void Start()
    {
        IsConnected = false;
        _deviceId = SystemInfo.deviceUniqueIdentifier;
    }

    /// <summary>
    /// Inicia la conexión WebSocket al servidor especificado.
    /// Maneja eventos de apertura, mensajes y cierre.
    /// </summary>
    public void StartConnection()
    {
        try
        {
            ws = new WebSocket("wss://cyclops.uab.cat/game/");
            ws.OnOpen += HandleOnOpen;
            ws.OnMessage += HandleSessionMessage;
            ws.OnClose += HandleOnClose;
            ws.ConnectAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError("Error en la conexión WebSocket: " + ex.Message);
        }
    }

    /// <summary>
    /// Maneja el evento cuando se cierra la conexión WebSocket.
    /// Actualiza el estado de conexión y pausa el juego si no fue una desconexión limpia.
    /// </summary>
    /// <param name="sender">El objeto que envía el evento.</param>
    /// <param name="e">Información del evento de cierre.</param>
    private void HandleOnClose(object sender, CloseEventArgs e)
    {
        IsConnected = false;
        Session = e.WasClean ? null : Session;

        if (!e.WasClean)
        {
            Debug.Log("Conexión perdida. No hay Internet.");
            GameManager.Instance.Pause(true);
        }
    }

    /// <summary>
    /// Maneja el evento cuando se abre la conexión WebSocket.
    /// Envía un mensaje al servidor con el tipo de mensaje "CreateSession".
    /// </summary>
    /// <param name="sender">El objeto que envía el evento.</param>
    /// <param name="e">Información del evento de apertura.</param>
    private void HandleOnOpen(object sender, EventArgs e)
    {
        IsConnected = true;
        var message = new MessageSent(MessageType.CreateSession, Session, _deviceId);
        SendWebSocketMessage(message);
    }

    /// <summary>
    /// Maneja los mensajes iniciales de sesión recibidos del servidor.
    /// Si se recibe un mensaje válido, actualiza la sesión y cambia el manejador de mensajes a "HandleGeneralMessage".
    /// </summary>
    /// <param name="sender">El objeto que envía el evento.</param>
    /// <param name="e">El mensaje recibido.</param>
    private void HandleSessionMessage(object sender, MessageEventArgs e)
    {
        if (TryDeserializeMessage(e.Data, ref receivedMessage))
        {
            Session = receivedMessage.data?.ToString();
            Debug.Log(Session);
            ws.OnMessage -= HandleSessionMessage;
            ws.OnMessage += HandleGeneralMessage;
        }
    }

    /// <summary>
    /// Maneja los mensajes generales recibidos del servidor una vez establecida la sesión.
    /// </summary>
    /// <param name="sender">El objeto que envía el evento.</param>
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
    /// Envía un mensaje al servidor a través del WebSocket.
    /// </summary>
    /// <param name="message">El mensaje a enviar.</param>
    public void SendWebSocketMessage(ClassRoomVR.MessageSent message)
    {
        if (ws != null && ws.IsAlive)
        {
            var jsonData = JsonConvert.SerializeObject(message);
            ws.SendAsync(jsonData, null);
        }
        else
        {
            Debug.LogWarning("La conexión WebSocket no está activa.");
        }
    }

    /// <summary>
    /// Si se recibe una acción del servidor, se procesa.
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
    /// Procesa la acción recibida desde el servidor.
    /// </summary>
    private void HandleAction()
    {
        Debug.Log("Handling Action from server!");
        var studentController = ClassManager.Instance.GetStudentsController();
        if (int.TryParse(receivedMessage.id, out int studentId) && studentId >= 0)
        {
            // Faltar al respeto/Sentarse juntos/Levantarse
            Debug.Log(studentId + " student is doing something disruptive!");
            studentController.DoSomethingDisruptive(studentId);
        }
        else
        {
            // Mensajes escritos desde servidor
            studentController.PlaySentence(receivedMessage.data.ToString());
        }
    }

    /// <summary>
    /// Intenta deserializar el mensaje recibido en un objeto MessageReceived.
    /// </summary>
    /// <param name="data">Datos del mensaje en formato JSON.</param>
    /// <param name="message">Referencia al objeto MessageReceived donde se almacenará el mensaje deserializado.</param>
    /// <returns>True si el mensaje se deserializó correctamente, False en caso contrario.</returns>
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
    /// Desconecta el WebSocket cerrando la conexión de manera segura.
    /// </summary>
    public void Disconnect()
    {
        ws?.Close();
    }

    /// <summary>
    /// Maneja la desconexión cuando se cierra la aplicación.
    /// </summary>
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}

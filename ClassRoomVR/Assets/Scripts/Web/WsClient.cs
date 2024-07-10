using WebSocketSharp;
using UnityEngine;
using System;
using Newtonsoft.Json;
using ClassRoomVR;

public class WsClient : GenericSingleton<WsClient>
{
    public static bool accion = false;
    public static WebSocket ws;
    public string session;
    ClassRoomVR.MessageReceived recMes;
    public string idDevice;

    public bool connected;
    private void Start()
    {
        connected = false;
        idDevice = SystemInfo.deviceUniqueIdentifier;
        //TEMPORAL
        //StartConnection();
        //Invoke(nameof(Info), 2f);
    }

    void Info() 
    {
        ServerMessage.SendInfoInitial();
    }
    public void StartConnection()
    {
        try
        {
            ws = new WebSocket("wss://cyclops.uab.cat/game/");
            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnSessionMessage;
            ws.OnClose += Ws_OnClose;
            ws.ConnectAsync();
        }
        catch (Exception ex) { Debug.LogError("Error en la conexi�n: " + ex.Message); }
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        connected = false;
        if (e.WasClean) {
            session = null;
        }
        else
        {
            Debug.Log("nO HAY INTERNET2");
            GameManager.Instance.Pause(true);
        }
    }

    void Ws_OnOpen(object sender, EventArgs e)
    {
        connected = true;
        string jsonData = JsonConvert.SerializeObject(new ClassRoomVR.MessageSent(ClassRoomVR.MessageType.CreateSession, session, idDevice));
        ws.SendAsync(jsonData, null);
    }

    
    void Ws_OnSessionMessage(object sender, MessageEventArgs e)
    {
        try
        {
            recMes = JsonConvert.DeserializeObject<ClassRoomVR.MessageReceived>(e.Data);
            Debug.Log(recMes.data.ToString());
            session = recMes.data.ToString();
            ws.OnMessage += Ws_OnMessage;
            ws.OnMessage -= Ws_OnSessionMessage;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + e.Data);
        }
    }

   
    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            accion = true;
            recMes = JsonConvert.DeserializeObject<MessageReceived>(e.Data);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + e.Data);
        }
    }


    public void Ws_SendMessage(ClassRoomVR.MessageSent mes)
    {
        if (ws != null && ws.IsAlive)
        {
            string jsonData = JsonConvert.SerializeObject(mes);
            ws.SendAsync(jsonData, null);
        }
        else
        {
           // Debug.LogWarning("La conexi�n WebSocket no est� activa.");
        }
    }


    private void Update()
    {
        if (accion)
        {
            accion = false;
            if (Convert.ToInt32(recMes.id)>=0)
            {
               
                ClassManager.Instance.GetStudentsController().DoSomethingDisruptive(Convert.ToInt32(recMes.id));
            }
            else
            {
                ClassManager.Instance.GetStudentsController().PlaySentence(recMes.data.ToString());
            }
        }
    }

    public void ToggleCon()
    {
        if (!connected) StartConnection();
        else Disconnect();
    }


    public void Disconnect()
    {
        if (ws != null)
        {
            ws.Close();
        }
    }

    public bool isAlive() { return ws != null ? ws.IsAlive : false; }

    private void OnApplicationQuit()
    {
        Disconnect();
    }


}
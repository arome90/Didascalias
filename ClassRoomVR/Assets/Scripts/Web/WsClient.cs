using WebSocketSharp;
using WebSocketSharp.Server;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using ClassRoomVR;
using UnityEngine.Events;

#region Server
//public class WsServer : MonoBehaviour
//{
//    public static bool accion = false;
//    public class Echo : WebSocketBehavior
//    {
//        protected override void OnMessage(MessageEventArgs e)
//        {
//            int n = e.Data[0] - '0';
//            if (n >= 0)
//            {
//                accion = true;
//                //ClassRoomVR.WebActions.ProcessMessage(n);
//            }
//            Debug.Log("Received message from Echo client: " + e.Data);

//            Send(e.Data);
//        }
//    }

//    WebSocketServer wssv;

//    void Start()
//    {

//        // URL del servidor WebSocket
//        // string serverURL = "ws://tu-servidor-websocket.com";
//        // string serverURL = "ws://dear-booming-quill.glitch.me/";


//        wssv = new WebSocketServer("ws://127.0.0.1:8080");

//        wssv.AddWebSocketService<Echo>("/Echo");


//        wssv.Start();
//        Debug.Log("WS server started on ws://127.0.0.1:7890/Echo");



//    }


//    private void Update()
//    {
//        if (accion)
//        {
//            accion = false;
//            ClassRoomVR.ClassManager.Instance.GetStudentsController().DoSomethingDisruptive(0);

//        }
//    }

//    // Aseg�rate de cerrar la conexi�n WebSocket cuando salgas de la aplicaci�n
//    private void OnDestroy()
//    {
//        if (wssv != null)
//        {
//            wssv.Stop();

//        }
//    }
//}
#endregion

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
        StartConnection();
        Invoke(nameof(Info), 2f);
    }

    void Info() 
    {
        ServerMessage.SendInfoInitial();
        ClassManager.Instance.GetStudentsController().PlaySentence("Hola buenas tardes chicos ");

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



//}
//string getKey(int len)
//{
//    string res = string.Empty;
//    const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
//    char[] charac = characters.ToCharArray();
//    for (int i = 0; i < len; i++) { res += charac[UnityEngine.Random.Range(0, charac.Length - 1)]; }
//    return res;
//}
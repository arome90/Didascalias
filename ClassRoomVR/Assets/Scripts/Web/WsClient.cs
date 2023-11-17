using WebSocketSharp;
using WebSocketSharp.Server;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
#region Server
//public class WsServer : MonoBehaviour
//{
//   public  static bool accion = false;
//     public class Echo : WebSocketBehavior
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
//       // string serverURL = "ws://dear-booming-quill.glitch.me/";


//             wssv = new WebSocketServer("ws://127.0.0.1:8080");

//            wssv.AddWebSocketService<Echo>("/Echo");


//        wssv.Start();
//            Debug.Log("WS server started on ws://127.0.0.1:7890/Echo");



//    }


//    private void Update()
//    {
//        if (accion) 
//        {
//            accion = false;
//            ClassRoomVR.ClassManager.Instance.GetStudentsController().DoSomethingDisruptive(0);

//        }
//    }

//    // Asegúrate de cerrar la conexión WebSocket cuando salgas de la aplicación
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
    ClassRoomVR.UnityMessage mes;
    public bool conected = false;

    public void StartConnection()
    {
        try
        {
            conected = true;
            ws = new WebSocket("wss://cyclops.uab.cat/game/");
            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnSessionMessage;
            ws.ConnectAsync();
        }
        catch (Exception ex) { Debug.LogError("Error en la conexión: " + ex.Message); }
    }

    void Ws_OnOpen(object sender, EventArgs e)
    {
        Debug.Log("open");
        string jsonData = JsonConvert.SerializeObject(new ClassRoomVR.UnityMessage(ClassRoomVR.MessageType.CreateSession, null));
        ws.SendAsync(jsonData, null);
    }

    void Ws_OnSessionMessage(object sender, MessageEventArgs e)
    {
        try
        {
            mes = JsonConvert.DeserializeObject<ClassRoomVR.UnityMessage>(e.Data);
            session = mes.data.ToString();
            ws.OnMessage += Ws_OnMessage;
            ws.OnMessage -= Ws_OnSessionMessage;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + e.Data);
        }
    }

    [Serializable]
    struct A { public string type; public string id; }
    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        try
        {
            accion = true;
            mes.data = JsonConvert.DeserializeObject<A>(e.Data).id;
            Debug.Log("Mes" + " " + mes.data.ToString());
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + e.Data);
        }
    }


    public void Ws_SendMessage(ClassRoomVR.UnityMessage mes)
    {
        if (ws != null && ws.IsAlive)
        {
            string jsonData = JsonConvert.SerializeObject(mes);
            ws.SendAsync(jsonData, null);
        }
        else
        {
            Debug.LogWarning("La conexión WebSocket no está activa.");
        }
    }

    private void Update()
    {
        if (accion)
        {
            accion = false;
            Debug.Log(Convert.ToInt32(mes.data));
            ClassRoomVR.ClassManager.Instance.GetStudentsController().DoSomethingDisruptive(Convert.ToInt32(mes.data));
        }
    }

    public void ToggleCon()
    {
        if (!conected) StartConnection();
        else Disconnect();
    }


    public void Disconnect()
    {
        conected = false;
        if (ws != null) ws.CloseAsync();
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
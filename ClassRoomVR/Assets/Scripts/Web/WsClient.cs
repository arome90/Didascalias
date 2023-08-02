using WebSocketSharp;
using WebSocketSharp.Server;
using UnityEngine;
using System;
using Newtonsoft.Json;
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
//            ClassRoomVR.GameManager.Instance.GetClassManager().GetStudentsController().DoSomethingDisruptive(0);

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

public class WsClient : MonoBehaviour
{
    public static bool accion = false;
    static WebSocket ws;
    void Start()
    {
        Debug.Log("start");
        try
        {
            WebSocket ws = new WebSocket("wss://cyclops.uab.cat/game/");
            // WebSocket ws = new WebSocket("ws://127.0.0.1:8080/Echo");
            ws.OnMessage += Ws_OnMessage;
            
            ws.Connect();

            Ws_SendMessage(new MessageData { type = Type.CreateSession, value = 1 });
        }
        catch { Debug.Log("Error en conexion"); }
    }


    private static void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        accion = true;
        //int n = e.Data[0] - '0';
        //if (n >= 0)
        //{
        //    accion = true;
        //    //ClassRoomVR.WebActions.ProcessMessage(n);
        //}
        try
        {
            MessageData mes = JsonConvert.DeserializeObject<MessageData>(e.Data);
            Debug.Log("Received message from Echo client: " + e.Data);
        }
        catch(Exception ex) 
        {
            Debug.Log("Idk" + e.Data);
        }

    }

    private static void Ws_SendMessage(MessageData mes) 
    {
        string jsonData = JsonConvert.SerializeObject(mes);
        ws.Send(jsonData);
    }

    private void Update()
    {
        if (accion)
        {
            accion = false;
            ClassRoomVR.GameManager.Instance.GetClassManager().GetStudentsController().DoSomethingDisruptive(0);

        }
    }

   

    private void OnDestroy()
    {
        if (ws != null) ws.Close();
    }



    [System.Serializable]
    public class MessageData
    {
        public Type type;
        public int value;
    }


    public enum Type{ NewSpectator,CreateSession,Action,SendSessionInfo }


    [System.Serializable]
    public class SessionInfo
    {
        //.. Añadir todos los datos que se tienen que enviar en una session 
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
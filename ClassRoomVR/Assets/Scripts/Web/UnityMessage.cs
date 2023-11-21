using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ClassRoomVR
{
    [System.Serializable]
    public class UnityMessage
    {
        public MessageType type;
        public string session;
        public object data;
        public UnityMessage(MessageType messageType,string sessionid ,object dat)
        {
            session = sessionid;
            type = messageType;
            data = dat;
        }
    }

    public enum MessageType
    {
        NewSpectator,
        CreateSession,
        Init,
        Info,
        Action,
        Resume
    }

    // Clases para mensajes específicos




    [System.Serializable]
    public struct VectorJson
    {
        public float x;
        public float y;

        public VectorJson(float xValue, float yValue)
        {
            x = xValue;
            y = yValue;
        }
    }

    [System.Serializable]
    public struct AlumnoInit
    {
        public string nombre;
        public int id;
        public VectorJson posicion;
        //Añadir caracteristicas que no se modifican
    }

    [System.Serializable]
    public struct AlumnoFeatures
    {
        public string nombre;
        public VectorJson posicion;
        StudentVariables var;
    }


    [System.Serializable]
    public struct CatalogoOpciones
    {
        public string[] opcionesGlobales;
        public string[] opcionesIndividuales;
    }




    [System.Serializable]
    public struct InitialMessageData
    {
        public AlumnoInit[] alumnosPosiciones;
        public string horaClase;
        public long tiempoSesion;
        public CatalogoOpciones catalogo;

        public InitialMessageData(AlumnoInit[] posiciones, string hora, long sesion, CatalogoOpciones cat)
        {
            
            alumnosPosiciones = posiciones;
            horaClase = hora;
            tiempoSesion = sesion;
            catalogo = cat;
        }
    }






    [System.Serializable]
    public struct MessageData
    {
        public InputVariables input;
        //public Clima clima;
        //  public AlumnoFeatures[] students;
        public MessageData( InputVariables input)
        {
            this.input = input;
        }

    }
    public static class ServerMessage
    {

        public static void SendInfoInitial()
        {
            AlumnoInit[] posiciones = new AlumnoInit[]
            {
                new AlumnoInit { nombre = "Alumno1", posicion = new VectorJson(1.0f, 0.0f) },
                new AlumnoInit { nombre = "Alumno2", posicion = new VectorJson(0.5f, 1.0f) }
            };
            string hora = "9:00 AM";
            long sesion = new System.DateTimeOffset(System.DateTime.Now).ToUnixTimeSeconds();
            CatalogoOpciones cat = new CatalogoOpciones();
            cat.opcionesGlobales = new string[] { "Faltar el respeto", "Sentarse juntos","Levantarse" };
            cat.opcionesIndividuales = new string[] { "Pelear", "Insultar" };
            InitialMessageData initData = new InitialMessageData(posiciones, hora, sesion, cat);
            WsClient.Instance.Ws_SendMessage(new UnityMessage(MessageType.Init, WsClient.Instance.session, initData));
        }


        public static void SendInfo()
        {
            Debug.Log("MandarInfo");
            MessageData initData = new MessageData(InputManager.Instance.input);
            WsClient.Instance.Ws_SendMessage(new UnityMessage(MessageType.Info, WsClient.Instance.session, initData));
            
        }


    }
}

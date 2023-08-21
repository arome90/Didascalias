using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ClassRoomVR
{
    [System.Serializable]
    public class UnityMessage
    {
        public MessageType type;
        public object data;
        public UnityMessage(MessageType messageType, object dat)
        {
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
    public struct AlumnoPosicion
    {
        public string nombre;
        public VectorJson posicion;
        //Añadir caracteristicas
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
        public AlumnoPosicion[] alumnosPosiciones;
        public string horaClase;
        public long tiempoSesion;
        public CatalogoOpciones catalogo;

        public InitialMessageData(AlumnoPosicion[] posiciones, string hora, long sesion, CatalogoOpciones cat)
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
        public InputManager input; 
        public Clima clima;
        public StudentVariables[] students;
    }


    public static class Prueba 
    {
        
        public static void CreateInfoInitial()
        {
            AlumnoPosicion[] posiciones = new AlumnoPosicion[]
            {
                new AlumnoPosicion { nombre = "Alumno1", posicion = new VectorJson(1.0f, 0.0f) },
                new AlumnoPosicion { nombre = "Alumno2", posicion = new VectorJson(0.5f, 1.0f) }
            };
            string hora = "9:00 AM";
            long sesion = new System.DateTimeOffset(System.DateTime.Now).ToUnixTimeSeconds();
            CatalogoOpciones cat = new CatalogoOpciones();
            cat.opcionesGlobales = new string[] { "Hacer ruido", "Tirar papeles" };
            cat.opcionesIndividuales = new string[] { "Pelear", "Insultar" };
            InitialMessageData initData = new InitialMessageData(posiciones, hora, sesion, cat);
            WsClient.Instance.Ws_SendMessage(new UnityMessage(MessageType.Init, initData));
        }

       

    }

}
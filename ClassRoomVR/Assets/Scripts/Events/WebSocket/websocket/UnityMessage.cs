using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ClassRoomVR
{
    [Serializable]
    public struct MessageReceived
    {
        public string type;
        public string id;
        public string data;
    }

    [Serializable]
    public struct MessageSent
    {
        public MessageType type;
        public string session;
        public object data;

        /// <summary>
        /// Constructor para enviar un mensaje con tipo, sesi�n y datos.
        /// </summary>
        /// <param name="messageType">El tipo de mensaje</param>
        /// <param name="sessionId">El ID de la sesi�n</param>
        /// <param name="dat">Los datos del mensaje</param>
        public MessageSent(MessageType messageType, string sessionId, object dat)
        {
            session = sessionId;
            type = messageType;
            data = dat;
            deviceID = WsClient.Instance._deviceId;
        }
    }

    /// <summary>
    /// Enumeraci�n para los tipos de mensajes que se pueden enviar/recibir.
    /// </summary>
    public enum MessageType
    {
        NewSpectator,
        CreateSession,
        Init,
        Info,
        Action,
        Resume
    }

    [Serializable]
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
    public struct AlumnoInit
    {
        public string nombre;
        public int id;
        public VectorJson posicion;
    }

    [Serializable]
    public struct AlumnoFeatures
    {
        public string nombre;
        public VectorJson posicion;
    }

    [Serializable]
    public struct CatalogoOpciones
    {
        public string[] opcionesGlobales;
        public string[] opcionesIndividuales;
    }

    [Serializable]
    public struct InitialMessageData
    {
        public AlumnoInit[] alumnosPosiciones;
        public string horaClase;
        public long tiempoSesion;
        public CatalogoOpciones catalogo;
        public string deviceID;

        /// <summary>
        /// Constructor para el mensaje inicial con los datos de los alumnos y el cat�logo de opciones.
        /// </summary>
        /// <param name="posiciones">Las posiciones de los alumnos</param>
        /// <param name="hora">La hora de la clase</param>
        /// <param name="sesion">El tiempo de la sesi�n en formato Unix</param>
        /// <param name="cat">El cat�logo de opciones disponibles</param>
        public InitialMessageData(AlumnoInit[] posiciones, string hora, long sesion, CatalogoOpciones cat)
        {
            alumnosPosiciones = posiciones;
            horaClase = hora;
            tiempoSesion = sesion;
            catalogo = cat;
            deviceID = WsClient.Instance._deviceId;
        }
    }

    [Serializable]
    public struct MessageData
    {
        public InputVariables input;
        public string deviceID;

        /// <summary>
        /// Constructor que recibe los datos de entrada.
        /// </summary>
        /// <param name="input">Los datos de entrada registrados</param>
        public MessageData(InputVariables input)
        {
            this.input = input;
            this.deviceID = SystemInfo.deviceUniqueIdentifier;
        }
    }

    public static class ServerMessage
    {
        /// <summary>
        /// Env�a el mensaje inicial con los datos de los alumnos y la configuraci�n de la clase.
        /// </summary>
        public static void SendInfoInitial()
        {
            if(!WsClient.Instance.IsConnected || WsClient.Instance.Session == null)
            {
                Debug.LogError("You must first detect a connection to send data");
                return;
            }

            Debug.Log("Detected connection on WebSocket! Session: " + WsClient.Instance.Session);

            var initData = CreateInitialMessageData();
            WsClient.Instance.SendWebSocketMessage(new MessageSent(MessageType.Init, WsClient.Instance.Session, initData));
        }

        /// <summary>
        /// Env�a el mensaje con los datos de entrada (input) registrados.
        /// </summary>
        public static void SendInfo()
        {
            var inputData = new MessageData(InputLogger.Instance.Input);
            WsClient.Instance.SendWebSocketMessage(new MessageSent(MessageType.Info, WsClient.Instance.Session, inputData));
        }

        /// <summary>
        /// Crea los datos del mensaje inicial para ser enviados a los clientes.
        /// </summary>
        /// <returns>Devuelve los datos iniciales para el mensaje</returns>
        private static InitialMessageData CreateInitialMessageData()
        {
            AlumnoInit[] posiciones = new AlumnoInit[]
            {
                new AlumnoInit { nombre = "Alumno1", posicion = new VectorJson(1.0f, 0.0f) },
                new AlumnoInit { nombre = "Alumno2", posicion = new VectorJson(0.5f, 1.0f) }
            };

            string hora = GetFormattedCurrentTime();
            long sesion = GetCurrentUnixTimestamp();

            var catalogo = new CatalogoOpciones
            {
                opcionesGlobales = new string[] { "Faltar el respeto", "Sentarse juntos", "Levantarse", "Restart" },
                opcionesIndividuales = new string[] { "Pelear", "Insultar" }
            };
            var data = new InitialMessageData(posiciones, hora, sesion, catalogo);

            return data;
        }

        /// <summary>
        /// Obtiene la hora actual formateada.
        /// </summary>
        /// <returns>Devuelve la hora actual en formato de cadena</returns>
        private static string GetFormattedCurrentTime()
        {
            return DateTime.Now.ToString("h:mm tt");
        }

        /// <summary>
        /// Obtiene el timestamp actual en formato Unix.
        /// </summary>
        /// <returns>Devuelve el timestamp actual en formato Unix</returns>
        private static long GetCurrentUnixTimestamp()
        {
            return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        }
    }
}

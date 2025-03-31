using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    [Serializable]
    public struct MessageReceived2
    {
        public string type;
        public string id;
        public string data;
    }

    [Serializable]
    public struct MessageSent2
    {
        public MessageType2 type;
        public string session;
        public object data;

        /// <summary>
        /// Constructor para enviar un mensaje con tipo, sesión y datos.
        /// </summary>
        /// <param name="messageType">El tipo de mensaje</param>
        /// <param name="sessionId">El ID de la sesión</param>
        /// <param name="dat">Los datos del mensaje</param>
        public MessageSent2(MessageType2 messageType, string sessionId, object dat)
        {
            session = sessionId;
            type = messageType;
            data = dat;
        }
    }

    /// <summary>
    /// Enumeración para los tipos de mensajes que se pueden enviar/recibir.
    /// </summary>
    public enum MessageType2
    {
        NewSpectator,
        CreateSession,
        Init,
        Info,
        Action,
        Resume
    }

    [Serializable]
    public struct VectorJson2
    {
        public float x;
        public float y;

        /// <summary>
        /// Constructor para un vector en formato JSON.
        /// </summary>
        /// <param name="xValue">El valor en X</param>
        /// <param name="yValue">El valor en Y</param>
        public VectorJson2(float xValue, float yValue)
        {
            x = xValue;
            y = yValue;
        }
    }

    [Serializable]
    public struct AlumnoInit2
    {
        public string nombre;
        public int id;
        public VectorJson2 posicion;
    }

    [Serializable]
    public struct AlumnoFeatures2
    {
        public string nombre;
        public VectorJson2 posicion;
    }

    [Serializable]
    public struct CatalogoOpciones2
    {
        public string[] opcionesGlobales;
        public string[] opcionesIndividuales;
    }

    [Serializable]
    public struct InitialMessageData2
    {
        public AlumnoInit2[] alumnosPosiciones;
        public string horaClase;
        public long tiempoSesion;
        public CatalogoOpciones2 catalogo;

        /// <summary>
        /// Constructor para el mensaje inicial con los datos de los alumnos y el catálogo de opciones.
        /// </summary>
        /// <param name="posiciones">Las posiciones de los alumnos</param>
        /// <param name="hora">La hora de la clase</param>
        /// <param name="sesion">El tiempo de la sesión en formato Unix</param>
        /// <param name="cat">El catálogo de opciones disponibles</param>
        public InitialMessageData2(AlumnoInit2[] posiciones, string hora, long sesion, CatalogoOpciones2 cat)
        {
            alumnosPosiciones = posiciones;
            horaClase = hora;
            tiempoSesion = sesion;
            catalogo = cat;
        }
    }

    [Serializable]
    public struct MessageData2
    {
        public InputVariables input;

        /// <summary>
        /// Constructor que recibe los datos de entrada.
        /// </summary>
        /// <param name="input">Los datos de entrada registrados</param>
        public MessageData2(InputVariables input)
        {
            this.input = input;
        }
    }

    public static class ServerMessage2
    {
        /// <summary>
        /// Envía el mensaje inicial con los datos de los alumnos y la configuración de la clase.
        /// </summary>
        public static void SendInfoInitial()
        {
            var initData = CreateInitialMessageData();
            WsClient2.Instance.SendWebSocketMessage(new MessageSent2(MessageType2.Init, WsClient2.Instance.Session, initData));
        }

        /// <summary>
        /// Envía el mensaje con los datos de entrada (input) registrados.
        /// </summary>
        public static void SendInfo()
        {
            var inputData = new MessageData2(InputLogger2.Instance.Input);
            WsClient2.Instance.SendWebSocketMessage(new MessageSent2(MessageType2.Info, WsClient2.Instance.Session, inputData));
        }

        /// <summary>
        /// Crea los datos del mensaje inicial para ser enviados a los clientes.
        /// </summary>
        /// <returns>Devuelve los datos iniciales para el mensaje</returns>
        private static InitialMessageData2 CreateInitialMessageData()
        {
            Didascalia_LocalizationManager l = Didascalia_LocalizationManager.Instance;
            l.GetTranslation("student1", Didascalia_LocalizationManager.TableCollections.WEB, out string student1);
            l.GetTranslation("student2", Didascalia_LocalizationManager.TableCollections.WEB, out string student2);
            AlumnoInit2[] posiciones = new AlumnoInit2[]
            {
                new AlumnoInit2 { nombre = student1, posicion = new VectorJson2(1.0f, 0.0f) },
                new AlumnoInit2 { nombre = student2, posicion = new VectorJson2(0.5f, 1.0f) }
            };
            l.GetTranslation("disrespectButton", Didascalia_LocalizationManager.TableCollections.WEB, out string disrespectButton);
            l.GetTranslation("fightButton", Didascalia_LocalizationManager.TableCollections.WEB, out string fightButton);
            l.GetTranslation("insultButton", Didascalia_LocalizationManager.TableCollections.WEB, out string insultButton);
            l.GetTranslation("sitButton", Didascalia_LocalizationManager.TableCollections.WEB, out string sitButton);
            l.GetTranslation("standButton", Didascalia_LocalizationManager.TableCollections.WEB, out string standButton);

            string hora = GetFormattedCurrentTime();
            long sesion = GetCurrentUnixTimestamp();

            var catalogo = new CatalogoOpciones2
            {
                opcionesGlobales = new string[] { disrespectButton, sitButton, standButton },
                opcionesIndividuales = new string[] { fightButton, insultButton }
            };

            return new InitialMessageData2(posiciones, hora, sesion, catalogo);
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Gestiona todo el input recopilado del usuario.
    /// </summary>
    public class InputLogger : GenericSingleton<InputLogger>
    {
        // Variable que se envía al servidor
        private InputVariables _input;
        public InputVariables Input => _input;

        // Variable que se escribe
        private InputVariablesToWrite _inputTW;
        public InputVariablesToWrite InputTW => _inputTW;

        // Datos estadísticos recopilados de la cabeza
        private HeadVariables _head;

        // Datos estadísticos recopilados de ambas manos
        private HandsManager _hands;

        //TO DO: VOICE VARIABLES
        // Datos estadísticos recopilados de la voz
        //private VoiceVariables _voice;

        // Lista interna para almacenar las diferencias de velocidad
        private float[] _list;

        // Tiempo de actualización de los datos
        [SerializeField] private float TimeUpdate = 1f;

        // Tiempo entre saves d datos
        [SerializeField] private float WriteStep = 1f;

        [Header("Escritura de los jsons")]
        [SerializeField] private string WriteDir = "writtenInfo";
        [SerializeField] private string WritePath = "test";
        private string WriteTo = "";

        private string JsonText = "";

        // Estructura para las variables mínimas y máximas
        [Serializable]
        public struct Variable
        {
            public float min;
            public float max;
        }

        /// <summary>
        /// Inicializa las variables, envía un primer paquete al servidor y comienza la recopilación de datos.
        /// </summary>
        private void Start()
        {
            _head = new HeadVariables();
            _hands = new HandsManager();
            _list = new float[3];

            CreateDir();
            IncrementPath();
            WriteTo = System.IO.Path.Combine(WriteDir, WritePath);
            Debug.Log($"nuevo path: {WriteTo}");
            /*
             * System.IO.StreamWriter file = new System.IO.StreamWriter(WriteTo);
            file.WriteLine("{ " + $"\"{WritePath}\": [");
            file.Close();
            */

            StartJson();

            InvokeRepeating(nameof(SendInfo), 2f, 2f);
            InvokeRepeating(nameof(WriteInfo), 1f, WriteStep);
            //AL COMENTAR ESTA LINEA EL JSON NO SE ENVIA (SE GUARDA SIN DIVIDIR)
            InvokeRepeating(nameof(SendDevInfo), 4f, 4f);
            StartCoroutine(UpdateInfo());
        }

        private void StartJson()
        {
            string session = WsClient.Instance.Session;
            System.IO.StreamWriter file = System.IO.File.CreateText(WriteTo);
            file.WriteLine("{ " + $"\"{session}\": [");
            file.Close();

            JsonText = "{ " + $"\"{session}\": [";
        }


        private void CreateDir(){
            WriteDir = System.IO.Path.Combine(Application.persistentDataPath, WriteDir);
            if (!System.IO.Directory.Exists(WriteDir)){
                System.IO.Directory.CreateDirectory(WriteDir);
            }
        }

        private void IncrementPath()
        {
            int it = 1;
            string ogPath = WritePath;
            while (System.IO.File.Exists(System.IO.Path.Combine(WriteDir, WritePath) + ".json") && it < 10)
            {
                if (!WritePath.EndsWith("]"))
                {
                    WritePath += "[1]";
                }
                else
                {
                    WritePath = $"{ogPath}[{it}]";
                }
                it++;
            }
            WritePath += ".json";
        }

        /// <summary>
        /// Corrutina que actualiza las estadísticas del input cada cierto tiempo.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateInfo()
        {
            while (true)
            {
                yield return new WaitForSeconds(TimeUpdate);
                _head.UpdateHead(TimeUpdate);
                _hands.UpdateHands(TimeUpdate);
            }
        }

        /// <summary>
        /// Envía la información recopilada al servidor.
        /// </summary>
        private void SendInfo()
        {
            List<float> actlist = new List<float>
            {
                _head.Velocidad.Variable,
                _hands.LeftHand.Velocity.Variable,
                _hands.RightHand.Velocity.Variable
            };

            int i = FindGreatestDistinction(actlist);
            //_input = new InputVariables(i, actlist[0], actlist[1], actlist[2]);
            _input = new InputVariables(i, actlist[0], actlist[1], actlist[2]);
            actlist.CopyTo(_list);
            Debug.Log("SendInfo desde InputLogger");
            ServerMessage.SendInfo();
        }

        /// <summary>
        /// Envía el json al servidor d desarrollo.
        /// </summary>
        private void SendDevInfo()
        {
            WriteEnd();
            Debug.Log("SendDevInfo desde InputLogger: " + JsonText);
            HttpClient.Instance.sendJson(JsonText);
            StartJson(); //return false??
        }

        /// <summary>
        /// Escribe la información recopilada.
        /// </summary>
        private void WriteInfo()
        {
            List<float> actlist = new List<float>
            {
                _head.Velocidad.Variable,
                _hands.LeftHand.Velocity.Variable,
                _hands.RightHand.Velocity.Variable
            };

            int i = FindGreatestDistinction(actlist);
            _inputTW = new InputVariablesToWrite(i, actlist[0], actlist[1], actlist[2]);

            string info = JsonUtility.ToJson(_inputTW, true);
            
            System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
            file.WriteLine(info + ", ");
            file.Close();
            fs.Close();

            JsonText += (info + ", ");
        }

        public void WriteToJson(string actionInfo, string eventType, string alumnx)
        {
            System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            string text = "{\n" + "    \"Time\": \"" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (actionInfo != "") text += "\" , \n    \"ActionInfo\": \"" + actionInfo + "\"";
            if (eventType != "") text += ", \n    \"EventType\": \"" + eventType + "\"";
            if (alumnx != "") text += ", \n    \"Alumnx(s)\": [" + alumnx + "] \n";
            text += "}, ";
            System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
            file.WriteLine(text);
            file.Close();
            fs.Close();

            JsonText += text;
        }

        /*
        public void WriteToJson(string text)
        {
            System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            text = "{\n" + "    \"Time\": \"" + DateTime.Now.ToString() + "\" , \n    \"ActionInfo\": \"" + text + "\" \n}, ";
            System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
            file.WriteLine(text);
            file.Close();
            fs.Close();

            JsonText += text;
        }
        */

        public void WriteEnd()
        {
            System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            string text = "{\n" + "    \"Time\": \"" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\" , \n    \"Test\": \"Enviado\" } ]}";
            System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
            file.WriteLine(text);
            file.Close();
            fs.Close();

            JsonText += text;
        }

        /// <summary>
        /// Encuentra la variable con la mayor diferencia respecto a la llamada anterior.
        /// </summary>
        /// <param name="actlist">Lista de valores actuales.</param>
        /// <returns>El índice con la mayor diferencia.</returns>
        public int FindGreatestDistinction(List<float> actlist)
        {
            float greatestDistinction = float.MinValue;
            int indexGreatestDistinction = -1;
            for (int i = 0; i < _list.Length; i++)
            {
                float distinction = Math.Abs(actlist[i] - _list[i]);
                if (distinction > greatestDistinction)
                {
                    greatestDistinction = distinction;
                    indexGreatestDistinction = i;
                }
            }
            return indexGreatestDistinction;
        }

        /// <summary>
        /// Actualiza las estadísticas de movimiento de la cabeza.
        /// </summary>
        private void Update()
        {
            _head.UpdateMotionHead();
        }

        /// <summary>
        /// Método para registrar una nueva acción realizada por el usuario.
        /// </summary>
        public void NewAction()
        {
            _hands.LeftHand.Velocity.NewAction();
        }

        /// <summary>
        /// Compara las velocidades promedio de las acciones realizadas por la mano izquierda.
        /// </summary>
        public void CompareVelocity()
        {
            double mediaRun = _hands.LeftHand.Velocity.Run.Mean;
            double actionMov = _hands.LeftHand.Velocity.ActionMean;

            if (mediaRun > actionMov)
            {
                Debug.Log($"La media de Run ({mediaRun}) es mayor que la de Action ({actionMov}).");
            }
            else if (mediaRun < actionMov)
            {
                Debug.Log($"La media de Action ({actionMov}) es mayor que la de Run ({mediaRun}).");
            }
        }
    }

    /// <summary>
    /// Estructura que se manda al servidor con las variables del input del usuario.
    /// </summary>
    [Serializable]
    public struct InputVariables
    {
        public int TypeMax { get; private set; }
        public float VelHead { get; private set; }
        public float VelHandIzq { get; private set; }
        public float VelHandDer { get; private set; }

        /// <summary>
        /// Constructor de InputVariables.
        /// </summary>
        /// <param name="typeMax">Índice de la variable con mayor distinción.</param>
        /// <param name="velHead">Velocidad de la cabeza.</param>
        /// <param name="velHandIzq">Velocidad de la mano izquierda.</param>
        /// <param name="velHandDer">Velocidad de la mano derecha.</param>
        public InputVariables(int typeMax, float velHead, float velHandIzq, float velHandDer)
        {
            TypeMax = typeMax;
            VelHead = velHead;
            VelHandIzq = velHandIzq;
            VelHandDer = velHandDer;
        }
    }

    /// <summary>
    /// Estructura que se guarda con las variables del input del usuario.
    /// </summary>
    [Serializable]
    public struct InputVariablesToWrite
    {
        public string Time;
        public int TypeMax;
        public float VelHead;
        public float VelHandIzq;
        public float VelHandDer;

        /// <summary>
        /// Constructor de InputVariables.
        /// </summary>
        /// <param name="typeMax">Índice de la variable con mayor distinción.</param>
        /// <param name="velHead">Velocidad de la cabeza.</param>
        /// <param name="velHandIzq">Velocidad de la mano izquierda.</param>
        /// <param name="velHandDer">Velocidad de la mano derecha.</param>
        public InputVariablesToWrite(int typeMax, float velHead, float velHandIzq, float velHandDer)
        {
            TypeMax = typeMax;
            VelHead = velHead;
            VelHandIzq = velHandIzq;
            VelHandDer = velHandDer;
            Time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"); ;
        }
    }
}

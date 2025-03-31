using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Gestiona todo el input recopilado del usuario.
    /// </summary>
    public class InputLogger2 : GenericSingleton<InputLogger2>
    {
        // Variable que se envía al servidor
        private InputVariables _input;
        public InputVariables Input => _input;

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

            InvokeRepeating(nameof(SendInfo), 1f, 1f);
            StartCoroutine(UpdateInfo());
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
            _input = new InputVariables(i, actlist[0], actlist[1], actlist[2]);
            actlist.CopyTo(_list);
            ServerMessage2.SendInfo();
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
    public struct InputVariables2
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
        public InputVariables2(int typeMax, float velHead, float velHandIzq, float velHandDer)
        {
            TypeMax = typeMax;
            VelHead = velHead;
            VelHandIzq = velHandIzq;
            VelHandDer = velHandDer;
        }
    }
}

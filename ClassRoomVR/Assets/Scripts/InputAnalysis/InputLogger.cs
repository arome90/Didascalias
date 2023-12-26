using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Gestiona todo el input recopilado del usuario
    /// </summary>
    public class InputLogger : GenericSingleton<InputLogger>
    {
        
        //Variable que se envia al servidor 
        public InputVariables input;
        //Datos estadisticos recopilados de la cabeza
        HeadVariables head;
        //Datos estadisticos recopilados de ambas manos
        public HandsManager hands;
        //Datos estadisticos recopilados de la voz
        VoiceVariables voice;
        private float[] list;
        //TO DO ? Lista para el mapeo
        //public List<Variable> variables = new List<Variable>(); // Valores aleatorios para el mapeo

        public float timeUpdate = 1f;
        public int windowSize = 5;
        //Para testear usar un texto y asi verlo en pantalla
        //[SerializeField] TMPro.TextMeshProUGUI text;

        [System.Serializable]
        public struct Variable
        {
            public float min;
            public float max;
        }
        //Inicializa la lista, hace un envio inicial al servidor de los datos de la clase al inicio 
        // de la sesion y comienza la corrutina de medir y enviar la informacion recopilada
        private void Start()
        {
            head = new HeadVariables(windowSize);
            hands = new HandsManager(windowSize);
            
            list = new float[3];
           
            InvokeRepeating(nameof(SendInfo), 1f, 1f);
            StartCoroutine(UpdateInfo());
        }
        
        /// <summary>
        /// Actualiza las estadisticas del input 
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateInfo()
        {
            while (true)
            {
                yield return new WaitForSeconds(timeUpdate);
                head.UpdateHead(timeUpdate);
                hands.UpdateHands(timeUpdate);
            }
        }

        //Manda las estadisticas deseadas en cada invoke
        private void SendInfo()
        {
            List<float> actlist = new List<float>
            {
                head.velocidad.Variable,
                hands.handIzq.velocidad.Variable,
                hands.handDer.velocidad.Variable
            };
            int i = FindGreatestDistinction(actlist);
            input = new InputVariables(i, actlist[0], actlist[1], actlist[2]);
            actlist.CopyTo(list);

            //Para testear
            //if (text)
            //{
            //    text.text = actlist[0].ToString() + "\n" + actlist[1].ToString() + "\n" + actlist[2].ToString();
            //}
            //Enviar la informacion al servidor 
            ServerMessage.SendInfo();
        }

        /// <summary>
        /// Encuentra la variable con mayor diferencia respecto a la llamada anterior 
        /// </summary>
        /// <param name="actlist"></param>
        /// <returns></returns>
        public int FindGreatestDistinction(List<float> actlist)
        {
            float greatestDistinction = float.MinValue;
            int indexGreatestDistinction = -1;
            for (int i = 0; i < list.Length; i++)
            {
                // Mapea la variable utilizando el valor minimo y maximo estudiados
                // float valorMapeado = math.remap(valores[i].min, valores[i].max, 0f, 100f, actlist[i]);

                // Calcula la diferencia
                // float diferencia = Math.Abs(valorMapeado - lista[i]);
                float distinction = Math.Abs(actlist[i] - list[i]);
                // Verifica si es la mayor diferencia encontrada hasta ahora
                if (distinction > greatestDistinction)
                {
                    greatestDistinction = distinction;
                    indexGreatestDistinction = i;
                }
            }
            return indexGreatestDistinction;
        }

        private void Update()
        {
            head.UpdateMotionHead();
        }


        public void NewAction() 
        {
            hands.handIzq.velocidad.NewAction();
        }
        public void CompareVelocity()
        {
            // las medias de velocidad general y durante el conflicto
            double mediaRun = hands.handIzq.velocidad.Run.Mean;
            //double mediaMov = hands.handIzq.velocidad.Mov.Mean;
            double actionMov = hands.handIzq.velocidad.ActionMean;

            // Compara las medias y registra los resultados con Debug.Log
            //if (mediaRun > mediaMov)
            //{
            //    Debug.Log($"La media de Run ({mediaRun}) es mayor que la de Mov ({mediaMov}).");
            //}
            //else if (mediaRun < mediaMov)
            //{
            //    Debug.Log($"La media de Mov ({mediaMov}) es mayor que la de Run ({mediaRun}).");
            //}



            // Compara las medias y registra los resultados con Debug.Log
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
    /// Struct que se manda al servidor 
    /// </summary>
    [Serializable]
    public struct InputVariables
    {
        public int typeMax;
        public float velHead;
        public float velHandIzq;
        public float velHandDer;

        public InputVariables(int typeMax, float velHead, float velHandIzq, float velHandDer)
        {
            this.typeMax = typeMax;
            this.velHead = velHead;
            this.velHandIzq = velHandIzq;
            this.velHandDer = velHandDer;
        }
    }

}


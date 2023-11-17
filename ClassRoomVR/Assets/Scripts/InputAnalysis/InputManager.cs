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
    
    public class InputManager : GenericSingleton<InputManager>
    {
        public InputVariables input;
        [SerializeField] HeadVariables head;
        [SerializeField] HandsManager hands;
        VoiceVariables voice;
        Transform transform;
        //        public HandSelector hand;
        private float[] lista ;
        public List<Variable> valores = new List<Variable>(); // Valores aleatorios para el mapeo

        [SerializeField] TMPro.TextMeshProUGUI text;



        [System.Serializable]
        public struct Variable
        {
            public float min;
            public float max;
        }

        private void Start()
        {
            lista = new float[3];
            Prueba.CreateInfoInitial();
            StartCoroutine(Measure());
            InvokeRepeating(nameof(sEnd), 1f, 1f);
        }
        private void sEnd() 
        {
            Prueba.CreateInfo();
        }

        private IEnumerator Measure()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                List<float> actlist = new List<float>();
                actlist.Add(head.velocidad.Variable);
                actlist.Add(hands.handIzq.velocidad.Variable);
                actlist.Add(hands.handDer.velocidad.Variable);
                int i= EncontrarVariableConMayorDiferencia(actlist);
                input = new InputVariables(i,actlist[0], actlist[1], actlist[2]);

                if (text)
                {
                    text.text = actlist[0].ToString() + "\n" + actlist[1].ToString() + "\n" + actlist[2].ToString();
                }


                actlist.CopyTo(lista);
            }
        }


        public int EncontrarVariableConMayorDiferencia(List<float> actlist)
        {
            float mayorDiferencia = float.MinValue;
            int indiceMayorDiferencia = -1;
            for (int i = 0; i < lista.Length; i++)
            {
                // Mapea la variable utilizando el valor minimo y maximo estudiados
               // float valorMapeado = math.remap(valores[i].min, valores[i].max, 0f, 100f, actlist[i]);

                // Calcula la diferencia
               // float diferencia = Math.Abs(valorMapeado - lista[i]);
                float diferencia = Math.Abs(actlist[i] - lista[i]);
                // Verifica si es la mayor diferencia encontrada hasta ahora
                if (diferencia > mayorDiferencia)
                {
                    mayorDiferencia = diferencia;
                    indiceMayorDiferencia = i;
                }
            }
            return indiceMayorDiferencia;
        }

    }
   
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


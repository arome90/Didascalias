using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class StudentBehavior : MonoBehaviour
    {
        public float nivelAtencion = 50.0f;// a>50 atento   a<50 despistado
        public float NivelAtencion => nivelAtencion;

        [SerializeField] bool disruptiveBehavior;
        [SerializeField] float timeOfDecision  = 2.5f;
        public float TimeOfDecision => timeOfDecision;

        [SerializeField] float addAttention=30;
        [SerializeField] float subAttention=20;

        [SerializeField] float contAdd=0.2f;
        [SerializeField] float contSub=0.1f;


        [SerializeField] float distanceFactorAdd=2.0f;
        [SerializeField] float distanceFactorSub=2.0f;

        Student st;

        private float nivelAtencionMedia;
        private int cont;

        Transform player;

        float contAddAux;
        float contSubAux;
        // Start is called before the first frame update
        void Start()
        {
            st = GetComponent<Student>();
            player = Camera.main.transform;
            InvokeRepeating("DecidirComportamiento", timeOfDecision, timeOfDecision);
            contAddAux = contAdd;
            contSubAux = contSub;
        }

        
     
        public void AddAttention()
        {

            float dis = 1 - Vector3.Distance(transform.position, player.transform.position)*distanceFactorAdd / 100;
            nivelAtencion += Mathf.Abs(addAttention*(1+contAdd)*dis);
            if (nivelAtencion > 100) { nivelAtencion = 100; }
            //0.01 o tener otra variable para el aumento
            contAdd += contAddAux;
            contSub = contSubAux;
        }

        public void SubAttention()
        {
            float dis = 1 - Vector3.Distance(transform.position, player.transform.position) * distanceFactorSub / 100;

            nivelAtencion -= subAttention * (1 + contSub)/dis;
            if (nivelAtencion <= 0) { nivelAtencion = 0; }
            contSub += contSubAux;
            contAdd = contAddAux;
        }


        public void SetDisruptive(bool value) { disruptiveBehavior = value; }



        //Método de toma de decisiones
        private void DecidirComportamiento()
        {

            if (st.IsStudentOnVision())
                AddAttention();
            else SubAttention();
            //Debug.Log(nivelAtencion + name);
            ////float v = nivelAtencion / 100.0f;
            ////if (Random.Range(0.0f, 1.0f) <= v)
            ////{

            //if (nivelAtencion > 70)
            //{
            //    // El personaje está atento
            //    st.PayAttention();

            //}
            //else
            //{
            //    // El personaje está despistado
            //    st.GetDistracted();
            //}


            //float v=nivelAtencion / 100.0f ;



            //if ()
            //{
            //    El personaje está realizando acciones disruptivas
            //     Implementa aquí el comportamiento correspondiente
            //}
        }

        /// <summary>
        /// Calcula la media de la atencion 
        /// </summary>
        public float CalculateMedia() 
        {
           
            float sum = nivelAtencionMedia * cont + nivelAtencion;
            cont++;
            nivelAtencionMedia = sum/cont;
            return nivelAtencionMedia;
        }

        public float GetNivelAtencionMedia()
        {
            return nivelAtencionMedia;
        }


    }
}
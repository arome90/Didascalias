using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class StudentBehavior : MonoBehaviour
    {
        [SerializeField] float nivelAtencion = 50.0f; // a>50 atento   a<50 despistado
        [SerializeField] bool disruptiveBehavior;
        [SerializeField] float timeOfDecision=2.5f;
        Student st;

        [SerializeField] float addAttention=30;
        [SerializeField] float subAttention=20;

        [SerializeField] float contAdd=0.2f;
        [SerializeField] float contSub=0.1f;


        float contAddAux;
        float contSubAux;
        // Start is called before the first frame update
        void Start()
        {
            st = GetComponent<Student>();
            InvokeRepeating("DecidirComportamiento", timeOfDecision, timeOfDecision);
            contAddAux = contAdd;
            contSubAux = contSub;
        }



        public void AddAttention()
        {
            nivelAtencion += addAttention*(1+contAdd);
            if (nivelAtencion > 100) { nivelAtencion = 100; }
            //0.01 o tener otra variable para el aumento
            contAdd += contAddAux;
            contSub = contSubAux;
        }

        public void SubAttention() 
        {
            nivelAtencion -= subAttention*(1+contSub);
            if (nivelAtencion < 0) { nivelAtencion = 0; }
            contSub += contSubAux;
            contAdd = contAddAux;
        }


        public void SetDisruptive(bool value) { disruptiveBehavior = value; }


        private bool VisionOfProfessor()
        {
            Plane[] cameraFrustum;
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var bounds = st.GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }




        //Método de toma de decisiones
        private void DecidirComportamiento()
        {

            if (VisionOfProfessor()) 
                AddAttention(); 
            else SubAttention();

            float v=nivelAtencion / 100.0f ;
            if (Random.Range(0.0f,1.0f) <= v)
            {
                // El personaje está atento
                st.PayAttention();

            }
            else
            {
                // El personaje está despistado
                st.GetDistracted();
            }


            //if ()
            //{
            //    El personaje está realizando acciones disruptivas
            //     Implementa aquí el comportamiento correspondiente
            //}
        }
    }
}
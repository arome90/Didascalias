using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class StudentBehavior : MonoBehaviour
    {
        [SerializeField] float nivelAtencion = 50.0f; // a>50 atento   a<50 despistado
        [SerializeField] bool disruptiveBehavior;

        Student st;

        // Start is called before the first frame update
        void Start()
        {
            st = GetComponent<Student>();
            InvokeRepeating("DecidirComportamiento", 2.5f, 2.5f);
        }



        public void AddAttention(float value)
        {
            nivelAtencion += value;
            if (nivelAtencion < 0) { nivelAtencion = 0; }
            else if (nivelAtencion > 100) { nivelAtencion = 100; }
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
            

            AddAttention(VisionOfProfessor() ? 20 : -20);
            float v=nivelAtencion / 100.0f;
            if (Random.Range(0.0f,1.0f) < v)
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class StudentAttentionCalculator : MonoBehaviour
    {
        [SerializeField] float media;
        private float mediaActual;
        Student[] students_;
        private int cont;
        private void Start()
        {
          cont = 0;
          students_= GetComponent<ClassManager>().GetStudents();
          InvokeRepeating("CalculateMedia",2.5f, 2.5f);
        }
        public void CalculateMedia()
        {
            mediaActual = 0;
            for (int i = 0; i < students_.Length; i++)
            {
                float mediaStudent = students_[i].GetBehavior().NivelAtencion;
                students_[i].GetBehavior().CalculateMedia();

                mediaActual += mediaStudent;
                
            }
            mediaActual /= students_.Length;

            float sum = media * cont + mediaActual;
            cont++;
            media = sum / cont;
            Debug.Log("Media actual: " + mediaActual);

        }
       
        private void OnApplicationQuit()
        {
            Debug.Log("Media final de atencion: "+media);

        }

    }

}
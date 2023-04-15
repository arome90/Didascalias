using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class Action : MonoBehaviour
    {
        [SerializeField] float distanceNear;

        StudentsController controller;
        GameObject player;
        BehaviorTree bh;
        List<Student> problematics;
        DisruptiveAction a;
        private void Start()
        {
            player = GameManager.Instance.GetPlayer();
            controller = GameManager.Instance.GetClassManager().GetStudentsController();
            controller.SetMode(StudentsController.TalkMode.None);
        }

        public void SetParameters(List<Student> st,DisruptiveAction dis) 
        {
            problematics = st;
            a = dis;
            if (a.risas)
            {
                Invoke("Risas", 3.0f);
            }
            bh = GetComponent<BehaviorTree>();
            bh.EnableBehavior();

        }
        /// <summary>
        /// Invoca un metodo en x segundos y empieza una corrutina para controlar si el profesor deja de
        /// observar al alumno durante unos segunos 
        /// </summary>
        public void Ignore()
        {
            Invoke("IgnoreTime", a.timeToReact);
            foreach (Student s in problematics)
            {
                StartCoroutine(IgnoreStudent(s));
            }
        }
        /// <summary>
        /// Invocacion para elegir el camino de ignore si no
        /// se ha elegido otro antes
        /// </summary>
        void IgnoreTime()
        {
            if ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                bh.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = a.reaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
        }
        
        public IEnumerator IgnoreStudent(Student s)
        {
            yield return new WaitUntil(() => s.IsStudentOnVision());
            yield return new WaitForSecondsRealtime(3);
            if (!s.IsStudentOnVision())
            {
                bh.GetVariable("Path").SetValue(3);
                if (a.reaccionClase != null)
                {
                    player.GetComponent<AudioSource>().clip = a.reaccionClase;
                    player.GetComponent<AudioSource>().Play();
                }
            }
            else if ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                StartCoroutine(IgnoreStudent(s));
            }
        }

        //private void OnEnable()
        //{
        //    bh = GetComponent<BehaviorTree>();
        //    bh.EnableBehavior();
        //    player.GetComponent<AudioSource>().Stop();
        //}

        /// <summary>
        /// Metodo para controlar el camino . Acercarse y hablar bien 
        /// </summary>
        public void Near()
        {
            foreach (Student s in problematics)
            {
                if (controller.GetMode() == StudentsController.TalkMode.Good && Vector3.Distance(s.transform.position, player.transform.position) <= distanceNear && s.IsStudentOnVision())
                {
                    bh.GetVariable("Path").SetValue(1);
                    controller.GoOut();
                }
            }
        }


        /// <summary>
        /// Metodo para controlar el camino  . Grito o falta de respeto
        /// </summary>
        public void Shout()
        {
            if (controller.GetMode() == StudentsController.TalkMode.Disrespect )
            {
                bh.GetVariable("Path").SetValue(2);
                player.GetComponent<AudioSource>().clip = a.ruido;
                player.GetComponent<AudioSource>().Play();
            }
        }

        //Suena el clip de sonrisas 
        void Risas()
        {
            if (a.reaccionClase != null)
            {
                player.GetComponent<AudioSource>().Stop();
                player.GetComponent<AudioSource>().clip = a.reaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
        }


        //Termina la clase
        public void Termina()
        {
            
            GameManager.Instance.GetClassManager().DisruptiveSituation = false;
            foreach (Student s in problematics)
            {
                s.SetNoProblematicStudent();
            }
            Debug.Log(bh.GetVariable("Path").GetValue());
            controller.SetMode(StudentsController.TalkMode.None);
            //StopAllCoroutines();
            //gameObject.SetActive(false);
            Destroy(gameObject);
        }

        
    }
}
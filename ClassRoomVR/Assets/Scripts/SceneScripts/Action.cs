using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        TextMeshProUGUI text;
        private void Start()
        {
            controller = ClassManager.Instance.GetStudentsController();
            controller.SetMode(TalkMode.None);
        }

        public void SetParameters(GameObject player,List<Student> st,DisruptiveAction dis, TextMeshProUGUI t) 
        {
            this.player= player;
            problematics = st;
            a = dis;
            if (a.laughter)
            {
                Invoke(nameof(Laughter), 2.0f);
            }
            bh = GetComponent<BehaviorTree>();
            InputLogger.Instance.NewAction();
            bh.EnableBehavior();
            text = t;
            text.text = "-1";

        }
        /// <summary>
        /// Invoca un metodo en x segundos y empieza una corrutina para controlar si el profesor deja de
        /// observar al alumno durante unos segunos 
        /// </summary>
        public void Ignore()
        {
            Invoke("IgnoreTime", a.reactionTime);
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
                Laughter();
            }
        }

        public IEnumerator IgnoreStudent(Student s)
        {
            float maxWaitTime = 10.0f; // Maximum wait time for optimization

            float startTime = Time.realtimeSinceStartup;
            while (s.IsStudentInFieldOfVision())
            {
                yield return null;

                if (Time.realtimeSinceStartup - startTime > maxWaitTime)
                {
                    // Break the loop if the maximum wait time is exceeded
                    break;
                }
            }
            //cambiar a 4 
            yield return new WaitForSecondsRealtime(1);

            if (!s.IsStudentInFieldOfVision())
            {
                bh.GetVariable("Path").SetValue(3);
                if (a.classLaughter != null)
                {
                    Laughter();
                }
            }
            else if ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                StartCoroutine(IgnoreStudent(s));
            }
        }



        //public IEnumerator IgnoreStudent(Student s)
        //{
        //    yield return new WaitWhile(() => s.IsStudentInFieldOfVision());
        //    yield return new WaitForSecondsRealtime(3);
        //    if (!s.IsStudentInFieldOfVision())
        //    {
        //        bh.GetVariable("Path").SetValue(3);
        //        if (a.classLaughter != null)
        //        {
        //            Laughter();
        //        }
        //    }
        //    else if ((int)bh.GetVariable("Path").GetValue() < 0)
        //    {
        //        StartCoroutine(IgnoreStudent(s));
        //    }
        //}


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
                if (Vector3.Distance(s.transform.position, player.transform.position) <= distanceNear && s.IsStudentInFieldOfVision())
                {
                    Debug.Log("Cerca"+ controller.GetMode());
                    if (controller.GetMode() == TalkMode.Good)
                    {
                        Debug.Log("Genial");
                        bh.GetVariable("Path").SetValue(1);

                    }
                    if (controller.GetMode() == TalkMode.Normal)
                    {

                        Debug.Log("Segundo camino");
                        bh.GetVariable("Path").SetValue(1);
                    }
                  //  controller.GoOut();
                }
            }
        }


        /// <summary>
        /// Metodo para controlar el camino  . Grito o falta de respeto
        /// </summary>
        public void Shout()
        {
            if (controller.GetMode() == TalkMode.Disrespect )
            {
                bh.GetVariable("Path").SetValue(2);
                player.GetComponent<AudioSource>().clip = a.noise;
                player.GetComponent<AudioSource>().Play();
                
            }
        }

        //Suena el clip de sonrisas 
        void Laughter()
        {
            if (a.classLaughter != null)
            {
                player.GetComponent<AudioSource>().Stop();
                player.GetComponent<AudioSource>().clip = a.classLaughter;
                player.GetComponent<AudioSource>().Play();
            }
        }


        //Termina la clase
        public void Finish()
        {
            ChangeText();
            ClassManager.Instance.DisruptiveSituation = false;
            foreach (Student s in problematics)
            {
                s.SetNotProblematicStudent();
            }
            Debug.Log(bh.GetVariable("Path").GetValue());
            controller.SetMode(TalkMode.None);
            Destroy(gameObject,2f);
            InputLogger.Instance.CompareVelocity();
        }


        private void ChangeText()
        {
            text.text = bh.GetVariable("Path").GetValue().ToString();
        }

        private void OnDestroy()
        {
            text.text = string.Empty;
        }

    }
}
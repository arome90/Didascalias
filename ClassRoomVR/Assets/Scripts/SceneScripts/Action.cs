using BehaviorDesigner.Runtime;
using MathNet.Numerics.Distributions;
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
        DisruptiveAction action;
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
            action = dis;
            if (action.laughter)
            {
                Invoke(nameof(Laughter), 2.0f);
            }
            bh = GetComponent<BehaviorTree>();
            InputLogger.Instance.NewAction();
            bh.EnableBehavior();
            if (t != null)
            {
                text = t;
                text.text = "-1";
            }

        }
        /// <summary>
        /// Invoca un metodo en x segundos y empieza una corrutina para controlar si el profesor deja de
        /// observar al alumno durante unos segunos 
        /// </summary>
        public void Ignore()
        {
            Invoke("IgnoreTime", action.reactionTime);
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
            float maxOutOfVisionTime = 20.0f; // Tiempo máximo que el estudiante debe estar fuera de visión
            float outOfVisionTimer = 0f; // Temporizador para el tiempo fuera de visión

            while ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                // Esperar al siguiente frame
                yield return null;

                // Comprobar si el estudiante está en el campo de visión
                if (s.IsStudentInFieldOfVision())
                {
                    // Si el estudiante está en el campo de visión, reiniciar el temporizador
                    outOfVisionTimer = 0f;
                }
                else
                {
                    // Si el estudiante no está en el campo de visión, incrementar el temporizador
                    outOfVisionTimer += Time.deltaTime;
                    // Comprobar si el estudiante ha estado fuera de visión durante el tiempo requerido
                    if (outOfVisionTimer >= maxOutOfVisionTime)
                    {
                        // Seleccionar el camino
                        bh.GetVariable("Path").SetValue(3);
                        if (action.classLaughter != null)
                        {
                            Laughter();
                        }
                        // Salir de la corrutina
                        yield break;
                    }
                }
            }
        }


        /// <summary>
        /// Metodo para controlar el camino . Acercarse y hablar bien 
        /// </summary>
        public void Near()
        {
            foreach (Student s in problematics)
            {
                if (Resolve(s))
                {
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
                }
            }
        }

        private bool Resolve(Student s) 
        {
            return (Vector3.Distance(s.transform.position, player.transform.position) <= distanceNear 
                && s.IsStudentInFieldOfVision()) 
                || ((controller.Resolutions & action.action) == action.action && controller.GetMode() != TalkMode.Disrespect);
        }
        /// <summary>
        /// Metodo para controlar el camino  . Grito o falta de respeto
        /// </summary>
        public void Shout()
        {
            if (controller.GetMode() == TalkMode.Disrespect )
            {
                bh.GetVariable("Path").SetValue(2);
                player.GetComponent<AudioSource>().clip = action.noise;
                player.GetComponent<AudioSource>().Play();
                
            }
        }

        //Suena el clip de sonrisas 
        void Laughter()
        {
            if (action.classLaughter != null)
            {
                player.GetComponent<AudioSource>().Stop();
                player.GetComponent<AudioSource>().clip = action.classLaughter;
                player.GetComponent<AudioSource>().Play();
            }
        }


        //Termina la clase
        public void Finish()
        {
            ChangeText();
            foreach (Student s in problematics)
            {
                Debug.Log(s.name);
                s.SetNotProblematicStudent();
            }
            Debug.Log(bh.GetVariable("Path").GetValue());
            controller.SetMode(TalkMode.None);
            Destroy(gameObject,2f);
            InputLogger.Instance.CompareVelocity();
        }


        private void ChangeText()
        {
            if (text != null)
            {
                text.text = bh.GetVariable("Path").GetValue().ToString();
            }
        }

        private void OnDestroy()
        {
            if (text != null)
            {
                text.text = string.Empty;
            }
            if (action != null)
            {
                action = null;
            }
        }

    }
}
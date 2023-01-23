using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

namespace ClassRoomVR
{
    public class Scene1 : MonoBehaviour
    {

        BehaviorTree bh;
        Student problematic;
        ClassManager classManager;
        ScenePackage sceneInfo;
        GameObject player;
        VoiceActivation voice;
        float distanceInitial;
       
        [SerializeField] AudioClip ruido;
        void Start()
        {
            bh = GetComponent<BehaviorTree>();
            sceneInfo = GameManager.Instance._packeges[0];
            player = GameManager.Instance.GetPlayer();
            voice = GameManager.Instance.voice;
            classManager = GameManager.Instance.GetClassManager();
            classManager.GetComponent<AudioSource>().clip = sceneInfo.before_bell;
            classManager.GetComponent<AudioSource>().Play();
        }

        /// <summary>
        /// Invoca un metodo en x segundos y empieza una corrutina para controlar si el profesor deja de
        /// observar al alumno durante unos segunos 
        /// </summary>
        /// <param name="time"></param>
        public void Ignore(float time) 
        {
            Invoke("IgnoreTime", time);
            StartCoroutine(IgnoreStudent());            
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
                player.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
        }
        /// <summary>
        /// Corrutina para controlar si el profesor ignora al alumno
        /// </summary>
        /// <returns></returns>
        public IEnumerator IgnoreStudent()
        {
            yield return new WaitUntil(() => !classManager.IsStudentOnVision(problematic));
            yield return new WaitForSecondsRealtime(3);
            if (!classManager.IsStudentOnVision(problematic))
            {
                bh.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
            else if((int)bh.GetVariable("Path").GetValue() < 0)
            {
                StartCoroutine(IgnoreStudent());
            }
        }



        /// <summary>
        /// Metodo para controlar el camino . Acercarse y hablar bien 
        /// </summary>
        public void Near()
        {
            if (classManager.GetMode()==ClassManager.TalkMode.Good && Vector3.Distance(problematic.transform.position,player.transform.position)<= distanceInitial/2)
            {
                bh.GetVariable("Path").SetValue(1);
                Student[] students = classManager.GetStudents();
                //Salen los demas alumnos de clase 
                for (int i =0;i < students.Length; i++)
                {
                    if(!students[i].GetProblematicStudent())StartCoroutine(WaitAndExit(students[i], i));
                }
            }
        }

        //Salida de un alumno de clase en x segundos 
        IEnumerator WaitAndExit(Student st,float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            st.MoveTo(classManager.door.position);
        }

        /// <summary>
        /// Metodo para controlar el camino  . Grito o falta de respeto
        /// </summary>
        public void Shout() 
        {
            if (classManager.GetMode()==ClassManager.TalkMode.Disrespect)
            {
                bh.GetVariable("Path").SetValue(2);
                classManager.GetComponent<AudioSource>().clip = ruido;
                classManager.GetComponent<AudioSource>().volume = 0.1f;
                classManager.GetComponent<AudioSource>().Play();
            }
        }

        /// <summary>
        /// Inicia la situacion 
        /// </summary>
        public void InitSituation()
        {
            Student[]students = classManager.GetStudents();
            bool pro = false;
            int i = 0;
            while (!pro && i < students.Length) 
            {
                pro = students[i].GetProblematicStudent();
                i++;
            }
            problematic = students[i-1];
            if (sceneInfo.problematicsAnimation != null)
            {
                problematic.PlayAnimation(sceneInfo.problematicsAnimation.name);
            }
            problematic.GetComponent<AudioSource>().clip = problematic.GetSex() == Student.Sex.Men
                ? sceneInfo.audioSituationMasculino : sceneInfo.audioSituationFemenino;
            problematic.GetComponent<AudioSource>().Play();
            Invoke("Risas", 2f);

            bh.GetVariable("AccionAlumno").SetValue(true);
            distanceInitial = Vector3.Distance(problematic.transform.position,player.transform.position);
            voice.ActivateWit();
            
        }

        //Suena el clip de sonrisas 
        void Risas() 
        {
            if (sceneInfo.audioReaccionClase != null)
            {
                classManager.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                classManager.GetComponent<AudioSource>().Play();
            }
        }

        //Termina la clase
        public void Termina() 
        {
            classManager.GetComponent<AudioSource>().clip = sceneInfo.after_bell;
            classManager.GetComponent<AudioSource>().Play();
        }
    }
}

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
        ScenePackage sceneInfo;
        GameObject player;
        float distanceInitial;
        void Start()
        {
            bh = GetComponent<BehaviorTree>();
            sceneInfo = GameManager.Instance._packeges[0];
            player = GameManager.Instance.GetPlayer();
        }

        public void Ignore() 
        {
            if ((int)bh.GetVariable("Path").GetValue() < 0) 
            {
              bh.GetVariable("Path").SetValue(3);
            }
        }

         

        public void Near()
        {
            if (Vector3.Distance(problematic.transform.position,GameManager.Instance.GetPlayer().transform.position)<= distanceInitial/2)
            {
                bh.GetVariable("Path").SetValue(1);
            }
        }

        public void Shout() 
        {
            if (Input.GetKeyUp(KeyCode.O))
            {
                bh.GetVariable("Path").SetValue(2);
            }
        }


        public void InitSituation()
        {
             Student[]students = GameManager.Instance.GetClassManager().GetStudents();
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
            distanceInitial = Vector3.Distance(problematic.transform.position, GameManager.Instance.GetPlayer().transform.position);
        }


        void Risas() 
        {
            if (sceneInfo.audioReaccionClase != null)
            {
               GameManager.Instance.GetClassManager().GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                GameManager.Instance.GetClassManager().GetComponent<AudioSource>().Play();
            }
        }

        public void Termina() { }
    }
}

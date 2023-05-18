using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

namespace ClassRoomVR
{
    public class Scene1 : MonoBehaviour
    {
        private BehaviorTree behaviorTree;
        private Student problematicStudent;
        private ClassManager classManager;
        private ScenePackage sceneInfo;
        private GameObject player;
        private float initialDistance;

        [SerializeField] private AudioClip noiseClip;

        private void Start()
        {
            behaviorTree = GetComponent<BehaviorTree>();
            sceneInfo = GameManager.Instance.GetPackageAtIndex(0);
            player = GameManager.Instance.GetPlayer();
            classManager = GameManager.Instance.GetClassManager();
            classManager.GetComponent<AudioSource>().clip = sceneInfo.beforeClassBell;
            classManager.GetComponent<AudioSource>().Play();
        }

        public void Ignore(float time)
        {
            Invoke("IgnoreTime", time);
            StartCoroutine(IgnoreStudent());
        }

        private void IgnoreTime()
        {
            if ((int)behaviorTree.GetVariable("Path").GetValue() < 0)
            {
                behaviorTree.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
                player.GetComponent<AudioSource>().Play();
            }
        }

        public IEnumerator IgnoreStudent()
        {
            yield return new WaitUntil(() => !classManager.GetStudentsController().IsStudentInFieldOfVision(problematicStudent));
            yield return new WaitForSecondsRealtime(3);
            if (!classManager.GetStudentsController().IsStudentInFieldOfVision(problematicStudent))
            {
                behaviorTree.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
                player.GetComponent<AudioSource>().Play();
            }
            else if ((int)behaviorTree.GetVariable("Path").GetValue() < 0)
            {
                StartCoroutine(IgnoreStudent());
            }
        }

        public void Near()
        {
            if (classManager.GetStudentsController().GetMode() == TalkMode.Good &&
                Vector3.Distance(problematicStudent.transform.position, player.transform.position) <= initialDistance / 2)
            {
                behaviorTree.GetVariable("Path").SetValue(1);
                Student[] students = classManager.GetStudents();
                foreach (Student student in students)
                {
                    if (!student.IsProblematicStudent())
                        StartCoroutine(WaitAndExit(student, students.Length));
                }
            }
        }

        private IEnumerator WaitAndExit(Student student, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            student.MoveTo(classManager.GetStudentsController().Door.position);
        }

        public void Shout()
        {
            if (classManager.GetStudentsController().GetMode() == TalkMode.Disrespect)
            {
                behaviorTree.GetVariable("Path").SetValue(2);
                classManager.GetComponent<AudioSource>().clip = noiseClip;
                classManager.GetComponent<AudioSource>().volume = 0.1f;
                classManager.GetComponent<AudioSource>().Play();
            }
        }

        public void InitSituation()
        {
            Student[] students = classManager.GetStudents();
            bool problematicFound = false;
            int i = 0;
            while (!problematicFound && i < students.Length)
            {
                problematicFound = students[i].IsProblematicStudent();
                i++;
            }

            problematicStudent = students[i - 1];
            if (sceneInfo.problematicsAnimation != null)
            {
                problematicStudent.PlayAnimation(sceneInfo.problematicsAnimation.name);
            }
            problematicStudent.GetComponent<AudioSource>().clip = problematicStudent.GetGender() == Gender.Men
                ? sceneInfo.situationAudioMasculine : sceneInfo.situationAudioFeminine;
            problematicStudent.GetComponent<AudioSource>().Play();
            Invoke("Laugh", 2f);

            behaviorTree.GetVariable("AccionAlumno").SetValue(true);
            initialDistance = Vector3.Distance(problematicStudent.transform.position, player.transform.position);
        }

        private void Laugh()
        {
            if (sceneInfo.classReactionAudio != null)
            {
                classManager.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
                classManager.GetComponent<AudioSource>().Play();
            }
        }

        public void Finish()
        {
            classManager.GetComponent<AudioSource>().clip = sceneInfo.afterClassBell;
            classManager.GetComponent<AudioSource>().Play();
        }
    }

}

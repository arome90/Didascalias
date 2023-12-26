//using BehaviorDesigner.Runtime;
//using System.Collections;
//using UnityEngine;

//namespace ClassRoomVR
//{
//    public class Scene3 : MonoBehaviour
//    {
//        private BehaviorTree behaviorTree;
//        private Student problematicStudent;
//        private ClassManager classManager;
//        private ScenePackage sceneInfo;
//        private GameObject player;
//        private float initialDistance;

//        [SerializeField] private AudioClip noiseClip;
//        [SerializeField] private AnimationClip classAnimation;
//        [SerializeField] private AnimationClip problematicAnimation;

//        private void Start()
//        {
//            behaviorTree = GetComponent<BehaviorTree>();
//            sceneInfo = GameManager.Instance.GetPackageAtIndex(0);
//            player = GameManager.Instance.GetPlayer();
//            classManager = ClassManager.Instance;
//            classManager.GetComponent<AudioSource>().clip = sceneInfo.beforeClassBell;
//            classManager.GetComponent<AudioSource>().Play();
//        }

//        public void InitializeSituation()
//        {
//            Student[] students = classManager.GetStudents();
//            int problematicCount = sceneInfo.problematicStudents;
//            int i = 0;
//            while (problematicCount != 0 && i < students.Length)
//            {
//                if (students[i].IsProblematicStudent())
//                {
//                    problematicCount--;
//                    problematicStudent = students[i];
//                    problematicStudent.MoveTo(classManager.GetStudentsController().FrontSide.position);
//                }
//                i++;
//            }

//            if (problematicStudent != null)
//            {
//                problematicStudent.GetComponent<AudioSource>().clip = problematicStudent.GetGender() == Gender.Men
//                    ? sceneInfo.situationAudioMasculine : sceneInfo.situationAudioFeminine;
//                problematicStudent.GetComponent<AudioSource>().Play();
//                initialDistance = Vector3.Distance(problematicStudent.transform.position, player.transform.position);
//            }

//            behaviorTree.GetVariable("AccionAlumno").SetValue(true);
//            //voice.ActivateWit();
//        }

//        public void Ignore(float time)
//        {
//            Invoke("IgnoreTime", time);
//            StartCoroutine(IgnoreStudent());
//        }

//        private void IgnoreTime()
//        {
//            if ((int)behaviorTree.GetVariable("Path").GetValue() < 0)
//            {
//                behaviorTree.GetVariable("Path").SetValue(3);
//                player.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
//                player.GetComponent<AudioSource>().Play();
//            }
//        }

//        private IEnumerator IgnoreStudent()
//        {
//            yield return new WaitUntil(() => !classManager.GetStudentsController().IsStudentInFieldOfVision(problematicStudent));
//            yield return new WaitForSecondsRealtime(3);
//            if (!classManager.GetStudentsController().IsStudentInFieldOfVision(problematicStudent))
//            {
//                behaviorTree.GetVariable("Path").SetValue(3);
//                player.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
//                player.GetComponent<AudioSource>().Play();
//            }
//            else if ((int)behaviorTree.GetVariable("Path").GetValue() < 0)
//            {
//                StartCoroutine(IgnoreStudent());
//            }
//        }

//        public void Near()
//        {
//            if (classManager.GetStudentsController().GetMode() == TalkMode.Good && Vector3.Distance(problematicStudent.transform.position, player.transform.position) <= initialDistance / 2)
//            {
//                behaviorTree.GetVariable("Path").SetValue(1);
//                Student[] students = classManager.GetStudents();
//                for (int i = 0; i < students.Length; i++)
//                {
//                    if (!students[i].IsProblematicStudent())
//                    {
//                        StartCoroutine(WaitAndExit(students[i], i));
//                    }
//                }
//            }
//        }

//        private IEnumerator WaitAndExit(Student student, float waitTime)
//        {
//            yield return new WaitForSeconds(waitTime);
//            student.MoveTo(classManager.GetStudentsController().Door.position);
//        }

//        public void Shout()
//        {
//            if (classManager.GetStudentsController().GetMode() == TalkMode.Disrespect)
//            {
//                behaviorTree.GetVariable("Path").SetValue(2);
//                classManager.GetComponent<AudioSource>().clip = noiseClip;
//                classManager.GetComponent<AudioSource>().volume = 0.1f;
//                classManager.GetComponent<AudioSource>().Play();
//            }
//        }

//        private void Laughter()
//        {
//            if (sceneInfo.classReactionAudio != null)
//            {
//                classManager.GetComponent<AudioSource>().clip = sceneInfo.classReactionAudio;
//                classManager.GetComponent<AudioSource>().Play();
//            }
//        }

//        public void Finish()
//        {
//            classManager.GetComponent<AudioSource>().clip = sceneInfo.afterClassBell;
//            classManager.GetComponent<AudioSource>().Play();
//        }
//    }

//}

//    #region 
//    //private GameObject agent;
//    //private MySceneManager sm;

//    //// Metodo de la situacion
//    //[Obsolete]
//    //public void walkAwayFromGroup()
//    //{
//    //    bool done = false;

//    //    if (sm == null) sm = GameManager.Instance._sceneManager;

//    //    if (agent == null)
//    //    {
//    //        GameObject[] problematics = sm.getProblematics();
//    //        agent = problematics[0];
//    //    }

//    //    // Hacer que el alumno se vaya a la parte delantera de la clase
//    //    NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
//    //    navMeshAgent.enabled = true;
//    //    Vector3 dest = new Vector3(0, 0, 0);

//    //    try
//    //    {
//    //        dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("FrontSide").position;
//    //    }
//    //    catch (Exception e)
//    //    {
//    //        Debug.Log("No se encontraron las posiciones de la clase en el prefab");
//    //    }

//    //    if (!agent.GetComponent<Animator>().GetBool("onFoot"))
//    //    {
//    //        agent.GetComponent<Animator>().SetBool("onFoot", true);
//    //    }
//    //    else
//    //    {
//    //        if (agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Walking")
//    //        {
//    //            navMeshAgent.SetDestination(dest);
//    //        }

//    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
//    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
//    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

//    //        if (x < 0.5 && y < 0.5 && z < 0.5)
//    //        {
//    //            agent.GetComponent<Animator>().Play("Standing");
//    //            agent.transform.Rotate(new Vector3(0, 180, 0));
//    //            done = true;
//    //        }

//    //        if (done)
//    //        {
//    //            navMeshAgent.enabled = false;
//    //            agent.transform.position = dest;
//    //            sm.setSpecialSituation(done);
//    //        }
//    //    }
//    //}

//    //    // Metodo path1
//    //    public void sitBack()
//    //    {
//    //        bool done = false;

//    //        // Hacer que el alumno vuelva a su sitio
//    //        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
//    //        navMeshAgent.enabled = true;
//    //        Vector3 dest = sm.getProblematicIniPos();

//    //        navMeshAgent.SetDestination(dest);
//    //        agent.GetComponent<Animator>().Play("Walking");

//    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
//    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
//    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

//    //        if (x < 0.5 && y < 0.5 && z < 0.5)
//    //        {
//    //            agent.GetComponent<Animator>().SetBool("onFoot", false);
//    //            done = true;
//    //        }

//    //        if (done)
//    //        {
//    //            navMeshAgent.enabled = false;
//    //            agent.transform.position = dest;
//    //            agent.transform.LookAt(sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("TeacherIni").position);
//    //            sm.setSpecialPath(done);
//    //        }
//    //    }

//    //    // Metodo path2
//    //    public void goEndOfclass()
//    //    {
//    //        bool done = false;

//    //        // Hacer que el alumno vaya al final de la clase
//    //        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
//    //        navMeshAgent.enabled = true;
//    //        Vector3 dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("BackCorner").position;

//    //        if (navMeshAgent.destination == null && dest.x != 0 || dest.y != 0 || dest.z != 0)
//    //        {
//    //            navMeshAgent.SetDestination(dest);
//    //            agent.GetComponent<Animator>().Play("Walking");
//    //        }

//    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
//    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
//    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

//    //        if (x < 0.5 && y < 0.5 && z < 0.5)
//    //        {
//    //            agent.GetComponent<Animator>().Play("Standing");
//    //            done = true;
//    //        }

//    //        if (done && agent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 2)
//    //        {
//    //            navMeshAgent.enabled = false;
//    //            agent.transform.position = dest;
//    //            sm.setSpecialPath(done);
//    //        }
//    //    }
//    #endregion



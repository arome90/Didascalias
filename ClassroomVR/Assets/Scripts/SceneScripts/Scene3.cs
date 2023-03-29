using BehaviorDesigner.Runtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR
{
    public class Scene3 : MonoBehaviour
    {


        BehaviorTree bh;
        Student problematic;
        ClassManager classManager;
        ScenePackage sceneInfo;
        GameObject player;
        VoiceActivation voice;
        float distanceInitial;

        [SerializeField] AudioClip ruido;
        [SerializeField] AnimationClip classAnimation;
        [SerializeField] AnimationClip probAnimation;
        void Start()
        {
            bh = GetComponent<BehaviorTree>();
            sceneInfo = GameManager.Instance.GetScenePackage(0);
            player = GameManager.Instance.GetPlayer();
            voice = GameManager.Instance.GetVoiceActivation();
            classManager = GameManager.Instance.GetClassManager();
            classManager.GetComponent<AudioSource>().clip = sceneInfo.before_bell;
            classManager.GetComponent<AudioSource>().Play();
        }
        public void InitSituation()
        {
            Student[] students = classManager.GetStudents();
            int pro = sceneInfo.problematicStudents;
            int i = 0;
            while (pro != 0 && i < students.Length)
            {
                if (students[i].GetProblematicStudent())
                {
                    pro--;
                    problematic = students[i];
                    problematic.MoveTo(classManager.GetStudentsController().FrontSide.position);

                }
                i++;
            }
            if(problematic){
                problematic.GetComponent<AudioSource>().clip = problematic.GetSex() == Student.Gender.Men
                   ? sceneInfo.audioSituationMasculino : sceneInfo.audioSituationFemenino;
                problematic.GetComponent<AudioSource>().Play();
                distanceInitial = Vector3.Distance(problematic.transform.position, player.transform.position);
            }
            bh.GetVariable("AccionAlumno").SetValue(true);
            //voice.ActivateWit();

        }

        public void Ignore(float time)
        {
            Invoke("IgnoreTime", time);
            StartCoroutine(IgnoreStudent());

        }
        void IgnoreTime()
        {
            if ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                bh.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
        }
        public IEnumerator IgnoreStudent()
        {
            yield return new WaitUntil(() => !classManager.GetStudentsController().IsStudentOnVision(problematic));
            yield return new WaitForSecondsRealtime(3);
            if (!classManager.GetStudentsController().IsStudentOnVision(problematic))
            {
                bh.GetVariable("Path").SetValue(3);
                player.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                player.GetComponent<AudioSource>().Play();
            }
            else if ((int)bh.GetVariable("Path").GetValue() < 0)
            {
                StartCoroutine(IgnoreStudent());
            }
        }




        public void Near()
        {
            if (classManager.GetStudentsController().GetMode() == StudentsController.TalkMode.Good && Vector3.Distance(problematic.transform.position, player.transform.position) <= distanceInitial / 2)
            {
                bh.GetVariable("Path").SetValue(1);
                Student[] students = classManager.GetStudents();
                for (int i = 0; i < students.Length; i++)
                {
                    if (!students[i].GetProblematicStudent()) StartCoroutine(WaitAndExit(students[i], i));
                }
            }
        }


        IEnumerator WaitAndExit(Student st, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            st.MoveTo(classManager.GetStudentsController().Door.position);
        }

        public void Shout()
        {
            if (classManager.GetStudentsController().GetMode() == StudentsController.TalkMode.Disrespect)
            {
                bh.GetVariable("Path").SetValue(2);
                classManager.GetComponent<AudioSource>().clip = ruido;
                classManager.GetComponent<AudioSource>().volume = 0.1f;
                classManager.GetComponent<AudioSource>().Play();
            }
        }




        void Risas()
        {
            if (sceneInfo.audioReaccionClase != null)
            {
                classManager.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                classManager.GetComponent<AudioSource>().Play();
            }
        }

        public void Termina()
        {
            classManager.GetComponent<AudioSource>().clip = sceneInfo.after_bell;
            classManager.GetComponent<AudioSource>().Play();
        }
    }

    #region 
    //private GameObject agent;
    //private MySceneManager sm;

    //// Metodo de la situacion
    //[Obsolete]
    //public void walkAwayFromGroup()
    //{
    //    bool done = false;

    //    if (sm == null) sm = GameManager.Instance._sceneManager;

    //    if (agent == null)
    //    {
    //        GameObject[] problematics = sm.getProblematics();
    //        agent = problematics[0];
    //    }

    //    // Hacer que el alumno se vaya a la parte delantera de la clase
    //    NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
    //    navMeshAgent.enabled = true;
    //    Vector3 dest = new Vector3(0, 0, 0);

    //    try
    //    {
    //        dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("FrontSide").position;
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.Log("No se encontraron las posiciones de la clase en el prefab");
    //    }

    //    if (!agent.GetComponent<Animator>().GetBool("onFoot"))
    //    {
    //        agent.GetComponent<Animator>().SetBool("onFoot", true);
    //    }
    //    else
    //    {
    //        if (agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Walking")
    //        {
    //            navMeshAgent.SetDestination(dest);
    //        }

    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

    //        if (x < 0.5 && y < 0.5 && z < 0.5)
    //        {
    //            agent.GetComponent<Animator>().Play("Standing");
    //            agent.transform.Rotate(new Vector3(0, 180, 0));
    //            done = true;
    //        }

    //        if (done)
    //        {
    //            navMeshAgent.enabled = false;
    //            agent.transform.position = dest;
    //            sm.setSpecialSituation(done);
    //        }
    //    }
    //}

    //    // Metodo path1
    //    public void sitBack()
    //    {
    //        bool done = false;

    //        // Hacer que el alumno vuelva a su sitio
    //        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
    //        navMeshAgent.enabled = true;
    //        Vector3 dest = sm.getProblematicIniPos();

    //        navMeshAgent.SetDestination(dest);
    //        agent.GetComponent<Animator>().Play("Walking");

    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

    //        if (x < 0.5 && y < 0.5 && z < 0.5)
    //        {
    //            agent.GetComponent<Animator>().SetBool("onFoot", false);
    //            done = true;
    //        }

    //        if (done)
    //        {
    //            navMeshAgent.enabled = false;
    //            agent.transform.position = dest;
    //            agent.transform.LookAt(sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("TeacherIni").position);
    //            sm.setSpecialPath(done);
    //        }
    //    }

    //    // Metodo path2
    //    public void goEndOfclass()
    //    {
    //        bool done = false;

    //        // Hacer que el alumno vaya al final de la clase
    //        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
    //        navMeshAgent.enabled = true;
    //        Vector3 dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("BackCorner").position;

    //        if (navMeshAgent.destination == null && dest.x != 0 || dest.y != 0 || dest.z != 0)
    //        {
    //            navMeshAgent.SetDestination(dest);
    //            agent.GetComponent<Animator>().Play("Walking");
    //        }

    //        float x = Mathf.Abs(agent.transform.position.x - dest.x);
    //        float y = Mathf.Abs(agent.transform.position.y - dest.y);
    //        float z = Mathf.Abs(agent.transform.position.z - dest.z);

    //        if (x < 0.5 && y < 0.5 && z < 0.5)
    //        {
    //            agent.GetComponent<Animator>().Play("Standing");
    //            done = true;
    //        }

    //        if (done && agent.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 2)
    //        {
    //            navMeshAgent.enabled = false;
    //            agent.transform.position = dest;
    //            sm.setSpecialPath(done);
    //        }
    //    }
    #endregion
}


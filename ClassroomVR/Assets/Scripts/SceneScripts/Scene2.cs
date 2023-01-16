using BehaviorDesigner.Runtime;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR
{
    public class Scene2 : MonoBehaviour
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
            sceneInfo = GameManager.Instance._packeges[0];
            player = GameManager.Instance.GetPlayer();
            voice = GameManager.Instance.voice;
            classManager = GameManager.Instance.GetClassManager();
            classManager.GetComponent<AudioSource>().clip = sceneInfo.before_bell;
            classManager.GetComponent<AudioSource>().Play();
        }
        public void InitSituation()
        {
            Student[] students = classManager.GetStudents();
            int pro = sceneInfo.problematicStudents;
            int i = 0;
            while (pro!=0 && i < students.Length)
            {
                if (students[i].GetProblematicStudent()) 
                {
                    pro--;
                    problematic = students[i];
                    problematic.PlayAnimation(probAnimation.name);
                   
                }
                i++;
            }
           
            bh.GetVariable("AccionAlumno").SetValue(true);
            distanceInitial = Vector3.Distance(problematic.transform.position, player.transform.position);
            voice.ActivateWit();

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
            yield return new WaitUntil(() => !classManager.IsStudentOnVision(problematic));
            yield return new WaitForSecondsRealtime(3);
            if (!classManager.IsStudentOnVision(problematic))
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
            if (classManager.GetMode() == ClassManager.TalkMode.Good && Vector3.Distance(problematic.transform.position, player.transform.position) <= distanceInitial / 2)
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
            st.MoveTo(classManager.door.position);
        }

        public void Shout()
        {
            if (classManager.GetMode() == ClassManager.TalkMode.Disrespect)
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
}

    //    private Vector3 dest = new Vector3(0, 0, 0);

    //    // Metodo de la situacion, path 4
    //    public void separateProblematics()
    //    {
    //        MySceneManager sm = GameManager.Instance._sceneManager;
    //        GameObject[] problematics = sm.getProblematics();
    //        GameObject schoolClass = sm.getClass();
    //        bool[] asientosOcupados = sm.getFreeDesks();

    //        bool done = false;

    //        // Hacer que uno de los alumnos se separe
    //        GameObject agent = problematics[0];
    //        NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
    //        navMeshAgent.enabled = true;

    //        for (int i = 0; i < asientosOcupados.Length; i++)
    //        {
    //            if (!asientosOcupados[i])
    //            {
    //                dest = schoolClass.GetComponentInChildren<Transform>().Find("Desks").Find("DeskPositions").GetChild(i).position;
    //                break;
    //            }
    //        }

    //        if (!agent.GetComponent<Animator>().GetBool("onFoot"))
    //        {
    //            agent.GetComponent<Animator>().SetBool("onFoot", true);
    //        }
    //        else
    //        {
    //            if (agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Walking")
    //            {
    //                navMeshAgent.SetDestination(dest);
    //            }

    //            float x = Mathf.Abs(agent.transform.position.x - dest.x);
    //            float y = Mathf.Abs(agent.transform.position.y - dest.y);
    //            float z = Mathf.Abs(agent.transform.position.z - dest.z);

    //            if (x < 0.5 && y < 0.5 && z < 0.5)
    //            {
    //                agent.GetComponent<Animator>().SetBool("onFoot", false);
    //                done = true;
    //            }

    //            if (done && agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sitting")
    //            {
    //                navMeshAgent.enabled = false;
    //                agent.transform.position = dest;
    //                agent.transform.position = new Vector3(agent.transform.position.x, agent.transform.position.y - 0.5f, agent.transform.position.z);
    //                agent.transform.LookAt(sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("TeacherIni").position);
    //                sm.setSpecialPath(done);
    //            }
    //        } //end else
    //    } // end SeparateProblematics
    //}
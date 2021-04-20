using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR {
    public class Scene2 : MonoBehaviour
    {
        private Vector3 dest = new Vector3(0, 0, 0);

        // Metodo de la situacion, path 4
        public void separateProblematics()
        {
            MySceneManager sm = GameManager.Instance._sceneManager;
            GameObject[] problematics = sm.getProblematics();
            GameObject schoolClass = sm.getClass();
            bool[] asientosOcupados = sm.getFreeDesks();

            bool done = false;

            // Hacer que uno de los alumnos se separe
            GameObject agent = problematics[0];
            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;

            for (int i = 0; i < asientosOcupados.Length; i++)
            {
                if (!asientosOcupados[i])
                {
                    dest = schoolClass.GetComponentInChildren<Transform>().Find("Desks").Find("DeskPositions").GetChild(i).position;
                    break;
                }
            }

            if (!agent.GetComponent<Animator>().GetBool("onFoot"))
            {
                agent.GetComponent<Animator>().SetBool("onFoot", true);
            }
            else
            {
                if (agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Walking")
                {
                    navMeshAgent.SetDestination(dest);
                }

                float x = Mathf.Abs(agent.transform.position.x - dest.x);
                float y = Mathf.Abs(agent.transform.position.y - dest.y);
                float z = Mathf.Abs(agent.transform.position.z - dest.z);

                if (x < 0.5 && y < 0.5 && z < 0.5)
                {
                    agent.GetComponent<Animator>().SetBool("onFoot", false);
                    done = true;
                }

                if (done && agent.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name == "Sitting")
                {
                    navMeshAgent.enabled = false;
                    agent.transform.position = dest;
                    agent.transform.position = new Vector3(agent.transform.position.x, agent.transform.position.y - 0.5f, agent.transform.position.z);
                    agent.transform.LookAt(sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("TeacherIni").position);
                    sm.setSpecialPath(done);
                }
            } //end else
        } // end SeparateProblematics
    }
}

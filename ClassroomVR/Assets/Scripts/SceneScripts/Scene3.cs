using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR {
    public class Scene3 : MonoBehaviour {

        private Vector3 iniPos = new Vector3(0, 0, 0);
        private GameObject agent;
        private MySceneManager sm;

        // Metodo de la situacion
        public void walkAwayFromGroup()
        {
            bool done = false;

            if(sm == null) sm = GameManager.Instance._sceneManager;

            if (agent == null) {
                GameObject[] problematics = sm.getProblematics();
                agent = problematics[0];
            }

            if (iniPos.x == 0 && iniPos.y == 0 && iniPos.z == 0)
            {
                iniPos = agent.transform.position;
            }

            // Hacer que el alumno se vaya a la parte delantera de la clase
            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;
            Vector3 dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("FrontSide").position;

            if (navMeshAgent.destination == null && dest.x != 0 || dest.y != 0 || dest.z != 0)
            {
                navMeshAgent.SetDestination(dest);
                agent.GetComponent<Animator>().Play("Walking");
            }

            float x = Mathf.Abs(agent.transform.position.x - dest.x);
            float y = Mathf.Abs(agent.transform.position.y - dest.y);
            float z = Mathf.Abs(agent.transform.position.z - dest.z);

            if (x < 0.5 && y < 0.5 && z < 0.5)
            {
                agent.GetComponent<Animator>().Play("Standing");
                //agent.transform.rotation.SetLookRotation(sm.getTeacher().transform.position);
                done = true;
            }

            if (done)
            {
                navMeshAgent.enabled = false;
                agent.transform.position = dest;
                sm.setSpecialSituation(done);
            }
        }

        // Metodo path1
        public void sitBack() {
            bool done = false;

            // Hacer que el alumno vuelva a su sitio
            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;
            Vector3 dest = iniPos;

            if (navMeshAgent.destination == null && dest.x != 0 || dest.y != 0 || dest.z != 0)
            {
                navMeshAgent.SetDestination(dest);
                agent.GetComponent<Animator>().Play("Walking");
            }

            float x = Mathf.Abs(agent.transform.position.x - dest.x);
            float y = Mathf.Abs(agent.transform.position.y - dest.y);
            float z = Mathf.Abs(agent.transform.position.z - dest.z);

            if (x < 0.5 && y < 0.5 && z < 0.5)
            {
                agent.GetComponent<Animator>().Play("Sitting");
                //agent.transform.rotation.SetLookRotation(sm.getTeacher().transform.position);
                done = true;
            }

            if (done)
            {
                navMeshAgent.enabled = false;
                agent.transform.position = dest;
                sm.setSpecialPath(done);
            }
        }

        // Metodo path2
        public void goEndOfclass() {
            bool done = false;

            // Hacer que el alumno vaya al final de la clase
            NavMeshAgent navMeshAgent = agent.GetComponent<NavMeshAgent>();
            navMeshAgent.enabled = true;
            Vector3 dest = sm.getClass().GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("BackCorner").position;

            if (navMeshAgent.destination == null && dest.x != 0 || dest.y != 0 || dest.z != 0)
            {
                navMeshAgent.SetDestination(dest);
                agent.GetComponent<Animator>().Play("Walking");
            }

            float x = Mathf.Abs(agent.transform.position.x - dest.x);
            float y = Mathf.Abs(agent.transform.position.y - dest.y);
            float z = Mathf.Abs(agent.transform.position.z - dest.z);

            if (x < 0.5 && y < 0.5 && z < 0.5)
            {
                agent.GetComponent<Animator>().Play("Standing");
                done = true;
            }


            if (done)
            {
                navMeshAgent.enabled = false;
                agent.transform.position = dest;
                sm.setSpecialPath(done);
            }
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR
{
    public class Scene3 : MonoBehaviour
    {




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
}

//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.AI;
//using UnityEngine.Events;

//public class WalkingScript : MonoBehaviour
//{
//    public UnityEvent studentSatEvent;

//    public Camera cam;
//	public Animator anim;
//	public NavMeshAgent agent;

//    private Vector3 chairPosition;

//    bool moving = false;
//    bool position = false;
//    bool moveWhenStandUpFinishes = false;

//    Quaternion quat;
//    float timeCount = 0.0f;

//    float rotationSpeed = 0.0f;
//    float positionSpeed = 0.0f;

//    TextMeshPro tmp;


//    void Start() {
//		anim = GetComponent<Animator>();
//        agent = GetComponent<NavMeshAgent>();

//        tmp = GetComponentInChildren<TextMeshPro>();
//    }
	
//    void Update()
//    {
//        if (!anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.STAND_UP_ANIM) && !anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.SITTING_IDLE_ANIM))
//        {
//            if (moveWhenStandUpFinishes)
//            {
//                moveWhenStandUpFinishes = false;
//                StartMovingToChair2();
//            }
//        }

//        if (moving)
//        {
//            if (anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.STAND_UP_ANIM) || anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.SIT_DOWN_ANIM) || anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.SITTING_IDLE_ANIM))
//            {
//                agent.isStopped = true;
//            }
//            else
//            {
//                agent.isStopped = false;
//            }

//            if (agent.remainingDistance <= agent.stoppingDistance)
//            {
//                anim.SetBool(Constants.STANDING_IDLE_PARAMETER, true);
//                anim.SetBool(Constants.WALKING_PARAMETER, false);
//                Debug.Log("StandingIdle a true en update");
//            }
//            else
//            {
//                anim.SetBool(Constants.STANDING_IDLE_PARAMETER, false);
//                anim.SetBool(Constants.WALKING_PARAMETER, true);
//            }
//        }

//        if(position)
//        {
//            Debug.Log("Me muevo");
//            // Change position
//            transform.position = Vector3.MoveTowards(transform.position, chairPosition, Time.deltaTime * positionSpeed);

//            // Change rotation
//            Quaternion q = new Quaternion(0f, 0f, 0f, 1f);
//            transform.rotation = Quaternion.Slerp(quat, q, timeCount * rotationSpeed);
//            timeCount = timeCount + Time.deltaTime;
            
//            // Stop when it finishes
//            if (transform.position.Equals(chairPosition))
//            {
//                position = false;
//                Debug.Log("Me he posicionado en la silla");
//                studentSatEvent.Invoke();
//            }
//        }
//    }

//    private void StartMovingToChair2()
//    {
//        chairPosition = new Vector3(-4.287f, 0.941f, -10.950f);
//        MovingConfiguration();
//    }

//    public void StartMovingToChair3Back()
//    {
//        chairPosition = new Vector3(-2.838f, 0.9524961f, -18.02f);
//        MovingConfiguration();
//    }

//    private void MovingConfiguration()
//    {
//        agent.enabled = true;
//        moving = true;
//        anim.SetBool(Constants.WALKING_PARAMETER, true);
//        anim.SetBool(Constants.STANDING_IDLE_PARAMETER, false);
//        anim.SetBool(Constants.SITTING_IDLE_PARAMETER, false);
//        Debug.Log("Walking a true");
//        agent.SetDestination(chairPosition);
//        agent.isStopped = true;
//    }

//    // Método al que se llama cuando el agente atraviesa un trigger de una silla
//    public void SitOnChair2()
//    {
//        SitConfiguration();

//        chairPosition = new Vector3(-4.287f, 0.941f, -11.232f);
//        rotationSpeed = 1.3f;
//        positionSpeed = 0.8f;
//    }

//    public void SitOnChair3Back()
//    {
//        SitConfiguration();

//        chairPosition = new Vector3(-2.838f, 0.9524961f, -17.42f);
//        rotationSpeed = 1.3f;
//        positionSpeed = 0.5f;
//    }

//    public void SitOnChair3Front()
//    {
//        SitConfiguration();

//        chairPosition = new Vector3(-2.838f, 0.9524961f, -11.2722f);
//        rotationSpeed = 1.3f;
//        positionSpeed = 0.5f;
//    }

//    public void StandUpInCorridorThirdScenario()
//    {
//        chairPosition = new Vector3(-2.449f, 0.9553962f, -11.175f);
//        StandUpConfiguration();
        
//        rotationSpeed = 1.3f;
//        positionSpeed = 0.5f;
//    }

//    public void StandUpInCorridorSecondScenario()
//    {
//        chairPosition = new Vector3(-0.9000001f, 0.9553962f, -12.5f);
//        StandUpConfiguration();

//        rotationSpeed = 1.3f;
//        positionSpeed = 0.5f;

//        moveWhenStandUpFinishes = true;
//    }

//    private void SitConfiguration()
//    {
//        if (agent.enabled)
//        {
//            agent.isStopped = true;
//        }
//        moving = false;
//        position = true;
//        quat = transform.rotation;
//        anim.SetBool(Constants.STANDING_IDLE_PARAMETER, false);
//        anim.SetBool(Constants.WALKING_PARAMETER, false);
//        anim.SetBool(Constants.SITTING_IDLE_PARAMETER, true);
//        agent.enabled = false;
//        timeCount = 0;
//    }

//    private void StandUpConfiguration()
//    {
//        Debug.Log("Me levanto");
//        agent.enabled = false;
//        moving = false;
//        position = true;
//        quat = transform.rotation;
//        anim.SetBool(Constants.STANDING_IDLE_PARAMETER, true);
//        anim.SetBool(Constants.WALKING_PARAMETER, false);
//        anim.SetBool(Constants.SITTING_IDLE_PARAMETER, false);
//        timeCount = 0;
//    }
//}

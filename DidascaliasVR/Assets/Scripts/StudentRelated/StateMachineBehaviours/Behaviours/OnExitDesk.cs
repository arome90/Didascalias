using UnityEngine;
using UnityEngine.AI;

public class OnExitDesk : StateMachineBehaviour
{
    Transform transform;
    Vector3 initialPos;
    Desk desk;

    NavMeshAgent agent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        transform = animator.transform.parent;
        initialPos = transform.position;
        desk = animator.GetComponentInParent<Student>().Desk;
        agent = animator.GetComponentInParent<NavMeshAgent>();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.enabled = true;
        
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnExitDesk: " + behaviour.name, this);
        behaviour.OnExitDesk.Invoke();

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        transform.position = Vector3.Lerp(
            initialPos, 
            desk.OutOfDeskTransform.position, 
            stateInfo.normalizedTime);
    }
}

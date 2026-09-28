using UnityEngine;
using UnityEngine.AI;

public class OnEnterDesk : StateMachineBehaviour
{
    Transform transform;
    Desk desk;
    NavMeshAgent agent;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        transform = animator.transform.parent;
        desk = animator.GetComponentInParent<Student>().Desk;
        agent = animator.GetComponentInParent<NavMeshAgent>();
        agent.enabled = false;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnEnterDesk: " + behaviour.name, this);
        behaviour.OnEnterDesk.Invoke();

        behaviour.PlaceMaterialOnDeskFromStanding();
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        transform.position = Vector3.Lerp(
            desk.OutOfDeskTransform.position,
            desk.StudentPosition.position,
            stateInfo.normalizedTime);
    }
}

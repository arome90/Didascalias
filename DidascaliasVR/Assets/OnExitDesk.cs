using UnityEngine;

public class OnExitDesk : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnExitDesk: " + behaviour.name, this);
        behaviour.OnExitDesk.Invoke();
    }
}

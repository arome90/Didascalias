using UnityEngine;

public class OnSitOnFloor : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnSitDownFloor: " + behaviour.name, this);
        behaviour.OnSitDownFloor.Invoke();
    }
}

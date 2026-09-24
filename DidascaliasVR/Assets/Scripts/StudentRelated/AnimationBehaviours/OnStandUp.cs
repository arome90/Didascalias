using UnityEngine;

// [System.Obsolete(
//     "This class is no longer used. The OnStandUp event is now called directly from the StudentBehaviour class,"
//     + "and the state transition is handled in the state machine of the Student prefab."
// )]
public class OnStandUp : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnStandUpChair: " + behaviour.name, this);
        behaviour.OnStandUpChair.Invoke();
    }
}

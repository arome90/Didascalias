using UnityEngine;

public class StopLookingToSide : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnStopLookingToSide: " + behaviour.name, this);
        behaviour.StopLookingToSide();
    }
}

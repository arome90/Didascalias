using UnityEngine;

public class OnStandUpFromFloor : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnStandUpFloor: " + behaviour.name, this);
        behaviour.OnStandUpFloor.Invoke();
    }
}

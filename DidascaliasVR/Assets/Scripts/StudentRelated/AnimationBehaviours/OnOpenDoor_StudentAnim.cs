using UnityEngine;

public class OnOpenDoor_StudentAnim : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnOpenDoor: " + behaviour.name, this);
        behaviour.OnOpenDoor.Invoke();
    }
}

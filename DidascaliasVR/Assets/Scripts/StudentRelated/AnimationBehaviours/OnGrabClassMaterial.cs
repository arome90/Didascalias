using UnityEngine;

public class OnGrabClassMaterial : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnGrabClassMaterial: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(true);
    }
}

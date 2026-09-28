using UnityEngine;

public class OnClassMaterialPlacedWhileStanding : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnMaterialPlaced_StudentAnim: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(false);
    }
}

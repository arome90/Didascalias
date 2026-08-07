using UnityEngine;

public class OnMaterialPlaced_StudentAnim : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnMaterialPlaced_StudentAnim: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(false);
    }
}

using UnityEngine;

public class OnPutMaterialOnDesk : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnPutMaterialOnDesk: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(false);
    }
}

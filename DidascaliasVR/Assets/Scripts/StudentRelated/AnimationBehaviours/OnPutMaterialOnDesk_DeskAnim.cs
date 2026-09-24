using UnityEngine;

public class OnPutMaterialOnDesk_DeskAnim : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.transform.parent.GetComponentInChildren<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnPutMaterialOnDesk: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(false);

        animator.SetBool("HasMaterialOut", false);
        animator.SetBool("HasFailedMaterial", false);
        animator.SetBool("HasMaterialOutUnsorted", true);
    }
}

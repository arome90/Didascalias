using UnityEngine;

public class OnSortMaterial : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.transform.parent.GetComponentInChildren<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnSortMaterial: " + behaviour.name, this);

        behaviour.SetIsCarryingMaterial(false);

        animator.SetBool("HasMaterialOut", true);
        animator.SetBool("HasFailedMaterial", false);
        animator.SetBool("HasMaterialOutUnsorted", false);
    }
}

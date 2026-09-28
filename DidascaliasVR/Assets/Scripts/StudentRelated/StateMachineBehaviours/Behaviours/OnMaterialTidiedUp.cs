using UnityEngine;

public class OnMaterialTidiedUp : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnMaterialTidiedUp: " + behaviour.name, this);

        behaviour.SetHasMaterialOut(true);
    }
}
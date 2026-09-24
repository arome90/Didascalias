using Didascalia.Student;
using UnityEngine;

public class TriggerIdleFromUnusedTurned : StateMachineBehaviour
{
    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (    !animator.GetBool(StudentAnimatorController.HashFromBooleanParameter(StudentAnimatorController.BooleanStudentParameter.IsLookingLeft)) 
            &&  !animator.GetBool(StudentAnimatorController.HashFromBooleanParameter(StudentAnimatorController.BooleanStudentParameter.IsLookingBack)) 
            &&  !animator.GetBool(StudentAnimatorController.HashFromBooleanParameter(StudentAnimatorController.BooleanStudentParameter.IsLookingRight))
            &&  animator.GetBool(StudentAnimatorController.HashFromBooleanParameter(StudentAnimatorController.BooleanStudentParameter.IsTurned)))
        {
            animator.SetTrigger("TriggerIdleFromUnusedTurned");
            animator.GetComponentInParent<StudentAnimatorController>().SetIsTurned(false);
        }
    }
}

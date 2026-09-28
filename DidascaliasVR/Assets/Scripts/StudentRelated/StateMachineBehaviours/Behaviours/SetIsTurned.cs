using Didascalia.Student;
using UnityEngine;

public class SetIsTurned : StateMachineBehaviour
{
    public bool _isTurnedOnEnter = false;

    bool _hasLeavedState = false;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(_hasLeavedState)
        {
            animator.GetComponentInParent<StudentAnimatorController>().SetIsTurned(_isTurnedOnEnter);
            _hasLeavedState = false;
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _hasLeavedState = true;
    }
}

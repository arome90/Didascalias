using UnityEngine;

public class OnCloseDoor_DoorAnim : StateMachineBehaviour
{
    // OnStateEnter is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Door door = animator.GetComponentInParent<Door>();
        door.SetOpen(false);
    }
}

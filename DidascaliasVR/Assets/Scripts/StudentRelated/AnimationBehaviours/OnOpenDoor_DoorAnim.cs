using UnityEngine;

public class OnOpenDoor_DoorAnim : StateMachineBehaviour
{
    // OnStateEnter is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Door door = animator.GetComponentInParent<Door>();
        door.SetOpen(true);
    }
}

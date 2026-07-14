using UnityEngine;

public class OnCloseDoor : StateMachineBehaviour
{
    public enum DoorHandling { Inside, Outside }

    [SerializeField]
    private DoorHandling _handling;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnCloseDoor: " + behaviour.name, this);
        if (_handling == DoorHandling.Inside)
        {
            behaviour.OnCloseDoor.Invoke();
        }
        else
        {
            behaviour.OnExpel.Invoke();
        }
    }
}

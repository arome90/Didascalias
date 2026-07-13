using UnityEngine;

public class OnSitDown : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        StudentBehaviour behaviour = animator.GetComponentInParent<StudentBehaviour>();
        Didascalia.Utils.Log.Info("OnSitDown: " + behaviour.name, this);
        behaviour.OnSitDown.Invoke();
    }

}

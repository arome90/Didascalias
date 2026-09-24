using UnityEngine;

public class LookAnnoyedToSide : StateMachineBehaviour
{
    StudentBehaviour behaviour = null;

    float _elapsedTime = 0.0f;
    float _minTime = 3.0f;
    float _maxTime = 8.0f;
    float _goalTime = 0.0f;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        behaviour = animator.GetComponentInParent<StudentBehaviour>();
        _elapsedTime = 0.0f;
        _goalTime = Random.Range(_minTime, _maxTime);    
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime > _goalTime)
        {
            _elapsedTime = 0.0f;
            behaviour.LookAtTarget();
        }
    }
}

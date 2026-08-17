using UnityEngine;

public class ChangeBetweenLaughs : StateMachineBehaviour
{
    public string _laughTriggerName = "TriggerBiggerLaugh";

    int _loops = 0;
    int _currentLoops = 0;
    bool _hasCheckedLoop = false;

    private void PickRandomLoopTimes() { _loops = Random.Range(2, 5); _currentLoops = 0; }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PickRandomLoopTimes();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 0.9f && !_hasCheckedLoop)
        {
            _hasCheckedLoop = true;
            _currentLoops++;
            if (_currentLoops == _loops) { animator.SetTrigger(_laughTriggerName); }
        }
        else if (_hasCheckedLoop && stateInfo.normalizedTime < 0.1f) _hasCheckedLoop = false;
    }
}

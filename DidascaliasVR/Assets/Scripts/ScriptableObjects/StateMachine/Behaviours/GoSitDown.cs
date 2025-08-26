using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "GoSitDown", menuName = "StateMachine/Transitions/GoSitDown")]
public class GoSitDown : Transition
{
    StudentBehaviour _stBehaviour = null;

    bool _hasToMove = false;

    public override bool Check()
    {
        return _hasToMove;
    }

    private void OnSitDownRequested()
    {
        _hasToMove = true;
    }

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _stBehaviour = _machine.GetComponent<StudentBehaviour>();
        _stBehaviour.OnSitDownRequested.AddListener(OnSitDownRequested);
    }

    public override void OnCheck()
    {
        base.OnCheck();

        _stBehaviour.OnSitDownRequested.RemoveListener(OnSitDownRequested);

        _stBehaviour.MoveTo(_stBehaviour.SitSpot);
    }
}

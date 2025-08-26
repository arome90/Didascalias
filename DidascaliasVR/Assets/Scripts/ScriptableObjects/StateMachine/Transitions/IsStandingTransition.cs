using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "IsStanding", menuName = "StateMachine/Transitions/IsStanding")]
public class IsStandingTransition : Transition
{
    StudentBehaviour _behaviour = null;

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = _machine.GetComponent<StudentBehaviour>();
    }
    public override bool Check()
    {
        return _behaviour.State == StudentState.Standing;
    }
}

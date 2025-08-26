using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "ShouldSitDown", menuName = "StateMachine/Transitions/ShouldSitDown")]
public class ShouldSitDown : Transition
{
    StudentBehaviour _behaviour = null;

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = machine.GetComponent<StudentBehaviour>();
    }
    public override bool Check()
    {
        return _behaviour.State == StudentState.Sitting;
    }

    public override void OnCheck()
    {
        _behaviour.StartSitDownAnimation();
    }
}

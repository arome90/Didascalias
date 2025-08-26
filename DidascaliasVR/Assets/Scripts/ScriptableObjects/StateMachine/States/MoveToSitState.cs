using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName ="MoveToSit", menuName ="StateMachine/States/MoveToSit")]
public class MoveToSitState : Didascalia.StateMachine.State
{
    StudentBehaviour _behaviour;

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = _machine.GetComponent<StudentBehaviour>();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        _behaviour.MoveTo(_behaviour.SitSpot);
    }
}

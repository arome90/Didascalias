using Didascalia.StateMachine;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName ="DestinationReached", menuName ="StateMachine/Transitions/DestinationReached")]
public class DestinationReached : Didascalia.StateMachine.Transition
{
    [SerializeField]
    StudentState _newStateOnTransitionChecked;

    NavMeshAgent _agent = null;

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _agent = _machine.GetComponent<NavMeshAgent>();
    }
    public override bool Check()
    {
        return (!_agent.pathPending) && _agent.remainingDistance <= _agent.stoppingDistance;
    }

    public override void OnCheck()
    {
        base.OnCheck();

        _machine.GetComponent<StudentBehaviour>().ChangeState(_newStateOnTransitionChecked);
    }
}

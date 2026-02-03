using UnityEngine;
using UnityEngine.AI;

namespace Didascalia.StateMachine {

    [CreateAssetMenu(fileName = "DestinationReached", menuName = "StateMachine/Transitions/DestinationReached")]
    public class ChangeStateOnDestinationReachedTransition : Transition
    {
        [SerializeField]
        protected StudentState _newStateOnTransitionChecked;

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
}


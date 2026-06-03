using UnityEngine;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(HyperestimulationRequested),
        menuName = MenuDirectoryNonNormative + nameof(HyperestimulationRequested)
    )]
    internal class HyperestimulationRequested : Transition
    {
        StudentBehaviour _behaviour;

        bool _hyperestimulationRequested = false;
        public bool HyperestimulationRequestedFlag
        {
            get { return _hyperestimulationRequested; }
            private set
            {
                _hyperestimulationRequested = value;
            }
        }
        private void OnHyperestimulationRequested()
        {
            HyperestimulationRequestedFlag = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            _behaviour.OnHyperstimulateRequested.AddListener(OnHyperestimulationRequested);
        }

        public override bool Check()
        {
            return HyperestimulationRequestedFlag;
        }

        public override void OnCheck()
        {
            // FIXME: PLACEHOLDER
            _behaviour.Animator.SetBooleanParameter(Student.StudentAnimatorController.HashIsStimulatedTEA);

            _behaviour.OnHyperstimulateRequested.RemoveListener(OnHyperestimulationRequested);
        }
    }
}
using UnityEngine;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(FrustrateRequested),
        menuName = MenuDirectoryNonNormative + nameof(FrustrateRequested)
    )]
    public class FrustrateRequested : Transition
    {
        StudentBehaviour _behaviour;

        bool _frustrate = false;
        public bool Frustrate
        {
            get { return _frustrate; }
            private set
            {
                _frustrate = value;
            }
        }
        private void OnFrustrateRequested()
        {
            Frustrate = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            _behaviour.OnFrustrateRequested.AddListener(OnFrustrateRequested);
        }

        public override bool Check()
        {
            return Frustrate;
        }

        public override void OnCheck()
        {
            // XXX: PLACEHOLDER
            _behaviour.Animator.SetBooleanParameter(Student.StudentAnimatorController.HashIsLostSightTEA);
            // FIXME: message logging about student entering the conflict albeit the animations for it do not work well yet
            Didascalia.Utils.Log.Info(
                "[Conflict] Frustrate conflict triggered. NOTE: animations for this conflict do not work well yet. This message is displayed instead.",
                _behaviour
            );

            _behaviour.OnFrustrateRequested.RemoveListener(OnFrustrateRequested);
        }
    }
}
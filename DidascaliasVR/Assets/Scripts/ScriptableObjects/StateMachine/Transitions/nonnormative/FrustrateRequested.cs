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

            _behaviour.OnFrustrateRequested.RemoveListener(OnFrustrateRequested);
        }
    }
}
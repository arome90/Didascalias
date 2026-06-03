using UnityEngine;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(FailToPayAttentionRequested),
        menuName = MenuDirectoryNonNormative + nameof(FailToPayAttentionRequested)
    )]
    public class FailToPayAttentionRequested : Transition
    {
        StudentBehaviour _behaviour;

        bool _failToPayAttentionRequested = false;
        public bool FailToPayAttentionRequestedFlag
        {
            get { return _failToPayAttentionRequested; }
            private set
            {
                _failToPayAttentionRequested = value;
            }
        }
        private void OnFailToPayAttentionRequested()
        {
            FailToPayAttentionRequestedFlag = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            _behaviour.OnFailToPayAttentionRequested.AddListener(OnFailToPayAttentionRequested);
        }

        public override bool Check()
        {
            return FailToPayAttentionRequestedFlag;
        }

        public override void OnCheck()
        {
            // XXX: PLACEHOLDER
            _behaviour.Animator.SetBooleanParameter(Student.StudentAnimatorController.HashIsTalkingFront);

            _behaviour.OnFailToPayAttentionRequested.RemoveListener(OnFailToPayAttentionRequested);
        }
    }
}
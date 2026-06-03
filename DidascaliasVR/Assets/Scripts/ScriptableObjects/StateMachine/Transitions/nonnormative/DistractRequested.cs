using UnityEngine;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(DistractRequested),
        menuName = MenuDirectoryNonNormative + nameof(DistractRequested)
    )]
    public class DistractRequested : Transition
    {
        StudentBehaviour _behaviour;

        bool _distractRequested = false;
        public bool DistractRequestedFlag
        {
            get { return _distractRequested; }
            private set
            {
                _distractRequested = value;
            }
        }
        private void OnDistractRequested()
        {
            DistractRequestedFlag = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            _behaviour.OnGetDistractedRequested.AddListener(OnDistractRequested);
        }

        public override bool Check()
        {
            return DistractRequestedFlag;
        }

        public override void OnCheck()
        {
            // XXX: PLACEHOLDER
            _behaviour.Animator.SetBooleanParameter(Student.StudentAnimatorController.HashIsDrawing);
            // FIXME: message logging about student entering the conflict albeit the animations for it do not work well yet
            Didascalia.Utils.Log.Info(
                "[Conflict] Distract conflict triggered. NOTE: animations for this conflict do not work well yet. This message is displayed instead.",
                _behaviour
            );

            _behaviour.OnGetDistractedRequested.RemoveListener(OnDistractRequested);
        }
    }
}
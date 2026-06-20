using Didascalia.Student;
using UnityEngine;
using UnityEngine.Events;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(ConflictAnimationTransition),
        menuName = MenuDirectoryNonNormative + nameof(ConflictAnimationTransition)
    )]
    internal class ConflictAnimationTransition : Transition
    {
        // DistractRequested =>             HashIsDrawing
        // FailToPayAttentionRequested =>   HashIsTalkingFront
        // FrustrateRequested =>            HashIsLostSightTEA
        // GetMaterialOutRequested =>       HashIsGetMaterialOut
        // HyperstimulationRequested =>     HashIsStimulatedTEA
        [SerializeField]
        private StudentAnimatorController.BooleanParameter _booleanParameter = StudentAnimatorController.BooleanParameter.None;
        [SerializeField]
        private StudentAnimatorController.TriggerParameter _triggerParameter = StudentAnimatorController.TriggerParameter.None;
        [SerializeField]
        private StudentManager.ConflictType _conflictType = 0;
        [SerializeField]
        private bool _unimplementedAnimation = false;
        StudentBehaviour _behaviour;

        bool _transitionRequested = false;
        public bool TransitionRequestedFlag
        {
            get { return _transitionRequested; }
            private set
            {
                _transitionRequested = value;
            }
        }
        private void OnTransitionRequested()
        {
            TransitionRequestedFlag = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            EventFromConflictType(_conflictType, _behaviour).AddListener(OnTransitionRequested);
        }

        public override bool Check()
        {
            return TransitionRequestedFlag;
        }

        public override void OnCheck()
        {
            // FIXME: PLACEHOLDER
            if (_booleanParameter != StudentAnimatorController.BooleanParameter.None)
            {
                _behaviour.Animator.SetBooleanParameter(StudentAnimatorController.HashFromBooleanParameter(_booleanParameter));
            }
            if (_triggerParameter != StudentAnimatorController.TriggerParameter.None)
            {
                _behaviour.Animator.SetTriggerParameter(StudentAnimatorController.HashFromTriggerParameter(_triggerParameter));
            }

            if (_unimplementedAnimation)
            {
                Didascalia.Utils.Log.Info(
                    $"[Conflict] {name}. NOTE: animations for this conflict do not work well yet. This message is displayed instead.",
                    _behaviour
                );
            }

            EventFromConflictType(_conflictType, _behaviour).RemoveListener(OnTransitionRequested);
        }

        public static UnityEvent EventFromConflictType(StudentManager.ConflictType conflictType, StudentBehaviour behaviour)
        {
            UnityEvent InvalidEvent()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"Invalid ConflictType: {conflictType}", behaviour);
                return null;
            }
            return conflictType switch
            {
                StudentManager.ConflictType.Disrespect =>       behaviour.OnYellRequested,
                StudentManager.ConflictType.SitTogether =>      behaviour.OnSitTogetherRequested,
                StudentManager.ConflictType.StandUp =>          behaviour.OnStandUpRequested,
                StudentManager.ConflictType.Hyperstimulation => behaviour.OnHyperstimulateRequested,
                StudentManager.ConflictType.Frustration =>      behaviour.OnFrustrateRequested,
                StudentManager.ConflictType.Disorganization =>  behaviour.OnGetMaterialOutRequested,
                StudentManager.ConflictType.Impulsivity =>      behaviour.OnFailToPayAttentionRequested,
                StudentManager.ConflictType.Inattention =>      behaviour.OnGetDistractedRequested,
                _ => InvalidEvent()
            };
        }
    }

    [System.Serializable]
    internal struct ConflictAnimationPool
    {
        [SerializeField]
        public Student.StudentAnimatorController.BooleanParameter[] BooleanAnimations;
        [SerializeField]
        public Student.StudentAnimatorController.TriggerParameter[] TriggerAnimations;
    }
}
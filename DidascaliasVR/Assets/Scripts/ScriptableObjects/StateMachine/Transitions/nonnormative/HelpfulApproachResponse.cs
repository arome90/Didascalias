//using UnityEngine;

//namespace Didascalia.StateMachine.NonNormative
//{
//    [CreateAssetMenu(
//        fileName = nameof(HelpfulApproachResponse),
//        menuName = MenuDirectoryNonNormative + nameof(HelpfulApproachResponse)
//    )]
//    internal class HelpfulApproachResponse : Transition
//    {
//        [SerializeField]
//        ConflictAnimationPool[] _resultAnimationPools;

//        private StudentBehaviour _behaviour;
//        bool _hasHelpfullyApproached = false;
//        public bool HasHelpfullyApproachedFlag
//        {
//            get { return _hasHelpfullyApproached; }
//            /*private */set
//            {
//                _hasHelpfullyApproached = value;
//            }
//        }
//        public override void Initialize(StateMachine machine)
//        {
//            base.Initialize(machine);

//            _behaviour = machine.GetComponent<StudentBehaviour>();
//        }

//        public override bool Check()
//        {
//            return HasHelpfullyApproachedFlag;
//        }

//        public override void OnCheck()
//        {
//            // FIXME: PLACEHOLDER
//            Didascalia.Utils.Log.Info($"HelpfulApproachResponse.OnCheck() called. HasHelpfullyApproachedFlag: {HasHelpfullyApproachedFlag}", this);
//            var pool = _resultAnimationPools[Random.Range(0, _resultAnimationPools.Length)];
//            var animation = Random.Range(0, pool.BooleanAnimations.Length + pool.TriggerAnimations.Length);
//            if (animation < pool.BooleanAnimations.Length)
//            {
//                var animationHash = Student.StudentAnimatorController.HashFromBooleanParameter(pool.BooleanAnimations[animation]);
//                _behaviour.Animator.SetStudentBooleanParameter(animationHash);
//            }
//            else
//            {
//                var animationHash = Student.StudentAnimatorController.HashFromTriggerParameter(pool.TriggerAnimations[animation - pool.BooleanAnimations.Length]);
//                _behaviour.Animator.SetStudentTriggerParameter(animationHash);
//            } 
//        }
//    }
//}
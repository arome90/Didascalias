//using UnityEngine;

//namespace Didascalia.StateMachine.NonNormative
//{
//    [CreateAssetMenu(
//        fileName = nameof(ForcefulInterventionResponse),
//        menuName = MenuDirectoryNonNormative + nameof(ForcefulInterventionResponse)
//    )]
//    internal class ForcefulInterventionResponse : Transition
//    {
//        [SerializeField]
//        ConflictAnimationPool[] _resultAnimationPools;

//        private StudentBehaviour _behaviour;
//        bool _hasForcefullyIntervened = false;
//        public bool HasForcefullyIntervenedFlag
//        {
//            get { return _hasForcefullyIntervened; }
//            /*private */set
//            {
//                _hasForcefullyIntervened = value;
//            }
//        }
//        public override void Initialize(StateMachine machine)
//        {
//            base.Initialize(machine);

//            _behaviour = machine.GetComponent<StudentBehaviour>();
//        }

//        public override bool Check()
//        {
//            return HasForcefullyIntervenedFlag;
//        }

//        public override void OnCheck()
//        {
//            // FIXME: PLACEHOLDER
//            Didascalia.Utils.Log.Info($"ForcefulInterventionResponse.OnCheck() called. HasForcefullyIntervenedFlag: {HasForcefullyIntervenedFlag}", this);
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
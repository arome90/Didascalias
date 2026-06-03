using UnityEngine;

namespace Didascalia.StateMachine.NonNormative
{
    [CreateAssetMenu(
        fileName = nameof(GetMaterialOutRequested),
        menuName = MenuDirectoryNonNormative + nameof(GetMaterialOutRequested)
    )]
    public class GetMaterialOutRequested : Transition
    {
        StudentBehaviour _behaviour;

        bool _getMaterialOutRequested = false;
        public bool GetMaterialOutRequestedFlag
        {
            get { return _getMaterialOutRequested; }
            private set
            {
                _getMaterialOutRequested = value;
            }
        }
        private void OnGetMaterialOutRequested()
        {
            GetMaterialOutRequestedFlag = true;
        }
        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _behaviour = machine.GetComponent<StudentBehaviour>();
            _behaviour.OnGetMaterialOutRequested.AddListener(OnGetMaterialOutRequested);
        }

        public override bool Check()
        {
            return GetMaterialOutRequestedFlag;
        }

        public override void OnCheck()
        {
            _behaviour.Animator.SetBooleanParameter(Student.StudentAnimatorController.HashIsGetMaterialOutWrong);

            _behaviour.OnGetMaterialOutRequested.RemoveListener(OnGetMaterialOutRequested);
        }
    }
}
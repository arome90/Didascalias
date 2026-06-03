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
            // FIXME: message logging about student entering the conflict albeit the animations for it do not work well yet
            Didascalia.Utils.Log.Info(
                "[Conflict] Get material out conflict triggered. NOTE: animations for this conflict do not work well yet. This message is displayed instead.",
                _behaviour
            );

            _behaviour.OnGetMaterialOutRequested.RemoveListener(OnGetMaterialOutRequested);
        }
    }
}
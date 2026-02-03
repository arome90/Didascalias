using UnityEngine;

namespace Didascalia.StateMachine
{
[CreateAssetMenu(fileName = "ShouldSitDown", menuName = "StateMachine/Transitions/SitTogether/OtherStudentStoodUp")]
    public class SitTogether_OtherStudentJustStoodUp : ChangeStateOnDestinationReachedTransition
    {
        StudentBehaviour _target;

        public override void Initialize(StateMachine machine)
        {
            base.Initialize(machine);

            _target = machine.GetData("sitTogether_nearStudent").GetComponent<StudentBehaviour>();
        }
        
        // CAMBIAR ESTO POR UN ONSTANDUP HIHI

        public override bool Check()
        {
            return _target.State == StudentState.Standing;
        }

        public override void OnCheck()
        {
            base.OnCheck();

            _target.ChangeSitSpotWithStudent(_machine.GetComponent<StudentBehaviour>());
        }
    }
}


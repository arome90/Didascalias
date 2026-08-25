//using UnityEngine;

//namespace Didascalia.StateMachine
//{
//    [CreateAssetMenu(fileName = "RequestTargetToChangePlaces", 
//        menuName = "StateMachine/Behaviours/RequestChangePlaces")]
//    public class RequestTargetStudentToChangePlaces : StateBehaviour
//    {
//        [SerializeField] string _targetStudentID;
//        StudentBehaviour _targetStudentBehaviour;
//        StudentBehaviour _myStudentBehaviour;

//        public override void Initialize(StateMachine machine)
//        {
//            base.Initialize(machine);

//            _myStudentBehaviour = _machine.GetComponent<StudentBehaviour>();

//            // cogemos el estudiante objetivo
//            _targetStudentBehaviour = machine.GetData(_targetStudentID).GetComponent<StudentBehaviour>();

//            _targetStudentBehaviour.ChangeState(StudentState.StandingOnDesk);
//            _targetStudentBehaviour.OnStandUpChair.AddListener(RequestSitDown);
//        }

//        private void RequestSitDown()
//        {
//            // this student sit down
//            _myStudentBehaviour.SitDownStudent();

//            // target student sit down
//            _targetStudentBehaviour.SitDownStudent();
//            _targetStudentBehaviour.OnStandUpChair.RemoveListener(RequestSitDown);
//        }

//        public override void Update() {}
//    }
//}

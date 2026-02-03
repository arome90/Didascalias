using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "SitTogetherRequested", menuName = "StateMachine/Transitions/SitTogetherRequested")]
public class SitTogetherRequested : Transition
{
    StudentBehaviour _behaviour;

    bool _sitTogether = false;
    private void OnSitTogetherRequested()
    {
        _sitTogether = true;
    }

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = machine.GetComponent<StudentBehaviour>();
        _behaviour.OnSitTogetherRequested.AddListener(OnSitTogetherRequested);
    }

    public override bool Check()
    {
        return _sitTogether;
    }

    public override void OnCheck()
    {
        // _behaviour.MoveTo(other.GetComponent<StudentBehaviour>().SitSpot);
        Student st = StudentManager.Instance.
            GetStudentFarFromOtherStudent(_behaviour.GetComponent<Student>());

        // st.SetAsConflictive();

        Student other = st.NextStudent == null ? st.PreviousStudent : st.NextStudent;

        _behaviour.MoveTo(other.transform);

        _behaviour.OnSitTogetherRequested.RemoveListener(OnSitTogetherRequested);

        // estudiante que vamos a quitar de su sitio
        _machine.AddData("sitTogether_nearStudent", other);

        // estudiante con el que nos queremos sentar (por si hiciera falta)
        _machine.AddData("sitTogether_farStudent", st);
    }
}

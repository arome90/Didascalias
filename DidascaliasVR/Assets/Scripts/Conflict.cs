using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conflict : MonoBehaviour
{
    [SerializeField]
    float _timeToResolve = 5.0f;

    private bool _success = false;

    private Student _conflictiveStudent = null;

    private List<Student> _affectedStudents = new List<Student>();

    private void Start()
    {
        _success = false;

        StartCoroutine(WaitToResolve());
    }

    public void SetConflictiveStudent(Student st)
    {
        _conflictiveStudent = st;
        _conflictiveStudent.SetAsConflictive();
    }

    public void AddAffectedStudent(Student affectedStudent)
    {
        if (_affectedStudents == null) _affectedStudents = new List<Student>();
        _affectedStudents.Add(affectedStudent);
    }

    public void AddAffectedStudents(List<Student> affectedStudents)
    {
        if (_affectedStudents == null) _affectedStudents = new List<Student>();
        _affectedStudents.AddRange(affectedStudents);
    }

    public void ReceivePositiveResolution()
    {
        _success = true;
        SuccessConflict();
    }

    private IEnumerator WaitToResolve()
    {
        yield return new WaitForSeconds(_timeToResolve);

        if (!_success) FailConflict();
    }

    private void SuccessConflict()
    {
        // Conflict disappears and class continues
        _conflictiveStudent.Deselect();

        SendStudentBackToTheirDesk(_conflictiveStudent);

        foreach (Student st in _affectedStudents) SendStudentBackToTheirDesk(st);

        Debug.Log("Conflict resolved!");

        StudentManager.Instance.RemoveConflict(_conflictiveStudent);
    }

    private void SendStudentBackToTheirDesk(Student st)
    {
        st.Behaviour.ChangeDesk(st.OriginalDesk, false);
        st.Behaviour.SitDown();
    }

    private void FailConflict()
    {
        // Engage on chaos
        Didascalia.Utils.Log.Warning("Conflict failed. TODO", this);
    }
}

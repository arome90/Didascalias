using System.Collections;
using UnityEngine;

public class Conflict : MonoBehaviour
{
    [SerializeField]
    float _timeToResolve = 5.0f;

    private bool _success = false;

    private Student _conflictiveStudent = null;

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

        Debug.Log("Conflict resolved!");

        StudentManager.Instance.RemoveConflict(_conflictiveStudent);
    }

    private void FailConflict()
    {
        // Engage on chaos
    }
}

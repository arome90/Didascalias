using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public struct ConflictSetupResult
{
    public ConflictGenerationError Error;
    public string errorWhy;
}

/// <summary>
/// Base class for any conflicts
/// </summary>
public abstract class Conflict : ScriptableObject
{
    protected List<PlayerAction> _positiveActions = null;

    protected List<PlayerAction> _neutralActions = null;
    
    protected List<PlayerAction> _negativeActions = null;

    protected StudentManager _manager = null;
    protected List<Student> _nonConflictiveStudents = null;

    protected StudentBehaviour _behaviour;

    protected ConflictType _type = ConflictType.UNKNOWN;

    protected bool _wasSetUp = false;

    protected PlayerResolutionToConflict _currentPlayerResolution = PlayerResolutionToConflict.None;

    /// <summary>
    /// The student that generated the conflict
    /// </summary>
    protected Student         _conflictiveStudent = null;

    public Student ConflictiveStudent => _conflictiveStudent;

    /// <summary>
    /// The students affected by the conflictive students actions
    /// May be none
    /// </summary>
    protected List<Student>   _affectedStudents = null;

    protected Conflict()
    {
        _manager = StudentManager.Instance;
        _nonConflictiveStudents = _manager.GetStudents();

        int i = 0;
        // doing this we make sure that every conflict has a list with all students that have NO active conflict
        while (i < _nonConflictiveStudents.Count)
        {
            if (_nonConflictiveStudents[i].ActiveConflict != null)      _nonConflictiveStudents.RemoveAt(i);
            else                                                        ++i;
        }

        _conflictiveStudent = null;

        RegisterActions();
    }

    public abstract void RegisterActions();

    public void ResolveAction(PlayerAction action)
    {
        if (_positiveActions.Contains(action)) _currentPlayerResolution =       PlayerResolutionToConflict.Positive;
        else if (_neutralActions.Contains(action)) _currentPlayerResolution =   PlayerResolutionToConflict.Neutral;
        else if (_negativeActions.Contains(action)) _currentPlayerResolution =  PlayerResolutionToConflict.Negative;
        else _currentPlayerResolution =                                         PlayerResolutionToConflict.None;
    }

    public void RegisterNewActions(ref List<PlayerAction> list, List<PlayerAction> actions)
    {
        if (list == null)   list = actions;
        else                list.AddRange(actions);
    }

    public void RegisterPositiveActions(List<PlayerAction> actions) => RegisterNewActions(ref _positiveActions, actions);
    public void RegisterNeutralActions(List<PlayerAction> actions) => RegisterNewActions(ref _neutralActions, actions);
    public void RegisterNegativeActions(List<PlayerAction> actions) => RegisterNewActions(ref _negativeActions, actions);

    // TODO: Make it possible so that from the web the user can select a student and check what conflicts are possible for that student
    // we have to change things a little bit to do that
    // public void SetConflictiveStudent(Student st) => _conflictiveStudent = st;

    /// <summary>
    /// Checks if the conditions for conflict generation are met
    /// </summary>
    /// <returns></returns>
    public abstract ConflictSetupResult IsConflictFeasible();

    /// <summary>
    /// This starts the conflict, calling the correct methods of the _conflictiveStudent
    /// </summary>
    public void StartConflict()
    {
        if (_type == ConflictType.UNKNOWN)
        {
            Debug.LogError($"Attempted to Start Conflict of type :'{_type}'. Check if you have changed the conflict type in the SetUpConflict method!!");
            return;
        }

        if (!_wasSetUp)
        {
            Debug.LogError($"Attempted to Start Conflict of type :'{_type}' when it is not feasible (IsConflictFeasible)!!. Conflict will not be generated");
            return;
        }

        _behaviour = _conflictiveStudent.Behaviour;
        _conflictiveStudent.SetConflict(this);
        _conflictiveStudent.RunConflict();
    }

    public abstract IEnumerator Run();

    protected void ResolveConflict()
    {
        _conflictiveStudent.StopConflict();
        _conflictiveStudent.SetConflict(null);

        StudentManager.Instance.RemoveActiveConflict(this);
    }

    protected bool HasNotActed() => _currentPlayerResolution == PlayerResolutionToConflict.None;
    protected bool IsPositive() =>  _currentPlayerResolution == PlayerResolutionToConflict.Positive;
    protected bool IsNeutral() =>   _currentPlayerResolution == PlayerResolutionToConflict.Neutral;
    protected bool IsNegative() =>  _currentPlayerResolution == PlayerResolutionToConflict.Negative;

    #region Player Actions
    protected IEnumerator WaitForPlayerAction()
    {
        ResetPlayerResolution();

        yield return new WaitUntil(() => _currentPlayerResolution != PlayerResolutionToConflict.None);
    }

    protected void ResetPlayerResolution()
    {
        _currentPlayerResolution = PlayerResolutionToConflict.None;

        // Player.StartListeningForPlayerResolution();
    }
    #endregion
}

//public class Conflict : MonoBehaviour
//{
//    [SerializeField]
//    float _timeToResolve = 5.0f;

//    private bool _success = false;

//    private Student _conflictiveStudent = null;

//    private List<Student> _affectedStudents = new List<Student>();

//    private void Start()
//    {
//        _success = false;

//        StartCoroutine(WaitToResolve());
//    }

//    public void SetConflictiveStudent(Student st)
//    {
//        _conflictiveStudent = st;
//        _conflictiveStudent.SetAsConflictive();
//    }

//    public void AddAffectedStudent(Student affectedStudent)
//    {
//        if (_affectedStudents == null) _affectedStudents = new List<Student>();
//        _affectedStudents.Add(affectedStudent);
//    }

//    public void AddAffectedStudents(List<Student> affectedStudents)
//    {
//        if (_affectedStudents == null) _affectedStudents = new List<Student>();
//        _affectedStudents.AddRange(affectedStudents);
//    }

//    public void ReceivePositiveResolution()
//    {
//        _success = true;
//        SuccessConflict();
//    }

//    private IEnumerator WaitToResolve()
//    {
//        yield return new WaitForSeconds(_timeToResolve);

//        if (!_success) FailConflict();
//    }

//    private void SuccessConflict()
//    {
//        // Conflict disappears and class continues
//        _conflictiveStudent.Deselect();

//        SendStudentBackToTheirDesk(_conflictiveStudent);

//        foreach (Student st in _affectedStudents) SendStudentBackToTheirDesk(st);

//        Debug.Log("Conflict resolved!");

//        StudentManager.Instance.RemoveConflict(_conflictiveStudent);
//    }

//    private void SendStudentBackToTheirDesk(Student st)
//    {
//        st.Behaviour.ChangeDesk(st.OriginalDesk, false);
//        st.Behaviour.SitDown();
//    }

//    private void FailConflict()
//    {
//        // Engage on chaos
//        Didascalia.Utils.Log.Warning("Conflict failed. TODO", this);
//    }
//}

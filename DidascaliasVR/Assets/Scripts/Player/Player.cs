using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum PlayerResolutionToConflict
{
    Positive,
    Neutral,
    Negative,
    None
}

public class Player : Singleton<Player>
{
    private List<Conflict> _conflicts = null;

    public void AddActiveConflict(Conflict conflict)
    {
        if (_conflicts == null) _conflicts = new List<Conflict>();

        _conflicts.Add(conflict);
    }

    public void RemoveActiveConflict(Conflict conflict)
    {
        if(_conflicts != null) _conflicts.Remove(conflict);
    }

    public void ProcessAction(PlayerAction action)
    {
        List<Student> selectedSt = StudentManager.Instance.GetSelectedStudents();
        if (selectedSt != null && selectedSt.Count > 0)
        {
            foreach (Student student in selectedSt)
                student.ActiveConflict.ResolveAction(action);
        }
        else ResolveConflicts(action);
    }

    private void ResolveConflicts(PlayerAction action)
    {
        foreach (Conflict conflict in _conflicts)
            conflict.ResolveAction(action);
    }

    //public static void StartListeningForPlayerResolution()
    //{
    //    Instance._hasResolved = false;
    //    Instance.StartCoroutine(Instance.ListeningForPlayerResolution());
    //}

    //IEnumerator ListeningForPlayerResolution()
    //{
    //    yield return new WaitUntil(() => _hasResolved);
    //    OnPlayerResolution.Invoke(_currentResolution);
    //}

    //public void PositiveResolution()
    //{
    //    _currentResolution = PlayerResolutionToConflict.Positive;
    //    _hasResolved = true;
    //}

    //public void NeutralResolution()
    //{
    //    _currentResolution = PlayerResolutionToConflict.Neutral;
    //    _hasResolved = true;
    //}

    //public void NegativeResolution()
    //{
    //    _currentResolution = PlayerResolutionToConflict.Negative;
    //    _hasResolved = true;
    //}

    //private void ReceiveResolution(PlayerResolutionToConflict resolution)
    //{
    //    _currentResolution = resolution;
    //    _hasResolved = _currentResolution != PlayerResolutionToConflict.None;
    //}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Player script = (Player)target;

        foreach (PlayerAction actionValue in System.Enum.GetValues(typeof(PlayerAction)))
        {
            if (GUILayout.Button(actionValue.ToString()))
            {
                script.ProcessAction(actionValue);
            }
        }

        DrawDefaultInspector();
    }
}
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TestingStudentBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform randomPoint = null;

    public string _queryLLMWithThis = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ClassManager.Instance.StartClass();
        onClassCreated.Invoke();
    }

    public void StandUp(Student student)
    {
        student.GetComponent<StudentBehaviour>().StandUp();
    }



    public void SitDown(Student student)
    {
        student.GetComponent<StudentBehaviour>().SitDown();
    }

    public void LeaveDesk(Student student)
    {
        student.GetComponent<StudentBehaviour>().LeaveDesk();
    }
        
    public void MoveToRandomPoint(Student student)
    {
        randomPoint.SetLocalPositionAndRotation(new Vector3(
            UnityEngine.Random.Range(-3.0f, 3.0f),
            0.0f,
            UnityEngine.Random.Range(-3.0f, 3.0f)), 
            Quaternion.identity);

        student.GetComponent<StudentBehaviour>().MoveTo(randomPoint, StudentBehaviour.MovementAction.Walk, true);
    }

    public void RunToRandomPoint(Student student)
    {
        randomPoint.SetLocalPositionAndRotation(new Vector3(
            UnityEngine.Random.Range(-3.0f, 3.0f),
            0.0f,
            UnityEngine.Random.Range(-3.0f, 3.0f)),
            Quaternion.identity);

        student.GetComponent<StudentBehaviour>().MoveTo(randomPoint, StudentBehaviour.MovementAction.Run, true);
    }

    public void AnxiousRunToRandomPoint(Student student)
    {
        randomPoint.SetLocalPositionAndRotation(new Vector3(
            UnityEngine.Random.Range(-3.0f, 3.0f),
            0.0f,
            UnityEngine.Random.Range(-3.0f, 3.0f)),
            Quaternion.identity);

        student.GetComponent<StudentBehaviour>().MoveTo(randomPoint, StudentBehaviour.MovementAction.RunAnxiety, true);
    }

    public void MoveToFrontDoor(Student student)
    {
        student.GetComponent<StudentBehaviour>().MoveToFrontDoor(true);
    }

    public void Expel(Student student)
    {
        student.GetComponent<StudentBehaviour>().Expel();
    }

    private void HandleConflict(ConflictType type)
    {
        ConflictGenerationResult result = StudentManager.Instance.GenerateConflict(type);
        if (result.Error != ConflictGenerationError.None)
        {
            Debug.LogError(result.errorWhy);
            return;
        }
        StudentManager.Instance.HandleConflict(result.ConflictInstance);
    }

    public void StandUpConflict()
    {
        HandleConflict(ConflictType.StandUp);
    }

    public void Hyperstimulate()
    {
        HandleConflict(ConflictType.Hyperstimulation);
    }

    public void GetDistracted()
    {
        HandleConflict(ConflictType.DistractionTEA);
    }

    public void SitTogether()
    {
        HandleConflict(ConflictType.SitTogether);
    }
    public void GetOutMaterialWrong()
    {
        HandleConflict(ConflictType.MaterialOutWrong);
    }

    public void DrawDistracted()
    {
        HandleConflict(ConflictType.DrawDistracted);
    }

    public void BotherRandomStudents()
    {
        HandleConflict(ConflictType.BotherStudents);
    }

    public void ChangeSits()
    {
        List<Student> sts = StudentManager.Instance.GetStudents();

        if (sts.Count < 2) { Debug.LogError("Can't test Change Sits. Minimum of 2 students is required"); }

        StudentManager.Instance.SelectStudent(sts[0].Name);
        StudentManager.Instance.SelectStudent(sts[1].Name);

        StudentManager.Instance.OnChangePlaces();

        StudentManager.Instance.DeselectStudents();
    }

    public void SetTEATrue(Student st)
    {
        st.Behaviour.SetAutism(true);
    }

    public void SetTEAFalse(Student st)
    {
        st.Behaviour.SetAutism(false);
    }

    public void GoToFloor(Student st)
    {
        st.Behaviour.GoToFloor();
    }

    public void MakeNearbyStudentsLaugh(Student st)
    {
        StudentManager.Instance.MakeNearbyStudentsLaugh(st);
    }

    public void MakeNearbyStudentsTalk(Student st)
    {
        StudentManager.Instance.MakeNearbyStudentsTalk(st);
    }

    public void TakeMaterialOut(Student st)
    {
        st.Behaviour.TriggerGetMaterialOut();
    }

    public void TakeMaterialOutAll()
    {
        StudentManager.Instance.GetMaterialOutAllStudents();
    }

    [HideInInspector]
    public UnityEvent onClassCreated = new UnityEvent();
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestingStudentBehaviour))]
public class TestingStudentBehaviourEditor : Editor
{
    private int _selectedIndex = 0;
    private List<Student> _students = new List<Student>();
    private string[] _objectNames = new string[0];

    public override void OnInspectorGUI()
    {
        // Referencia al script original
        TestingStudentBehaviour script = (TestingStudentBehaviour)target;

        script.onClassCreated.AddListener(RefreshStudentList);

        // Botón para refrescar la lista de GameObjects en tiempo real (Runtime / Editor)
        if (GUILayout.Button("Refrescar Lista de GameObjects"))
        {
            RefreshStudentList();
        }

        EditorGUILayout.Space(10);

        if (_students != null && _students.Count > 0)
        {
            // Asegurar que el índice no quede fuera de rango tras un refresco (hmmm coca cola)
            _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _students.Count - 1);

            // 1. Mostrar el Dropdown (Popup)
            _selectedIndex = EditorGUILayout.Popup("Student selected:", _selectedIndex, _objectNames);

            Student selectedSt = _students[_selectedIndex];

            // these are Movement testing
            EditorGUILayout.Space(5);
            if (GUILayout.Button("StandUp"))
            {
                script.StandUp(selectedSt);
            }
            if (GUILayout.Button("SitDown"))
            {
                script.SitDown(selectedSt);
            }
            if (GUILayout.Button("LeaveDesk"))
            {
                script.LeaveDesk(selectedSt);
            }
            if (GUILayout.Button("MoveToRandomPoint"))
            {
                script.MoveToRandomPoint(selectedSt);
            }
            if (GUILayout.Button("RunToRandomPoint"))
            {
                script.RunToRandomPoint(selectedSt);
            }
            if (GUILayout.Button("AnxiousRunToRandomPoint"))
            {
                script.AnxiousRunToRandomPoint(selectedSt);
            }
            if (GUILayout.Button("MoveToFrontDoor"))
            {
                script.MoveToFrontDoor(selectedSt);
            }
            if (GUILayout.Button("GoToFloor"))
            {
                script.GoToFloor(selectedSt);
            }
            if (GUILayout.Button("ChangeSits"))
            {
                script.ChangeSits();
            }
            if (GUILayout.Button("Expel"))
            {
                script.Expel(selectedSt);
            }
            if (GUILayout.Button("TakeMaterialOut"))
            {
                script.TakeMaterialOut(selectedSt);
            }

            // these are to toggle TEA
            EditorGUILayout.Space(5);
            if (GUILayout.Button("TEA On"))
            {
                script.SetTEATrue(selectedSt);
            }
            if (GUILayout.Button("TEA Off"))
            {
                script.SetTEAFalse(selectedSt);
            }

            EditorGUILayout.Space(5);
            if (GUILayout.Button("Make Students Laugh"))
            {
                script.MakeNearbyStudentsLaugh(selectedSt);
            }
            if (GUILayout.Button("Make Students Talk"))
            {
                script.MakeNearbyStudentsTalk(selectedSt);
            }

            EditorGUILayout.Space(15);

            if(GUILayout.Button("Query LLM"))
            {
                LLMManager.Instance.LLMInteraction_TeacherSpeaksToStudent(script._queryLLMWithThis, selectedSt);
            }
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("TakeMaterialAll"))
        {
            script.TakeMaterialOutAll();
        }

        // these are conflicts
        EditorGUILayout.Space(5);
        if (GUILayout.Button("StandUpConflict"))
        {
            script.StandUpConflict();
        }
        if (GUILayout.Button("Hyperstimulate"))
        {
            script.Hyperstimulate();
        }
        if (GUILayout.Button("GetDistracted"))
        {
            script.GetDistracted();
        }
        if (GUILayout.Button("DrawDistracted"))
        {
            script.DrawDistracted();
        }
        if (GUILayout.Button("BotherRandomStudents"))
        {
            script.BotherRandomStudents();
        }
        if (GUILayout.Button("GetOutMaterialWrong"))
        {
            script.GetOutMaterialWrong();
        }
        if (GUILayout.Button("SitTogether"))
        {
            script.SitTogether();
        }
        //// Mensaje informativo si la lista está vacía
        //EditorGUILayout.HelpBox("No se han cargado GameObjects. Haz clic en 'Refrescar Lista'.", MessageType.Info);

        // Dibuja el resto de variables públicas por defecto si las hubiera
        DrawDefaultInspector();
    }

    /// <summary>
    /// Busca y mapea los GameObjects de la escena de forma dinámica.
    /// </summary>
    private void RefreshStudentList()
    {
        _students = StudentManager.Instance.GetStudents();
        _objectNames = new string[_students.Count];
        for (int i = 0; i < _objectNames.Length; ++i)
        {
            _objectNames[i] = _students[i].Name;
        }
    }
}

#endif
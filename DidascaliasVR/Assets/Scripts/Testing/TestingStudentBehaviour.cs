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

    public void MoveToFrontDoor(Student student)
    {
        student.GetComponent<StudentBehaviour>().MoveToFrontDoor(true);
    }

    public void Expel(Student student)
    {
        student.GetComponent<StudentBehaviour>().Expel();
    }

    public void Hyperstimulate(Student student)
    {
        student.GetComponent<StudentBehaviour>().Hyperstimulate();
    }

    public void GetDistracted(Student student)
    {
        student.GetComponent<StudentBehaviour>().GetDistracted();
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

    public void SitTogether(Student st)
    {
        st.GetComponent<StudentBehaviour>().SitNextToRandomStudentConflict();
    }

    public void SetTEATrue(Student st)
    {
        st.Behaviour.SetTEA(true);
    }

    public void SetTEAFalse(Student st)
    {
        st.Behaviour.SetTEA(false);
    }

    public void GoToFloor(Student st)
    {
        st.Behaviour.GoToFloor();
    }

    public void GetOutMaterialWrong(Student st)
    {
        st.Behaviour.GetOutMaterialWrong();
    }

    public void DrawDistracted(Student st)
    {
        st.Behaviour.DrawDistacted();
    }

    public void BotherRandomStudents(Student st)
    {
        st.Behaviour.BotherOtherStudents();
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
            if (GUILayout.Button("MoveToFrontDoor"))
            {
                script.MoveToFrontDoor(selectedSt);
            }
            if (GUILayout.Button("GoToFloor"))
            {
                script.GoToFloor(selectedSt);
            }

            // these are conflicts
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Expel"))
            {
                script.Expel(selectedSt);
            }
            if (GUILayout.Button("Hyperstimulate"))
            {
                script.Hyperstimulate(selectedSt);
            }
            if (GUILayout.Button("GetDistracted"))
            {
                script.GetDistracted(selectedSt);
            }
            if (GUILayout.Button("DrawDistracted"))
            {
                script.DrawDistracted(selectedSt);
            }
            if (GUILayout.Button("BotherRandomStudents"))
            {
                script.BotherRandomStudents(selectedSt);
            }
            if (GUILayout.Button("GetOutMaterialWrong"))
            {
                script.GetOutMaterialWrong(selectedSt);
            }
            if (GUILayout.Button("ChangeSits"))
            {
                script.ChangeSits();
            }
            if (GUILayout.Button("SitTogether"))
            {
                script.SitTogether(selectedSt);
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

            EditorGUILayout.Space(15);

            if(GUILayout.Button("Query LLM"))
            {
                LLMManager.Instance.LLMInteraction_TeacherSpeaksToStudent(script._queryLLMWithThis, selectedSt);
            }
        }
        else
        {
            // Mensaje informativo si la lista está vacía
            EditorGUILayout.HelpBox("No se han cargado GameObjects. Haz clic en 'Refrescar Lista'.", MessageType.Info);
        }

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
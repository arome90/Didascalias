using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TestingAnimations : MonoBehaviour
{
    [SerializeField]
    private Transform randomPoint = null;

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

    [HideInInspector]
    public UnityEvent onClassCreated = new UnityEvent();
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestingAnimations))]
public class SpawnStudentsOnStartEditor : Editor
{
    private int _selectedIndex = 0;
    private List<Student> _students = new List<Student>();
    private string[] _objectNames = new string[0];

    public override void OnInspectorGUI()
    {
        // Referencia al script original
        TestingAnimations script = (TestingAnimations)target;

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

            Student objetoSeleccionado = _students[_selectedIndex];

            // these are Movement testing
            EditorGUILayout.Space(5);
            if (GUILayout.Button("StandUp"))
            {
                script.StandUp(objetoSeleccionado);
            }
            if (GUILayout.Button("SitDown"))
            {
                script.SitDown(objetoSeleccionado);
            }
            if (GUILayout.Button("LeaveDesk"))
            {
                script.LeaveDesk(objetoSeleccionado);
            }
            if (GUILayout.Button("MoveToRandomPoint"))
            {
                script.MoveToRandomPoint(objetoSeleccionado);
            }
            if (GUILayout.Button("MoveToFrontDoor"))
            {
                script.MoveToFrontDoor(objetoSeleccionado);
            }
            if (GUILayout.Button("GoToFloor"))
            {
                script.GoToFloor(objetoSeleccionado);
            }

            // these are conflicts
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Expel"))
            {
                script.Expel(objetoSeleccionado);
            }
            if (GUILayout.Button("Hyperstimulate"))
            {
                script.Hyperstimulate(objetoSeleccionado);
            }
            if (GUILayout.Button("GetDistracted"))
            {
                script.GetDistracted(objetoSeleccionado);
            }
            if (GUILayout.Button("GetOutMaterialWrong"))
            {
                script.GetOutMaterialWrong(objetoSeleccionado);
            }
            if (GUILayout.Button("ChangeSits"))
            {
                script.ChangeSits();
            }
            if (GUILayout.Button("SitTogether"))
            {
                script.SitTogether(objetoSeleccionado);
            }

            // these are to toggle TEA
            EditorGUILayout.Space(5);
            if (GUILayout.Button("TEA On"))
            {
                script.SetTEATrue(objetoSeleccionado);
            }
            if (GUILayout.Button("TEA Off"))
            {
                script.SetTEAFalse(objetoSeleccionado);
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
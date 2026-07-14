using System;
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

        student.GetComponent<StudentBehaviour>().MoveTo(randomPoint, true);
    }

    public void MoveToFrontDoor(Student student)
    {
        student.GetComponent<StudentBehaviour>().MoveToFrontDoor(true);
    }

    public void Expel(Student student)
    {
        student.GetComponent<StudentBehaviour>().Expel();
    }

    [HideInInspector]
    public UnityEvent onClassCreated = new UnityEvent();
}

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

            EditorGUILayout.Space(5);

            // 2. Botones para disparar las funciones pasando el parámetro
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
            if (GUILayout.Button("Expel"))
            {
                script.Expel(objetoSeleccionado);
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
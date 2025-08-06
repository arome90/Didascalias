using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Singleton que nos permite acceder a los diferentes estudiantes, 
/// además de manejarlos y generarlos según queramos
/// </summary>
public class StudentManager : Singleton<StudentManager>
{
    [SerializeField,
        Tooltip("Prefab de estudiante")]
    GameObject _studentPrefab;

    [SerializeField,
        Tooltip("Nombres de estudiantes")]
    StudentNames _studentNames;

    /// <summary>
    /// Diccionaria de estudiantes relacionados por su nombre
    /// </summary>
    Dictionary<string, Student> _students = null;

    ClassSettings _settings;

    protected override void Awake()
    {
        base.Awake();
        _settings = ClassManager.Instance.Settings;
    }

    /// <summary>
    /// Destruye a los estudiantes ya generados y limpia el diccionario de estudiantes
    /// </summary>
    void DestroyStudents()
    {
        if(_students == null)
        {
            _students = new Dictionary<string, Student>();
        }
        else
        {
            foreach (var student in _students)
            {
                Destroy(student.Value.gameObject);
            }
            _students.Clear();
        }
    }

    /// <summary>
    /// Borra a los antiguos estudiantes y genera tantos
    /// estudiantes como estén especificados en las ClassSettings
    /// 
    /// AVISO: Este método no coloca a los estudiantes en ningún lugar, tan solo los instancia
    /// </summary>
    /// <returns> La lista de estudiantes generados </returns>
    public List<Student> GenerateStudents()
    {
        DestroyStudents();
        List<Student> students = new List<Student>();

        List<string> boyNames = _studentNames.BoyNames;
        List<string> girlNames = _studentNames.GirlNames;

        int numBoys = 0;
        int numGirls = 0;

        // Esto sirve para decirle al componente LookAtConstaint de la Name Tag del estudiante
        // que mira constantemente a la cámra del jugador. Para encontrar al jugador, buscamos
        // el XR Origin, que es un componente único del jugador.
        ConstraintSource constraintSource = new ConstraintSource 
            { sourceTransform = FindAnyObjectByType<XROrigin>().
            GetComponentInChildren<Camera>().transform, weight = 1.0f };

        for (int i = 0; i < _settings.NumStudents; ++i)
        {
            Student st = Instantiate(_studentPrefab).GetComponent<Student>();

            string name;
            if ((Random.Range(0, 2) == 0 && numBoys < _settings.NumBoys) || numGirls == _settings.NumGirls)
            {
                st.Gender = Gender.Boy; 

                int index = Random.Range(0, boyNames.Count);
                name = boyNames[index];
                boyNames.RemoveAt(index);

                numBoys++;
            }
            else
            {
                st.Gender = Gender.Girl;

                int index = Random.Range(0, girlNames.Count);
                name = girlNames[index];
                girlNames.RemoveAt(index);

                numGirls++;
            }

            LookAtConstraint lookAt = st.GetComponentInChildren<LookAtConstraint>();
            lookAt.AddSource(constraintSource);
            lookAt.constraintActive = true;

            st.Name = name;
            students.Add(st);
            _students.Add(st.Name, st);
        }

        return students;
    }
}

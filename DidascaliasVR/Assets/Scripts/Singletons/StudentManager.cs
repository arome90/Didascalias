using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Lista de estudiantes seleccionados, sobre los que se aplicarán las acciones
    /// </summary>
    List<string> _selectedStudents = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        _settings = ClassManager.Instance.Settings;
    }

    private void OnEnable()
    {
        VoiceActivation.Instance.OnValidatePartialResponse.AddListener(SelectStudents);
    }

    private void OnDisable()
    {
        VoiceActivation.Instance.OnValidatePartialResponse.RemoveListener(SelectStudents);
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
    /// Devuelve el estudiante de nombre "name"
    /// </summary>
    /// <param name="name"> nombre del estudiante </param>
    /// <returns> el estudiante en cuestión. "null" en caso de error </returns>
    public Student GetStudent(string name)
    {
        if (_students.TryGetValue(name, out Student st)) return st;
        else return null;
    }

    public List<string> GetSelectedStudents()
    {
        return _selectedStudents;
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
            GameObject go = Instantiate(_studentPrefab);
            Student st = go.GetComponent<Student>();

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
            go.name = name;
            students.Add(st);
            _students.Add(st.Name, st);
        }

        return students;
    }

    /// <summary>
    /// Devuelve los estudiantes (por orden de creación)
    /// </summary>
    /// <returns> Estudiantes por orden de creación </returns>
    public List<Student> GetStudents()
    {
        return _students.Values.ToList();
    }

    /// <summary>
    /// Selecciona a los estudiantes que la transcripción de Wit haya entendido
    /// Primero, deselecciona a los anteriores que estaban seleccionados antes
    /// de continuar
    /// </summary>
    /// <param name="data"> Transcripción de Wit </param>
    public void SelectStudents(WitMessageData data)
    {
        // Solo quitamos a los estudiantes anteriormente seleccionados
        // si el mensaje de Wit ha encontrado nuevos estudiantes
        if(data.Names.Count > 0)
        {
            foreach (string st in _selectedStudents)
            {
                _students[st].Deselect();
            }
            _selectedStudents.Clear();
        }

        foreach(string name in data.Names)
        {
            if(_students.TryGetValue(name, out Student st))
            {
                st.Select();
                _selectedStudents.Add(st.Name);
            }
        }
    }

    /// <summary>
    /// Llamado al expulsar a un estudiante.
    /// Mueve a los estudiantes seleccionados a la puerta del aula.
    /// </summary>
    public void OnStudentExpelled()
    {
        foreach(string st in _selectedStudents)
        {
            _students[st].GetComponent<StudentBehaviour>().ExpelStudent();
        }
    }

    public void OnStudentSit()
    {
        if (_selectedStudents.Count == 0) return;

        StudentBehaviour st = _students[_selectedStudents[0]].GetComponent<StudentBehaviour>();
        st.OnSitDownRequested.Invoke();
    }

    public void OnChangePlaces()
    {
        if(_selectedStudents.Count <= 1) { return; }
        else
        {
            StudentBehaviour _st1 = _students[_selectedStudents[0]].GetComponent<StudentBehaviour>();
            StudentBehaviour _st2 = _students[_selectedStudents[1]].GetComponent<StudentBehaviour>();

            _st1.ChangeSitSpotWithStudent(_st2);

            _st1.OnChangePlacesRequested.Invoke();
            _st2.OnChangePlacesRequested.Invoke();
        }
    }

    #region DEBUG MUERTE Y DESTRUCCION BORRAR
    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.V))
        {
            _selectedStudents.Clear();
            int i = 0;
            foreach(string name in _students.Keys)
            {
                _selectedStudents.Add(name);
                ++i;
                if (i == 2) break;
            }
            OnChangePlaces();
        }
    }
    #endregion
}

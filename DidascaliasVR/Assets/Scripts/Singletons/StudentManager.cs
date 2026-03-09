using System.Collections.Generic;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Singleton que nos permite acceder a los diferentes estudiantes, 
/// adem�s de manejarlos y generarlos seg�n queramos
/// </summary>
public class StudentManager : Singleton<StudentManager>
{
    [SerializeField,
        Tooltip("Prefab de estudiante")]
    GameObject _studentPrefab;

    [SerializeField,
        Tooltip("Prefab que representa un conflicto (con el script Conflict)")]
    GameObject _conflictPrefab;

    [SerializeField, Range(1, 5)]
    private int _maxActiveConflicts = 1;

    private Dictionary<string, Conflict> _activeConflicts = null;

    [SerializeField,
        Tooltip("Nombres de estudiantes")]
    StudentNames _studentNames;

    /// <summary>
    /// Diccionaria de estudiantes relacionados por su nombre
    /// </summary>
    Dictionary<string, Student> _students = null;

    ClassSettings _settings;

    /// <summary>
    /// Lista de estudiantes seleccionados, sobre los que se aplicar�n las acciones
    /// </summary>
    List<string> _selectedStudents = new List<string>();

    protected override void Awake()
    {
        base.Awake();
        _settings = ClassManager.Instance.Settings;
        _activeConflicts = new Dictionary<string, Conflict>();
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
    /// <returns> el estudiante en cuesti�n. "null" en caso de error </returns>
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
    /// estudiantes como est�n especificados en las ClassSettings
    /// 
    /// AVISO: Este m�todo no coloca a los estudiantes en ning�n lugar, tan solo los instancia
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
        // que mira constantemente a la c�mra del jugador. Para encontrar al jugador, buscamos
        // el XR Origin, que es un componente �nico del jugador.
        ConstraintSource constraintSource = new ConstraintSource 
            { sourceTransform = FindAnyObjectByType<XROrigin>().
            GetComponentInChildren<Camera>().transform, weight = 1.0f };

        Student last = null;
        for (int i = 0; i < _settings.NumStudents; ++i)
        {
            GameObject go = Instantiate(_studentPrefab);
            Student st = go.GetComponent<Student>();

            if (last != null) last.NextStudent = st;

            st.PreviousStudent = last;

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

            last = st;
        }

        if(students.Count > 1 && ClassManager.Instance.Settings.ClassShape == ClassSettings.Shape.Circular)
        {
            students[0].PreviousStudent = students[students.Count - 1];
            students[students.Count - 1] = students[0];
        }

        return students;
    }

    /// <summary>
    /// Devuelve los estudiantes (por orden de creaci�n)
    /// </summary>
    /// <returns> Estudiantes por orden de creaci�n </returns>
    public List<Student> GetStudents()
    {
        return _students.Values.ToList();
    }

    /// <summary>
    /// Selecciona a los estudiantes que la transcripci�n de Wit haya entendido
    /// Primero, deselecciona a los anteriores que estaban seleccionados antes
    /// de continuar
    /// </summary>
    /// <param name="data"> Transcripci�n de Wit </param>
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

    private Student TryGetStudentByNameOrGetRandom(string studentName)
    {
        Student st = null;
        if(studentName != null)
        {
            st = GetStudent(studentName);
        } 
        else if (st == null)
        {
            st = GetStudents()[Random.Range(0, _students.Count)];
        }
        return st;
    }

    public Student GetStudentFarFromOtherStudent(Student other)
    {
        // Hacemos una lista por copia de los valores de los estudiantes y quitamos los que NO queremos coger
        List<Student> students = _students.Values.ToList();
        students.Remove(other);
        students.Remove(other.NextStudent);
        students.Remove(other.PreviousStudent);

        Student st = students[Random.Range(0, students.Count)];

        return st;
    }

    #region WEB EVENTS
    public void MakeStudentTalk(string studentName, string message)
    {
        Student st = TryGetStudentByNameOrGetRandom(studentName);

        st.Speak(message);        
    }

    public enum ConflictType
    {
        Disrespect = 0,
        SitTogether = 1,
        StandUp = 2,
    }

    public void GenerateConflict(ConflictType type, string studentName)
    {
        Conflict conflict;
        if (_activeConflicts.Count < _maxActiveConflicts)
        {
            conflict = Instantiate(_conflictPrefab).GetComponent<Conflict>();
        }
        else return;

        Student st = TryGetStudentByNameOrGetRandom(studentName);
        conflict.SetConflictiveStudent(st);
        _activeConflicts.Add(st.Name, conflict);

        switch (type)
        {
            case ConflictType.Disrespect:

                st.Speak("�Prueba de Insulto!");

                // Aqu� coger�amos lo correspondiente para hacer una animaci�n de faltar al respeto
                st.GetComponent<StudentBehaviour>().Yell();

                break;

            case ConflictType.SitTogether:
                // Si solo hay un estudiante, esto no puede tener efecto. 
                // TODO: Cambiar el mensaje que se env�a al servidor,
                // pero para eso hay que cambiar c�mo recibe el servidor las cosas :p
                if (_students.Count < 3) break;

                List<Student> sts = GetStudents();

                // En el caso espec�fico de que existan 3 estudiantes y sea el del medio el seleccionado,
                // el del medio ya est� sentado junto a sus dos compa�eros, por lo que no puede ser
                // este conflicto. Tenemos que escoger otro estudiante, ya sea el primer o el �ltimo
                if (_students.Count == 3 && st == sts[1])
                {
                    st.Deselect();
                    _activeConflicts.Remove(st.Name);
                    st = sts[Random.Range(0,2) == 0 ? 0 : 2];
                    conflict.SetConflictiveStudent(st);
                }

                st.GetComponent<StudentBehaviour>().OnSitTogetherRequested.Invoke();
                //Student otherSt = null; 

                //// esta es una forma fea de coger un segundo estudiante aleatorio, pero no se me ocurre ahora c�mo cambiarlo
                //// TODO: Lista de estudiantes "disponibles" para seleccionar
                //while ((otherSt == null || otherSt.Name == st.Name) && 
                //    // Buscamos un estudiante que no est� cerca del original
                //    otherSt != st.NextStudent && otherSt != st.PreviousStudent)
                //{
                //    otherSt = TryGetStudentByNameOrGetRandom(null);
                //}

                //otherSt.SetAsConflictive();


                // Student nonConflictiveStudent = otherSt.NextStudent == null ? otherSt.PreviousStudent : otherSt.NextStudent;

                break;

            case ConflictType.StandUp:
                st.GetComponent<StudentBehaviour>().StartStandUpAnimation();
                break;
        }
    }

    public void ResolveConflicts()
    {
        foreach(string st in _selectedStudents)
        {
            _activeConflicts[st].ReceivePositiveResolution();
        }

        _activeConflicts.Clear();
    }

    public void RemoveConflict(Student s)
    {
        _activeConflicts.Remove(s.Name);
    }
    #endregion

    #region DEBUG MUERTE Y DESTRUCCION BORRAR
    //private void Update()
    //{
    //    if(Input.GetKeyUp(KeyCode.V))
    //    {
    //        _selectedStudents.Clear();
    //        int i = 0;
    //        foreach(string name in _students.Keys)
    //        {
    //            _selectedStudents.Add(name);
    //            ++i;
    //            if (i == 2) break;
    //        }
    //        OnChangePlaces();
    //    }
    //}
    #endregion
}

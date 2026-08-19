using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Singleton que nos permite acceder a los diferentes estudiantes, 
/// adem�s de manejarlos y generarlos seg�n queramos
/// </summary>
public class StudentManager : Singleton<StudentManager>
{
    [SerializeField,
       Tooltip("Prefabs de estudiante")]
    GameObject[] _boyStudentPrefabs;

    [SerializeField,
       Tooltip("Prefabs de estudiante")]
    GameObject[] _girlStudentPrefabs;

    [SerializeField,
        Tooltip("Prefab que representa un conflicto (con el script Conflict)")]
    GameObject _conflictPrefab;

    [SerializeField, Range(1, 5)]
    private int _maxActiveConflicts = 1;

    private List<Conflict> _activeConflicts = null;

    [SerializeField,
        Tooltip("Nombres de estudiantes")]
    StudentNames _studentNames;

    /// <summary>
    /// Diccionario de estudiantes relacionados por su nombre
    /// </summary>
    Dictionary<string, Student> _students = null;

    [SerializeField, Range(0, 1), Tooltip("Proportion of autistic students in class")]
    private float _autisticStudentsProportion = 0.1f;
    [SerializeField, Tooltip("Always makes sure that there is at least one autistic student")]
    private bool _atLeastOneAutistic = true;

    [SerializeField, Range(0, 1), Tooltip("Proportion of students with ADHD in class")]
    private float _adhdStudentsProportion = 0.1f;
    [SerializeField, Tooltip("Always makes sure that there is at least one ADHD student; only failing when there is only 1 and 'At Least One Autitic' is checked, making that student Autistic instead of ADHD")]
    private bool _atLeastOneADHD = true;

    /// <summary>
    /// Non-normative students (ADHD and Autism)
    /// </summary>
    List<Student> _nonNormativeStudents = null;

    List<Student> _adhdStudents = null;

    List<Student> _autisticStudents = null;

    ClassSettings _settings;

    /// <summary>
    /// Lista de estudiantes seleccionados, sobre los que se aplicar�n las acciones
    /// </summary>
    List<string> _selectedStudents = new List<string>();

    [SerializeField, SerializedDictionary]
    SerializedDictionary<StudentType, BehaviourPatternModifier> _behaviourModifiers = null;

    public BehaviourPatternModifier GetBehaviourModifier(StudentType type) => _behaviourModifiers[type];

    protected override void Awake()
    {
        base.Awake();
        _settings = ClassManager.Instance.Settings;
        _activeConflicts = new List<Conflict>();
    }

    protected void Start()
    {
        if (_behaviourModifiers == null)
        {
            _behaviourModifiers = new SerializedDictionary<StudentType, BehaviourPatternModifier>();

            _behaviourModifiers.Add(StudentType.Problematic, Resources.Load("BehaviourPatterns/" + StudentType.Problematic.ToString()) as BehaviourPatternModifier);
            _behaviourModifiers.Add(StudentType.Talkative, Resources.Load("BehaviourPatterns/" + StudentType.Talkative.ToString()) as BehaviourPatternModifier);
            _behaviourModifiers.Add(StudentType.NonParticipative_NonProblematic, Resources.Load("BehaviourPatterns/" + StudentType.NonParticipative_NonProblematic.ToString()) as BehaviourPatternModifier);
            _behaviourModifiers.Add(StudentType.Participative_NonProblematic, Resources.Load("BehaviourPatterns/" + StudentType.Participative_NonProblematic.ToString()) as BehaviourPatternModifier);
            _behaviourModifiers.Add(StudentType.Autistic, Resources.Load("BehaviourPatterns/" + StudentType.Autistic.ToString()) as BehaviourPatternModifier);
            _behaviourModifiers.Add(StudentType.ADHD, Resources.Load("BehaviourPatterns/" + StudentType.ADHD.ToString()) as BehaviourPatternModifier);
        }
    }

    private void OnEnable()
    {
        if (VoiceActivation.Exists)
        {
            VoiceActivation.Instance.OnValidatePartialResponse.AddListener(SelectStudents);
        }
        else
        {
            Didascalia.Utils.Log.Warning("Voice Activation not found on enable", this);
        }
    }

    private void OnDisable()
    {
        if (VoiceActivation.Exists)
        {
            VoiceActivation.Instance.OnValidatePartialResponse.RemoveListener(SelectStudents);
        }
        else
        {
            Didascalia.Utils.Log.Warning("Voice Activation not found on enable", this);
        }
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
    public Student GetStudentByName(string name)
    {
        Student st = GetStudent(name);
        Didascalia.Utils.Error.DebugbreakFailIf(st == null, $"No student found with name: {name}", this);
        return st;
    }

    public List<string> GetSelectedStudents()
    {
        return _selectedStudents;
    }

    public List<Student> GetStudentsNearOrigin(Student origin)
    {
        Collider[] hitColliders = new Collider[9];
        int numColliders = Physics.OverlapSphereNonAlloc(origin.transform.position, 4.5f, hitColliders, LayerMask.GetMask("Student"));

        List<Student> students = new List<Student>();

        foreach (Collider collider in hitColliders)
        {
            if (collider == null) continue;

            Student st = collider.GetComponentInParent<Student>();

            // Evitamos añadir al alumno origen
            if (st == null || st == origin) continue;

            // Vector de dirección desde el origen hacia el otro estudiante (en espacio local del origen)
            Vector3 localDir = origin.transform.InverseTransformDirection(st.transform.position - origin.transform.position).normalized;

            // Descartamos si está DELANTE (z > 0.1) y además es DIAGONAL/LATERAL (abs(x) > 0.1)
            // Esto elimina exactamente la esquina delantera-izquierda y delantera-derecha
            bool isFrontLeftCorner = localDir.z > 0.1f && localDir.x < -0.1f;
            bool isFrontRightCorner = localDir.z > 0.1f && localDir.x > 0.1f;

            if (!isFrontLeftCorner && !isFrontRightCorner)
            {
                students.Add(st);
            }
        }

        return students;
    }


    // TODO: Change all these to a single function that accepts a callback or unityaction as an argument
    public void MakeNearbyStudentsReactToPositivelyResolvedConflict(Student origin)
    {
        List<Student> students = GetStudentsNearOrigin(origin);

        foreach (Student student in students)
        {
            student.Behaviour.ReactToPositivelyResolvedConflict(origin);
        }
    }

    public void MakeNearbyStudentsReactToNeutrallyResolvedConflict(Student origin)
    {
        List<Student> students = GetStudentsNearOrigin(origin);

        foreach (Student student in students)
        {
            student.Behaviour.ReactToNeutrallyResolvedConflict(origin);
        }
    }

    public void MakeNearbyStudentsReactToBadlyResolvedConflict(Student origin)
    {
        List<Student> students = GetStudentsNearOrigin(origin);

        foreach (Student student in students)
        {
            student.Behaviour.ReactToBadlyResolvedConflict(origin);
        }
    }

    public void MakeNearbyStudentsLaugh(Student origin)
    {
        List<Student> students = GetStudentsNearOrigin(origin);

        foreach (Student student in students)
        {
            MakeStudentLaugh(student, origin);
        }
    }

    public void MakeNearbyStudentsTalk(Student origin)
    {
        List<Student> students = GetStudentsNearOrigin(origin);

        foreach (Student student in students)
        {
            MakeStudentTalk(student, origin);
        }
    }

    public void MakeStudentLaugh(Student st, Student origin)
    {
        st.Behaviour.LookAtTarget(origin.transform);
        st.Behaviour.Laugh();
    }

    public void MakeStudentTalk(Student st, Student origin = null)
    {
        st.Behaviour.LookAtTarget(origin.transform);
        st.Behaviour.StartTalking();
    }

    public void GetMaterialOutAllStudents()
    {
        foreach (Student st in _students.Values)
        {
            st.Behaviour.TriggerGetMaterialOut();
        }
    }

    // we specifically ask for a copy of the students to avoid any mistakes :)
    public void AsignStudentType(List<Student> studentsCopy)
    {
        if (studentsCopy == null || studentsCopy.Count == 0)
        {
            Debug.LogWarning("La lista de estudiantes está vacía.");
            return;
        }

        // if there are less than five students, we do not bother in making them problematic.
        // it's easier to handle fewer students
        if (studentsCopy.Count > 5)
        {
            // at least one problematic
            // we can't use TryGetStudentOrGetRandom() because this method is called before all students are generated and therefore
            // assigned to the Students list variable
            int rand = UnityEngine.Random.Range(0, studentsCopy.Count);
            Student st = studentsCopy[rand];
            st.StType = StudentType.Problematic;
            LLMManager.Instance.GenerateStudentContext(st);

            studentsCopy.Remove(st);

            // at least one talkative
            rand = UnityEngine.Random.Range(0, studentsCopy.Count);
            st = studentsCopy[rand];
            st.StType = StudentType.Talkative;
            LLMManager.Instance.GenerateStudentContext(st);

            studentsCopy.Remove(st);

            // at least one participative
            rand = UnityEngine.Random.Range(0, studentsCopy.Count);
            st = studentsCopy[rand];
            st.StType = StudentType.Participative_NonProblematic;
            LLMManager.Instance.GenerateStudentContext(st);

            studentsCopy.Remove(st);
        }

        foreach (Student student in studentsCopy) { student.StType = SelectStudentType(); LLMManager.Instance.GenerateStudentContext(student); }
    }

    private static StudentType SelectStudentType()
    {
        int weightNonParticipative = 90;    // NonParticipative_NonProblematic: 70% (Gran mayoría)
        int weightParticipative = 10;       // Participative_NonProblematic: 10%
        int weightTalkative = 10;           // Talkative: 10%
        int weightProblematic = 10;         // Problematic: 10%

        int totalWeights = weightNonParticipative + weightParticipative + weightTalkative + weightProblematic;
        int rand = UnityEngine.Random.Range(0, totalWeights);

        if (rand < weightNonParticipative)  return StudentType.NonParticipative_NonProblematic;

        rand -= weightNonParticipative;
        if (rand < weightParticipative)     return StudentType.Participative_NonProblematic;

        rand -= weightParticipative;
        if (rand < weightTalkative)         return StudentType.Talkative;

        else                                return StudentType.Problematic;
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
        // una Camara, que es un componente �nico del jugador.
        ConstraintSource constraintSource = new ConstraintSource 
            { sourceTransform = FindAnyObjectByType<Camera>().
            transform, weight = 1.0f };

        Student last = null;
        for (int i = 0; i < _settings.NumStudents; ++i)
        {
            bool isBoy = (UnityEngine.Random.Range(0, 2) == 0 && numBoys < _settings.NumBoys) || numGirls == _settings.NumGirls;

            GameObject[] chosenPrefabs = null;

            if (isBoy)
            {
                chosenPrefabs = _boyStudentPrefabs;
            }
            else
            {
                chosenPrefabs = _girlStudentPrefabs;
            }

            // TO DO -> cambiar la creacion de personajes por algo definitivo y no aleatorio 
            int studentMeshID = UnityEngine.Random.Range(0, chosenPrefabs.Length);
            GameObject go = Instantiate(chosenPrefabs[studentMeshID]);
            Student st = go.GetComponent<Student>();

            if (last != null) last.NextStudent = st;

            st.PreviousStudent = last;

            string name;
            if (isBoy)
            {
                int index = UnityEngine.Random.Range(0, boyNames.Count);
                name = boyNames[index];
                boyNames.RemoveAt(index);

                numBoys++;
            }
            else
            {
                int index = UnityEngine.Random.Range(0, girlNames.Count);
                name = girlNames[index];
                girlNames.RemoveAt(index);

                numGirls++;
            }

            LookAtConstraint lookAt = st.GetComponentInChildren<LookAtConstraint>();
            lookAt.AddSource(constraintSource);
            lookAt.constraintActive = true;

            st.Name = name;
            go.name = name;

            st.Age = UnityEngine.Random.Range(13, 16);

            students.Add(st);
            _students.Add(st.Name, st);

            last = st;
        }

        if(students.Count > 1 && ClassManager.Instance.Settings.ClassShape == ClassSettings.Shape.Circular)
        {
            students[0].PreviousStudent = students[students.Count - 1];
            students[students.Count - 1] = students[0];
        }

        // Non-Normative students
        List<Student> stCopy = new List<Student>(students);

        _nonNormativeStudents = new List<Student>();

        // Autistic Students
        {
            _autisticStudents = new List<Student>();
            int numAutism = (int)(_autisticStudentsProportion * students.Count);

            // if there is at least one autistic student, we set it to the actual number or to 1 if numAutism is 0
            numAutism = _atLeastOneAutistic ? Mathf.Max(numAutism, 1) : numAutism;

            for (int i = 0; i < numAutism; ++i)
            {
                int index = UnityEngine.Random.Range(0, stCopy.Count);
                Student st = stCopy[index];
                st.Behaviour.SetAutism(true);
                st.StType = StudentType.Autistic;
                stCopy.Remove(st);

                _nonNormativeStudents.Add(st);
                _autisticStudents.Add(st);
            }
        }

        // ADHD students
        {
            _adhdStudents = new List<Student>();
            int numAdhd = (int)(_adhdStudentsProportion * students.Count);

            // if there is at least one adhd student, we set it to the actual number or to 1 if numAutism is 0
            numAdhd = _atLeastOneADHD ? Mathf.Max(numAdhd, 1) : numAdhd;

            for (int i = 0; i < numAdhd; ++i)
            {
                int index = UnityEngine.Random.Range(0, stCopy.Count);
                Student st = stCopy[index];
                st.Behaviour.SetADHD(true);
                st.StType = StudentType.ADHD;
                stCopy.Remove(st);

                _nonNormativeStudents.Add(st);
                _adhdStudents.Add(st);
            }
        }

        // This also adds the LLM Context
        // We use the stCopy to avoid giving Autistic and ADHD students another type
        AsignStudentType(stCopy);

        Didascalia.Utils.Log.Info("Generated students: " + string.Join(", ", students.Select(s => s.Name)), this);
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

    public List<Student> GetAutisticStudents()
    {
        return _autisticStudents;
    }

    public List<Student> GetADHDStudents()
    {
        return _adhdStudents;
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

    public void DeselectStudents()
    {
        foreach (string st in _selectedStudents)
        {
            _students[st].Deselect();
        }
        _selectedStudents.Clear();
    }

    public void SelectStudent(string name)
    {
        if (_students.TryGetValue(name, out Student st))
        {
            st.Select();
            _selectedStudents.Add(st.Name);
        }
    }

    public void OnChangePlaces()
    {
        if(_selectedStudents.Count <= 1) { return; }
        else
        {
            Student s1 = _students[_selectedStudents[0]];
            Student s2 = _students[_selectedStudents[1]];

            StudentBehaviour sb1 = s1.GetComponent<StudentBehaviour>();
            StudentBehaviour sb2 = s2.GetComponent<StudentBehaviour>();

            sb1.SitOnNewPlace(s2.Desk);
            sb2.SitOnNewPlace(s1.Desk);
        }
    }

    public Student TryGetStudentByNameOrGetRandom(string studentName)
    {
        Student st = null;
        if(studentName != null)
        {
            st = GetStudentByName(studentName);
        } 
        else if (st == null)
        {
            st = GetStudents()[UnityEngine.Random.Range(0, _students.Count)];
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

        Student st = students.Next();

        return st;
    }

    public Student GetSittingStudentFarFromOtherStudent(Student other)
    {
        // Hacemos una lista por copia de los valores de los estudiantes y quitamos los que NO queremos coger
        List<Student> students = _students.Values.ToList();
        students.Remove(other);
        students.Remove(other.NextStudent);
        students.Remove(other.PreviousStudent);

        Student st = students.Next();
        while (!st.Behaviour.IsSittingOnTheirDesk() && students.Count > 0) { students.Remove(st); st = students.Next(); }

        // no student was found
        if (students.Count == 0 && !st.Behaviour.IsSittingOnTheirDesk()) { return null; }

        return st;
    }

    public Student GetStudentDifferentFromGiven(List<Student> others)
    {
        // Hacemos una lista por copia de los valores de los estudiantes y quitamos los que NO queremos coger
        List<Student> students = _students.Values.ToList();
        foreach (Student other in others)
        {
            students.Remove(other);
        }

        Student st = students[UnityEngine.Random.Range(0, students.Count)];

        return st;
    }

    public Student GetNearestStudent(Student other)
    {
        if (other.Behaviour.IsSittingOnChair())
        {
            Student st1 = other.NextStudent;
            Student st2 = other.PreviousStudent;

            if (st1 == null && st2 == null) return null;
            else if (st1 == null) return st2;
            else if (st2 == null) return st1;
            else
            {
                float distanceTo1 = (other.transform.position - st1.transform.position).magnitude;
                float distanceTo2 = (other.transform.position - st2.transform.position).magnitude;

                if (distanceTo1 > distanceTo2) return st2;
                else return st1;
            }
        }
        else
        {
            Collider[] hitColliders = new Collider[9];
            Vector3 originPos = other.transform.position;
            int numColliders = Physics.OverlapSphereNonAlloc(originPos, 10.0f, hitColliders, LayerMask.GetMask("Student"));

            Student closestStudent = null;
            float closestDistanceSqr = Mathf.Infinity;

            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i] == null) continue;

                Student st = hitColliders[i].GetComponentInParent<Student>();

                // Omitimos si es el propio objeto origen o no tiene componente Student
                if (st == null || st == other) continue;

                // Calculamos la distancia al cuadrado (más eficiente que Vector3.Distance al evitar la raíz cuadrada)
                float dSqrToTarget = (st.transform.position - originPos).sqrMagnitude;

                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    closestStudent = st;
                }
            }

            return closestStudent;
        }
    }

    #region WEB EVENTS
    public void MakeStudentTalk(string studentName, string message)
    {
        Student st = TryGetStudentByNameOrGetRandom(studentName);

        st.Speak(message);        
    }

    #region Conflict' Safety Checks
    private bool IsConflictCapacityOk(ConflictType type, out ConflictGenerationResult erroneousResult)
    {
        if (_activeConflicts.Count == _maxActiveConflicts)
        {
            erroneousResult = new ConflictGenerationResult
            {
                Error = ConflictGenerationError.MaxActiveConflictsReached,
                errorWhy = "Can't generate conflict because there are too many active conflicts at the same time",
                ConflictInstance = null
            };
            return false;
        }
        else
        {
            erroneousResult = default;
            erroneousResult.Error = ConflictGenerationError.None;
            return true;
        }
    }

    // WE ARE NOT USING THIS BECAUSE WE CAN'T SELECT THE TARGET STUDENT FOR A CONFLICT (FOR NOW...)
    private bool IsStudentConflictFree(ConflictType type, out ConflictGenerationResult result, Student st = null)
    {
        result = default;
        bool isOK = false;

        // if it's null (we decide which student can be :))
        if (st == null || st.ActiveConflict == null) 
        {
            result.Error = ConflictGenerationError.None;
            isOK = true;
        }
        // TODO: Assing a conflict to the student directly!!
        else if (st.ActiveConflict != null)
        {
            result.Error = ConflictGenerationError.AlreadyActiveConflictForStudent;
            result.errorWhy = $"Selected student ({st.Name}) already has an active conflict";
            isOK = false;
        }

        return isOK;
    }
    #endregion

    public ConflictGenerationResult GenerateConflict(ConflictType type)
    {
        ConflictGenerationResult result;

        if (!IsConflictCapacityOk(type, out result)) return result;

        // if (!IsStudentConflictFree(type, out result, st)) return result;

        result = ConflictFactory.CreateConflict(type);
        if (result.Error != ConflictGenerationError.None) return result;

        ConflictSetupResult setupResult = result.ConflictInstance.IsConflictFeasible();
        if (setupResult.Error != ConflictGenerationError.None)
        {
            Debug.LogError($"[StuentManager] Error while setting up conflict '{type.ToString()}'.\nError: {setupResult.errorWhy}.");
            result.ConflictInstance = null;
        }

        return result;
    }

    /// <summary>
    /// Executes the given conflict.
    /// You MUST have called IsConflictFeasible and checked it is feasible before calling this method
    /// </summary>
    /// <param name="conflict"></param>
    public void HandleConflict(Conflict conflict)
    {
        conflict.StartConflict();
        _activeConflicts.Add(conflict);
        Player.Instance.AddActiveConflict(conflict);
    }

    public void RemoveActiveConflict(Conflict conflict)
    {
        _activeConflicts.Remove(conflict);
        Player.Instance.RemoveActiveConflict(conflict);
    }
    #endregion
}

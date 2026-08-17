using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    //[SerializeField,
    //    Tooltip("Prefabs de estudiante")]
    //GameObject[] _studentPrefab;

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
        _activeConflicts = new Dictionary<string, Conflict>();
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
            students.Add(st);
        }

        students.Remove(origin);

        return students;
    }
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

    /// <summary>
    /// Llamado al expulsar a un estudiante.
    /// Mueve a los estudiantes seleccionados a la puerta del aula.
    /// </summary>
    public void OnStudentExpelled()
    {
        foreach(string st in _selectedStudents)
        {
            _students[st].GetComponent<StudentBehaviour>().Expel();
        }
    }

    public void OnStudentSit()
    {
        if (_selectedStudents.Count == 0) return;

        StudentBehaviour st = _students[_selectedStudents[0]].GetComponent<StudentBehaviour>();
        st.SitDown();
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

        Student st = students[UnityEngine.Random.Range(0, students.Count)];

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
            Collider[] hitCollider = new Collider[1];
            int numColliders = Physics.OverlapSphereNonAlloc(other.transform.position, 100.0f, hitCollider, LayerMask.GetMask("Student"));

            return hitCollider[0].GetComponentInParent<Student>();
        }

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
        SitTogether,
        StandUp,

        // tea
        Hyperstimulation,
        Frustration,

        // tdah
        Disorganization,
        Impulsivity,
        Inattention,

        // NonFeasible = 1 << 31
    }
    public const ConflictType ConflictTypeNonFeasible = (ConflictType)(1 << 31);
    internal struct ConflictDescriptorDisrespect
    {
        public string StudentName;
    }
    internal struct ConflictDescriptorSitTogether
    {
        public string StudentName;
        public string TargetSeatStudentName;
    }
    internal struct ConflictDescriptorStandUp
    {
        public string StudentName;
    }

    internal struct ConflictDescriptorHyperstimulation
    {
        public string StudentName;
    }
    internal struct ConflictDescriptorFrustration
    {
        public string StudentName;
        public Vector3 LookAtPoint;
    }

    internal struct ConflictDescriptorDisorganization
    {
        public string StudentName;
        [System.Obsolete("This field is not currently used but it is reserved for future implementation")]
        public byte unusedBackpackData;
    }
    internal struct ConflictDescriptorImpulsivity
    {
        public string StudentName;
        public string TargetBotherStudentName;
    }
    internal struct ConflictDescriptorInattention
    {
        public string StudentName;
    }

    // XXX: Treat this type like an union
    [StructLayout(LayoutKind.Explicit)]
    internal struct ConflictDescriptor
    {
        [FieldOffset(0)]
        public ConflictType Type;

        [FieldOffset(128)]
        public List<string> AffectedStudents;

        // XXX: [FieldOffset(sizeof(ConflictType))] was not correct because it missaligned the pointer type inside the string variable inside the variants
        [FieldOffset(16)]
        public ConflictDescriptorDisrespect Disrespect;
        [FieldOffset(16)]
        public ConflictDescriptorSitTogether SitTogether;
        [FieldOffset(16)]
        public ConflictDescriptorStandUp StandUp;

        [FieldOffset(16)]
        public ConflictDescriptorHyperstimulation Hyperstimulation;
        [FieldOffset(16)]
        public ConflictDescriptorFrustration Frustration;

        [FieldOffset(16)]
        public ConflictDescriptorDisorganization Disorganization;
        [FieldOffset(16)]
        public ConflictDescriptorImpulsivity Impulsivity;
        [FieldOffset(16)]
        public ConflictDescriptorInattention Inattention;
    }

    // TODO: Change
    internal ConflictDescriptor GenerateConflictDescriptor(ConflictType type, string studentName)
    {
        ConflictDescriptor descriptor = new ConflictDescriptor { Type = type };

        descriptor.AffectedStudents = new List<string>();

        switch (type)
        {
            case ConflictType.Disrespect:
                descriptor.Disrespect = new ConflictDescriptorDisrespect { StudentName = studentName };
                break;
            case ConflictType.SitTogether:
            {
                if (_students.Count < 3)
                {
                    descriptor.Type |= ConflictTypeNonFeasible;
                    descriptor.SitTogether = new ConflictDescriptorSitTogether { StudentName = studentName, TargetSeatStudentName = null };
                }
                else if (_students.Count == 3)
                {
                    var selected = GetStudentByName(studentName);
                    string finalStudentName;
                    if (selected == GetStudents()[1])
                    {
                        Didascalia.Utils.Log.Warning(
                            "Selected student is the middle one, which is already sitting with the other two. Selecting another student for the conflict.",
                            this
                        );
                        var otherStudent = GetStudents()[UnityEngine.Random.Range(0, 2) == 0 ? 0 : 2];
                        finalStudentName = otherStudent.Name;
                    } else
                    {
                        finalStudentName = studentName;
                    }
                    descriptor.SitTogether = new ConflictDescriptorSitTogether {
                        StudentName = finalStudentName,
                        TargetSeatStudentName = GetStudents()[1].Name
                    };
                }
                else
                {
                    var targetSeatStudentNext = GetStudentByName(studentName).NextStudent;
                    var targetSeatStudentPrevious = GetStudentByName(studentName).PreviousStudent;
                    Didascalia.Utils.Error.DebugbreakFailIf(
                        targetSeatStudentNext == null && targetSeatStudentPrevious == null,
                        "Selected student has no next or previous student to sit together with", this
                    );
                    descriptor.SitTogether = new ConflictDescriptorSitTogether {
                        StudentName = studentName,
                        TargetSeatStudentName =
                            targetSeatStudentNext != null ? targetSeatStudentNext.Name : targetSeatStudentPrevious.Name
                    };
                }

                descriptor.AffectedStudents.Add(descriptor.SitTogether.TargetSeatStudentName);
                break;
            }
            case ConflictType.StandUp:
                descriptor.StandUp = new ConflictDescriptorStandUp { StudentName = studentName };
                break;
            case ConflictType.Hyperstimulation:
                descriptor.Hyperstimulation = new ConflictDescriptorHyperstimulation { StudentName = studentName };
                break;
            case ConflictType.Frustration:
            {
                Didascalia.Utils.Log.Warning(
                    "Conflict type " + type + " is not fully implemented because it requires a look at point that has not been implemented yet.",
                    this
                );
                descriptor.Frustration = new ConflictDescriptorFrustration { StudentName = studentName, LookAtPoint = Vector3.zero };
                break;
            }
            case ConflictType.Disorganization:
                descriptor.Disorganization = new ConflictDescriptorDisorganization { StudentName = studentName, unusedBackpackData = 0xFF };
                break;
            case ConflictType.Impulsivity:
            {
                if (_students.Count < 2)
                {
                    descriptor.Type |= ConflictTypeNonFeasible;
                    descriptor.Impulsivity = new ConflictDescriptorImpulsivity { StudentName = studentName, TargetBotherStudentName = null };
                }
                else                {
                    var targetSeatStudentNext = GetStudentByName(studentName).NextStudent;
                    var targetSeatStudentPrevious = GetStudentByName(studentName).PreviousStudent;
                    Didascalia.Utils.Error.DebugbreakFailIf(
                        targetSeatStudentNext == null && targetSeatStudentPrevious == null,
                        "Selected student has no next or previous student to sit together with", this
                    );
                    descriptor.Impulsivity = new ConflictDescriptorImpulsivity {
                        StudentName = studentName,
                        TargetBotherStudentName =
                            targetSeatStudentNext != null ? targetSeatStudentNext.Name : targetSeatStudentPrevious.Name
                    };
                }

                descriptor.AffectedStudents.Add(descriptor.Impulsivity.TargetBotherStudentName);

                break;
            }
            case ConflictType.Inattention:
                descriptor.Inattention = new ConflictDescriptorInattention { StudentName = studentName };
                break;
            default:
                Didascalia.Utils.Error.DebugbreakFailMessage("Unknown conflict type", this);
                break;
        }

        return descriptor;
    }
    internal ConflictDescriptor GenerateConflictDescriptorExpect(ConflictType type, string studentName)
    {
        var descriptor = GenerateConflictDescriptor(type, studentName);
        Didascalia.Utils.Error.DebugbreakFailIf(
            (descriptor.Type & ConflictTypeNonFeasible) != 0,
            "Generated conflict descriptor is not feasible", this
        );
        return descriptor;
    }
    internal ConflictDescriptor GenerateConflictDescriptorExpectSame(ConflictType type, string studentName)
    {
        var descriptor = GenerateConflictDescriptorExpect(type, studentName);
        const string errorMessageBase = "Generated conflict descriptor does not meet same student expectation for conflict type: ";

        bool OutOfRange()
        {
            Didascalia.Utils.Error.DebugbreakFailMessage("Unknown conflict type", this);
            return false;
        }
        bool condition = type switch
        {
            ConflictType.Disrespect => descriptor.Disrespect.StudentName == studentName,
            ConflictType.SitTogether => descriptor.SitTogether.StudentName == studentName,
            ConflictType.StandUp => descriptor.StandUp.StudentName == studentName,
            _ => OutOfRange()
        };
        Didascalia.Utils.Error.DebugbreakFailIf(
            !condition,
            errorMessageBase + type, this
        );
        return descriptor;
    }

    private void HandleConflict(ConflictDescriptor descriptor)
    {
        Student student = null;
        switch (descriptor.Type)
        {
            case ConflictType.Disrespect:
                student = GetStudentByName(descriptor.Disrespect.StudentName);
                // XXX: @ChichoRD Commented out this log
                // student.Speak("Prueba de Insulto!");
                student.GetComponent<StudentBehaviour>().Yell();
                break;

            case ConflictType.SitTogether:
                student = GetStudentByName(descriptor.SitTogether.StudentName);
                student.GetComponent<StudentBehaviour>().SitNextToGivenStudentConflict(StudentManager.Instance.GetStudent(descriptor.SitTogether.TargetSeatStudentName));
                break;

            case ConflictType.StandUp:
                student = GetStudentByName(descriptor.StandUp.StudentName);
                student.GetComponent<StudentBehaviour>().StandUpConflict();
                break;
            
            case ConflictType.Hyperstimulation:
                student = GetStudentByName(descriptor.Hyperstimulation.StudentName);
                student.GetComponent<StudentBehaviour>().Hyperstimulate();
                break;
            case ConflictType.Frustration:
                student = GetStudentByName(descriptor.Frustration.StudentName);
                student.GetComponent<StudentBehaviour>().GetDistractedTEA();
                break;
            
            case ConflictType.Disorganization:
                student = GetStudentByName(descriptor.Disorganization.StudentName);
                student.GetComponent<StudentBehaviour>().GetOutMaterialWrong();
                break;
            case ConflictType.Impulsivity:
                student = GetStudentByName(descriptor.Impulsivity.StudentName);
                student.GetComponent<StudentBehaviour>().BotherOtherStudents();
                break;
            case ConflictType.Inattention:
                student = GetStudentByName(descriptor.Inattention.StudentName);
                student.GetComponent<StudentBehaviour>().DrawDistacted();
                break;

            default:
                Didascalia.Utils.Error.DebugbreakFailMessage("Unknown conflict type", this);
                break;
        }
    }

    internal enum ConflictGenerationError
    {
        None,
        MaxActiveConflictsReached,
        NotFeasible,
        AlreadyActiveConflictForStudent,
        Unimplemented
    }
    internal struct ConflictGenerationResult
    {
        public ConflictGenerationError Error;
        public ConflictDescriptor Descriptor;
        #nullable enable
        public Conflict? ConflictInstance;
        #nullable restore
    }

    #region Conflict' Safety Checks
    private bool IsConflictCapacityOk(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
    {
        if (_activeConflicts.Count == _maxActiveConflicts)
        {
            erroneousResult = new ConflictGenerationResult
            {
                Error = ConflictGenerationError.MaxActiveConflictsReached,
                Descriptor = descriptor,
                ConflictInstance = null
            };
            return false;
        }
        else
        {
            erroneousResult = default;
            return true;
        }
    }
    private bool IsConflictFeasible(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
    {
        if ((descriptor.Type & ConflictTypeNonFeasible) != 0)
        {
            erroneousResult = new ConflictGenerationResult
            {
                Error = ConflictGenerationError.NotFeasible,
                Descriptor = descriptor,
                ConflictInstance = null
            };
            return false;
        }
        else
        {
            erroneousResult = default;
            return true;
        }
    }
    private bool IsStudentConflictFree(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult, out string descriptorName)
    {
        string OutOfRange()
        {
            Didascalia.Utils.Error.DebugbreakFailMessage("Unknown conflict type", this);
            return string.Empty;
        }
        descriptorName = descriptor.Type switch
        {
            ConflictType.Disrespect => descriptor.Disrespect.StudentName,
            ConflictType.SitTogether => descriptor.SitTogether.StudentName,
            ConflictType.StandUp => descriptor.StandUp.StudentName,

            ConflictType.Hyperstimulation => descriptor.Hyperstimulation.StudentName,
            ConflictType.Frustration => descriptor.Frustration.StudentName,

            ConflictType.Disorganization => descriptor.Disorganization.StudentName,
            ConflictType.Impulsivity => descriptor.Impulsivity.StudentName,
            ConflictType.Inattention => descriptor.Inattention.StudentName,
            _ => OutOfRange()
        };

        if (_activeConflicts.ContainsKey(descriptorName))
        {
            erroneousResult = new ConflictGenerationResult
            {
                Error = ConflictGenerationError.AlreadyActiveConflictForStudent,
                Descriptor = descriptor,
                ConflictInstance = null
            };
            return false;
        }
        else
        {
            erroneousResult = default;
            return true;
        }
    }
    private bool IsConflictTypeImplemented(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
    {
        erroneousResult = new ConflictGenerationResult
        {
            Error = ConflictGenerationError.Unimplemented,
            Descriptor = descriptor,
            ConflictInstance = null
        };
        return descriptor.Type switch
        {
            ConflictType.Disrespect => false,
            ConflictType.SitTogether => true,
            ConflictType.StandUp => true,

            ConflictType.Hyperstimulation => true,
            ConflictType.Frustration => true,
            
            ConflictType.Disorganization => true,
            ConflictType.Impulsivity => true,
            ConflictType.Inattention => true,

            _ => false
        };
    }
    #endregion

    internal ConflictGenerationResult GenerateConflict(ConflictType type, string studentName)
    {
        string name = studentName;
        // si no tenemos nombre de estudiantes, buscamos uno válido
        if (string.IsNullOrEmpty(name))
        {
            name = GetStudents()[UnityEngine.Random.Range(0, _students.Count)].Name;
        } 
        // esto nos devuelve un struct con el que definimos cada uno de los campos necesarios para ejecutar el conflicto
        var descriptor = GenerateConflictDescriptor(type, name);

        // comprobar si podemos generar el conflicto o si se ha generado un error porque la capacidad de conflictos activa a la vez
        // se ha excedido
        if (!IsConflictCapacityOk(descriptor, out ConflictGenerationResult capacityErrorResult))
        {
            // Didascalia.Utils.Error.DebugbreakFailMessage(
            Didascalia.Utils.Log.Warning(
                $"Cannot generate conflict of type {type} for student {name} because the maximum number of active conflicts has been reached.\n"
                + "Conflict will not be generated.",
                this
            );
            return capacityErrorResult;
        }

        // ni idea
        if (!IsConflictFeasible(descriptor, out ConflictGenerationResult feasibilityErrorResult))
        {
            Didascalia.Utils.Log.Warning(
                $"Generated conflict of type {type} for student {name} is not feasible. Conflict will not be generated.",
                this
            );
            return feasibilityErrorResult;
        }
        // no podemos generar un conflicto si este estudiante ya tiene un conflicto activo
        else if (!IsStudentConflictFree(descriptor, out ConflictGenerationResult studentConflictErrorResult, out string descriptorName))
        {
            Didascalia.Utils.Log.Warning(
                $"Generated conflict of type {type} for student {name} cannot be generated "
                + "because there is already an active conflict for this student.\n"
                + "Conflict will not be generated.",
                this
            );
            return studentConflictErrorResult;
        }
        // TODO: remove this check when all conflict types are implemented, because it should be the responsibility of the caller to not generate unimplemented conflict types
        else if (!IsConflictTypeImplemented(descriptor, out ConflictGenerationResult unimplementedErrorResult))
        {
            Didascalia.Utils.Log.Warning(
                $"Generated conflict of type {type} for student {name} is not implemented yet. Conflict will not be generated.",
                this
            );
            return unimplementedErrorResult;
        }
        else
        {
            // instanciamos el conflicto
            Conflict conflict = Instantiate(_conflictPrefab, transform).GetComponent<Conflict>();
            // le damos nombre
            conflict.name = $"Conflict_{type}_{descriptorName}";
            conflict.SetConflictiveStudent(GetStudentByName(descriptorName));

            foreach (string stName in descriptor.AffectedStudents)
            {
                conflict.AddAffectedStudent(GetStudent(stName));
            }

            _activeConflicts.Add(descriptorName, conflict);
            
            HandleConflict(descriptor);
            return new ConflictGenerationResult
            {
                Error = ConflictGenerationError.None,
                Descriptor = descriptor,
                ConflictInstance = conflict
            };
        }
    }

    internal struct ConflictGeneration
    {
        public ConflictDescriptor Descriptor;
        public Conflict ConflictInstance;
    }

    //internal ConflictGeneration GenerateConflictExpect(ConflictType type, string studentName)
    //{
    //    var result = GenerateConflict(type, studentName);
    //    Didascalia.Utils.Error.DebugbreakFailIf(
    //        result.Error != ConflictGenerationError.None,
    //        $"Failed to generate conflict of type {type} for student {studentName} with error: {result.Error}",
    //        this
    //    );
    //    return new ConflictGeneration
    //    {
    //        Descriptor = result.Descriptor,
    //        ConflictInstance = result.ConflictInstance!
    //    };
    //}

    public void ResolveConflicts()
    {
        foreach(Conflict ct in _activeConflicts.Values)
        {
            ct.ReceivePositiveResolution();
        }

        _activeConflicts.Clear();
    }

    public void RemoveAllConflicts()
    {
        _activeConflicts.Clear();
    }

    public void RemoveConflict(Student s)
    {
        _activeConflicts.Remove(s.Name);
    }
    #endregion
}

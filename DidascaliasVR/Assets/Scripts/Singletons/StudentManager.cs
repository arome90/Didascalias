using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    public Student GetStudentExpect(string name)
    {
        Student st = GetStudent(name);
        Didascalia.Utils.Error.DebugbreakFailIf(st == null, $"No student found with name: {name}", this);
        return st;
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
            students.Add(st);
            _students.Add(st.Name, st);

            last = st;
        }

        if(students.Count > 1 && ClassManager.Instance.Settings.ClassShape == ClassSettings.Shape.Circular)
        {
            students[0].PreviousStudent = students[students.Count - 1];
            students[students.Count - 1] = students[0];
        }

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
            st = GetStudentExpect(studentName);
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

    internal ConflictDescriptor GenerateConflictDescriptor(ConflictType type, string studentName)
    {
        ConflictDescriptor descriptor = new ConflictDescriptor { Type = type };

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
                    var selected = GetStudentExpect(studentName);
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
                    var targetSeatStudentNext = GetStudentExpect(studentName).NextStudent;
                    var targetSeatStudentPrevious = GetStudentExpect(studentName).PreviousStudent;
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
                    var targetSeatStudentNext = GetStudentExpect(studentName).NextStudent;
                    var targetSeatStudentPrevious = GetStudentExpect(studentName).PreviousStudent;
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
                student = GetStudentExpect(descriptor.Disrespect.StudentName);
                // XXX: @ChichoRD Commented out this log
                // student.Speak("Prueba de Insulto!");
                student.GetComponent<StudentBehaviour>().Yell();
                break;

            case ConflictType.SitTogether:
                student = GetStudentExpect(descriptor.SitTogether.StudentName);
                student.GetComponent<StudentBehaviour>().SitTogether();
                break;

            case ConflictType.StandUp:
                student = GetStudentExpect(descriptor.StandUp.StudentName);
                student.GetComponent<StudentBehaviour>().StandUp();
                break;
            
            case ConflictType.Hyperstimulation:
                student = GetStudentExpect(descriptor.Hyperstimulation.StudentName);
                student.GetComponent<StudentBehaviour>().Hyperstimulate();
                break;
            case ConflictType.Frustration:
                student = GetStudentExpect(descriptor.Frustration.StudentName);
                student.GetComponent<StudentBehaviour>().Frustrate();
                break;
            
            case ConflictType.Disorganization:
                student = GetStudentExpect(descriptor.Disorganization.StudentName);
                student.GetComponent<StudentBehaviour>().GetDistracted();
                break;
            case ConflictType.Impulsivity:
                student = GetStudentExpect(descriptor.Impulsivity.StudentName);
                student.GetComponent<StudentBehaviour>().GetMaterialOut();
                break;
            case ConflictType.Inattention:
                student = GetStudentExpect(descriptor.Inattention.StudentName);
                student.GetComponent<StudentBehaviour>().FailToPayAttention();
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

    private bool GenerateConflictIsCapacityOk(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
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
    private bool GenerateConflictIsConflictFeasible(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
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
    private bool GenerateConflictIsStudentConflictFree(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult, out string descriptorName)
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
    private bool GenerateConflictIsTypeImplemented(in ConflictDescriptor descriptor, out ConflictGenerationResult erroneousResult)
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
    internal ConflictGenerationResult GenerateConflict(ConflictType type, string studentName)
    {
        string name = studentName;
        if (string.IsNullOrEmpty(name))
        {
            name = GetStudents()[UnityEngine.Random.Range(0, _students.Count)].Name;
        } 
        // var descriptor = GenerateConflictDescriptorExpectSame(type, name);
        var descriptor = GenerateConflictDescriptor(type, name);

        if (!GenerateConflictIsCapacityOk(descriptor, out ConflictGenerationResult capacityErrorResult))
        {
            // Didascalia.Utils.Error.DebugbreakFailMessage(
            Didascalia.Utils.Log.Warning(
                $"Cannot generate conflict of type {type} for student {name} because the maximum number of active conflicts has been reached.\n"
                + "Conflict will not be generated.",
                this
            );
            return capacityErrorResult;
        }

        if (!GenerateConflictIsConflictFeasible(descriptor, out ConflictGenerationResult feasibilityErrorResult))
        {
            Didascalia.Utils.Log.Warning(
                $"Generated conflict of type {type} for student {name} is not feasible. Conflict will not be generated.",
                this
            );
            return feasibilityErrorResult;
        }
        else if (!GenerateConflictIsStudentConflictFree(descriptor, out ConflictGenerationResult studentConflictErrorResult, out string descriptorName))
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
        else if (!GenerateConflictIsTypeImplemented(descriptor, out ConflictGenerationResult unimplementedErrorResult))
        {
            Didascalia.Utils.Log.Warning(
                $"Generated conflict of type {type} for student {name} is not implemented yet. Conflict will not be generated.",
                this
            );
            return unimplementedErrorResult;
        }
        else
        {
            Conflict conflict = Instantiate(_conflictPrefab, transform).GetComponent<Conflict>();
            conflict.name = $"Conflict_{type}_{descriptorName}";
            conflict.SetConflictiveStudent(GetStudentExpect(descriptorName));
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

    internal ConflictGeneration GenerateConflictExpect(ConflictType type, string studentName)
    {
        var result = GenerateConflict(type, studentName);
        Didascalia.Utils.Error.DebugbreakFailIf(
            result.Error != ConflictGenerationError.None,
            $"Failed to generate conflict of type {type} for student {studentName} with error: {result.Error}",
            this
        );
        return new ConflictGeneration
        {
            Descriptor = result.Descriptor,
            ConflictInstance = result.ConflictInstance!
        };
    }

    public void ResolveConflicts()
    {
        foreach(string st in _selectedStudents)
        {
            _activeConflicts[st].ReceivePositiveResolution();
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

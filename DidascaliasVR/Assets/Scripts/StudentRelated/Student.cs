using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Chico (0) o Chica (1) (de momento)
/// </summary>
public enum Gender { Boy, Girl };

public enum StudentType
{
    Participative_NonProblematic,
    NonParticipative_NonProblematic,
    Talkative,
    Problematic,

    ADHD,
    Autistic
}

public struct StudentActionContext
{
    public string stateName;
    public string stateDescription;
    public float time;
    public List<string> avaliableActions;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("- Nombre estado: " + stateName);
        sb.AppendLine("- Descripción: " + stateDescription);

        return sb.ToString();
    }
}

/// <summary>
/// Componente que representa a una estudiante
/// </summary>
public class Student : MonoBehaviour
{
    [SerializeField,
        Tooltip("Texto que muestra el nombre del estudiante al jugador")]
    TextMeshProUGUI _nameTag;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugTypeText = null;
#endif

    [HideInInspector]
    public AudioSource _audioSource = null;

    /// <summary>
    /// Estudiante que est� justo antes de nuestro estudiante
    /// Puede tomar valor 'null' si es el primero
    /// </summary>
    [HideInInspector] public Student PreviousStudent = null;
    /// <summary>
    /// Estudiante que est� justo despu�s de nuestro estudiante
    /// Puede tomar valor 'null' si es el �ltimo
    /// </summary>
    [HideInInspector] public Student NextStudent = null;

#if UNITY_EDITOR
    [SerializeField] private TextMeshProUGUI _debugSpeak = null;
    [SerializeField] private GameObject _debugSpeakCanvas = null;
#endif

    private string _studentProfile = null;

    private StudentActionContext _currentActionContext;

    private List<string> _interactionHistory = null;
    private List<StudentActionContext> _previousActionContext = null;

    // in seconds
    const double _timeToUpdateActionContext = 30;

    private bool _active = true;

    NavMeshAgent _agent;

    [SerializeField]
    StudentType _type = StudentType.NonParticipative_NonProblematic;

    public StudentType StType
    {
        get { return _type; }
        set
        {
            SetStudentType(value);
        }
    }

    public void SetStudentType(StudentType type)
    {
        _type = type;
        this.Behaviour.SetBehaviourPattern(StudentManager.Instance.GetBehaviourModifier(_type));
#if UNITY_EDITOR
        UpdateNameTag();
#endif
    }

    private void UpdateNameTag()
    {
#if UNITY_EDITOR
        _nameTag.text = _name.ToUpper();
        _debugTypeText.text = StType.ToString();
#else
        _nameTag.text = _name.ToUpper();
#endif

    }

    string _name;

    /// <summary>
    /// Nombre del estudiante.
    /// Deber�a ser �nico entre los dem�s estudiantes.
    /// Ser� el identificador de cada estudiante, lo usaremos
    /// tambi�n para refererinos a ellos mediante voz.
    /// </summary>
    public string Name { get { return _name; } 
        set 
        { 
            _name = value;
            UpdateNameTag();
        } 
    }

    private int _age;

    /// <summary>
    /// Nombre del estudiante.
    /// Deber�a ser �nico entre los dem�s estudiantes.
    /// Ser� el identificador de cada estudiante, lo usaremos
    /// tambi�n para refererinos a ellos mediante voz.
    /// </summary>
    public int Age
    {
        get { return _age; }
        set
        {
            _age = value;
        }
    }

    private Conflict _activeConflict = null;
    public Conflict ActiveConflict => _activeConflict;

    private Coroutine _currentConflictRun = null;

    public void SetConflict(Conflict conflict) => _activeConflict = conflict;
    public void RunConflict()
    {
        if (_activeConflict == null)
            Debug.LogError($"Can't run a conflict because '{Name}' doesn't have an active conflict assigned.");
        else if (_currentConflictRun != null) 
            Debug.LogError($"Can't run a conflict because '{Name}' has already an instance running.");
        else
        {
            _nameTag.color = Color.red;
            _currentConflictRun = StartCoroutine(_activeConflict.Run());
        }
    }

    public void StopConflict()
    {
        if (_activeConflict == null) 
            Debug.LogError($"Can't stop a conflict because '{Name}' doesn't have an active conflict assigned.");
        
        if (_currentConflictRun != null)
        {
            StopCoroutine(_currentConflictRun);
            _currentConflictRun = null;
            _nameTag.color = Color.white;
        }
    }

    private StudentBehaviour _behaviour = null;
    public StudentBehaviour Behaviour { get {
            if (_behaviour == null) _behaviour = GetComponent<StudentBehaviour>();
            return _behaviour; } }

    private Desk _desk = null;
    private Desk _originalDesk = null;

    /// <summary>
    /// El escritorio al que el alumno está vinculado
    /// </summary>
    public Desk Desk { get { return _desk; }
        set { _desk = value; } }

    /// <summary>
    /// El escritorio al que el alumno está vinculado que representa el sitio que el 
    /// profesor le asignó. Esta variable SÓLO cambiará cuando sea la docente
    /// la que exija el cambio de escritorio de los alumnos y no sucederá
    /// el cambio cuando sea el alumno el que decida cambiarse de sitio
    /// </summary>
    public Desk OriginalDesk
    {
        get { return _originalDesk; }
        set { _originalDesk = value; }
    }

    [Header("Parameters")]
    private Gender _gender;
    /// <summary>
    /// G�nero del estudiante.
    /// </summary>
    public Gender Gender
    {
        get { return _gender; }
        set { _gender = value; }
    }

    public void Select()
    {
        if(_nameTag.color == Color.white) _nameTag.color = Color.blue;
    }

    public void Deselect()
    {
        _nameTag.color = Color.white;
    }

    public async void Speak(string speak)
    {
        Didascalia.Utils.Log.Message(Name + " speaks: "+ speak, this);

#if UNITY_EDITOR
        if (_debugSpeak != null)
        {
            StartCoroutine(ToggleSpeakDebugCanvas());
            _debugSpeak.SetText(speak);
        }
#endif
        if (AzureTextToSpeech.Exists)
        {
            Behaviour.StartTalking();
            await AzureTextToSpeech.Instance.Speak(speak, Gender, _audioSource);
            Behaviour.StopTalking();
        }
    }

    IEnumerator ToggleSpeakDebugCanvas()
    {
        _debugSpeakCanvas.SetActive(true);
        yield return new WaitForSeconds(6.5f);
        _debugSpeakCanvas.SetActive(false);
    }

    public void SpeakDidNotUnderstand()
    {
        Speak(StudentManager.Instance.MisunderstoodResponses.PossibleResponses.Next());
    }

    public void SetProfile(string newProfile) => _studentProfile = newProfile;

    public void SetStateContext(string stateName, string stateDescription, List<string> avaliableMethods)
    {
        if (_currentActionContext.stateName != stateName && _currentActionContext.stateName != null)
        {
            if (_previousActionContext == null) _previousActionContext = new List<StudentActionContext>();
            _previousActionContext.Add(_currentActionContext);
        }

        _currentActionContext.stateName         = stateName;
        _currentActionContext.stateDescription  = stateDescription;
        _currentActionContext.time              = Time.time;
        _currentActionContext.avaliableActions  = avaliableMethods;
    }

    private async void UpdatePreviousActionContext()
    {
        while (_active)
        {
            double currentTime = Time.timeAsDouble;

            // 1. Eliminar todas las acciones que tengan más de 30 segundos
            _previousActionContext.RemoveAll(action => (currentTime - action.time) > _timeToUpdateActionContext);

            // 2. Determinar el tiempo de espera hasta el próximo ciclo
            int delayMilliseconds = 1000; // Tiempo por defecto si la lista está vacía

            if (_previousActionContext.Count > 0)
            {
                // La acción más antigua está al principio de la lista
                double oldestActionTime = _previousActionContext[0].time;
                double timeElapsed = currentTime - oldestActionTime;
                double timeRemaining = _timeToUpdateActionContext - timeElapsed;

                // Calculamos los milisegundos exactos que le quedan a la primera acción para caducar
                delayMilliseconds = Mathf.Max(500, (int)(timeRemaining * 1000));
            }

            // 3. Esperar hasta que la acción más antigua caduque o haya que volver a comprobar
            await Task.Delay(delayMilliseconds);
        }
    }

    public string GetProfile()                                      => _studentProfile;

    public List<string> GetInteractionHistory()                     => _interactionHistory;

    public StudentActionContext GetActionContext()                  => _currentActionContext;
    public List<StudentActionContext> GetPreviousActionContext()    => _previousActionContext;

    public void AddStudentInteractionContext(string interactionContext) => AddToInteractionContext(Name, interactionContext);
    public void AddTeacherInteractionContext(string teacherContext) => AddToInteractionContext("Profesor", teacherContext);
    private void AddToInteractionContext(string name, string interaction)
    {
        if (_interactionHistory == null) _interactionHistory = new List<string>();
        _interactionHistory.Add(name + ": " + interaction);
    }

    private void Start()
    {
        _currentActionContext = default;

        if (!_nameTag)
        {
            _nameTag = GetComponentInChildren<TextMeshProUGUI>();
            if (!_nameTag) Debug.LogError("No name tag found in student");
        }
        _agent = GetComponent<NavMeshAgent>();

        _audioSource = GetComponent<AudioSource>();

        Task.Run(UpdatePreviousActionContext);

        _debugSpeakCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        _active = false;
    }
}

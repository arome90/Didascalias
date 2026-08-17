using System.Collections.Generic;
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
    Problematic
}

/// <summary>
/// Componente que representa a una estudiante
/// </summary>
public class Student : MonoBehaviour
{
    [SerializeField,
        Tooltip("Texto que muestra el nombre del estudiante al jugador")]
    TextMeshProUGUI _nameTag;

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

    private string _context = null;
    private string _ctxStateName = null;
    private string _ctxStateDescription = null;

    private List<string> _interactionHistory = null;

    NavMeshAgent _agent;

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
            _nameTag.text = _name.ToUpper();
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
    [SerializeField, Tooltip("Student's gender")]
    private Gender _gender;
    /// <summary>
    /// G�nero del estudiante.
    /// </summary>
    public Gender Gender
    {
        get { return _gender; }
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
        // if (AzureTextToSpeech.Instance != null) await AzureTextToSpeech.Instance.Speak(speak, Gender, _audioSource);
    }

    public void SetAsConflictive()
    {
        _nameTag.color = Color.red;
    }

    public void SetContext(string newContext)
    {
        _context = newContext;
    }

    public void SetStateContext(string stateName, string stateDescription)
    {
        _ctxStateName = stateName;
        _ctxStateDescription = stateDescription;
    }

    public string GetContext()
    {
        return LLMManager.Instance.AddHistoryToContext(_context, _interactionHistory);
    }

    public void AddStudentInteractionContext(string interactionContext)
    {
        if (_interactionHistory == null) _interactionHistory = new List<string>();
        _interactionHistory.Add(Name + ": " + interactionContext);
    }

    public void AddTeacherInteractionContext(string interactionContext)
    {
        if (_interactionHistory == null) _interactionHistory = new List<string>();
        _interactionHistory.Add("Profesor: " + interactionContext);
    }

    private void Start()
    {
        if(!_nameTag)
        {
            _nameTag = GetComponentInChildren<TextMeshProUGUI>();
            if (!_nameTag) Debug.LogError("No name tag found in student");
        }
        _agent = GetComponent<NavMeshAgent>();

        _audioSource = GetComponent<AudioSource>();
    }
}

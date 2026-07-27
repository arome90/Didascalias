using TMPro;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Chico (0) o Chica (1) (de momento)
/// </summary>
public enum Gender { Boy, Girl };

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

    NavMeshAgent _agent;

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
        await AzureTextToSpeech.Instance.Speak(speak, Gender, _audioSource);
        // Didascalia.Utils.Error.DebugbreakFailUnimplemented("Speak not implemened yet.", this);
    }

    public void SetAsConflictive()
    {
        _nameTag.color = Color.red;
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

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

    /// <summary>
    /// Estudiante que está justo antes de nuestro estudiante
    /// Puede tomar valor 'null' si es el primero
    /// </summary>
    [HideInInspector] public Student PreviousStudent = null;
    /// <summary>
    /// Estudiante que está justo después de nuestro estudiante
    /// Puede tomar valor 'null' si es el último
    /// </summary>
    [HideInInspector] public Student NextStudent = null;

    NavMeshAgent _agent;

    string _name;
    /// <summary>
    /// Nombre del estudiante.
    /// Debería ser único entre los demás estudiantes.
    /// Será el identificador de cada estudiante, lo usaremos
    /// también para refererinos a ellos mediante voz.
    /// </summary>
    public string Name { get { return _name; } 
        set 
        { 
            _name = value;
            _nameTag.text = _name.ToUpper();
        } 
    }

    Gender _gender;
    /// <summary>
    /// Género del estudiante.
    /// </summary>
    public Gender Gender
    {
        get { return _gender; }
        set
        {
            _gender = value;
        }
    }

    public void Select()
    {
        if(_nameTag.color == Color.white) _nameTag.color = Color.blue;
    }

    public void Deselect()
    {
        _nameTag.color = Color.white;
    }

    public void Speak(string speak)
    {
        Debug.LogWarning("Speak not implemened yet.");
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
    }
}

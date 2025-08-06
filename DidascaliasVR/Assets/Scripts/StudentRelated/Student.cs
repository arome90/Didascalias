using TMPro;
using UnityEngine;

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

    private void Start()
    {
        if(!_nameTag)
        {
            _nameTag = GetComponentInChildren<TextMeshProUGUI>();
            if (!_nameTag) Debug.LogError("No name tag found in student");
        }
    }

}

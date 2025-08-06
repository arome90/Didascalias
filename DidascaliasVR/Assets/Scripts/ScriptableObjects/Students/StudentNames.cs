using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objeto que guarda la información de muchos nombres de estudiantes
/// Cuando se accede a la lista de nombres, siempre se accede a una copia,
/// para evitar modificar el original sin querer.
/// </summary>
[CreateAssetMenu(fileName = "StudentNames", menuName = "Scriptable Objects/StudentNames")]
public class StudentNames : ScriptableObject
{
    [SerializeField]
    List<string> _boyNames = new List<string>();
    [SerializeField]
    List<string> _girlNames = new List<string>();

    /// <summary>
    /// Devuelve una copia de la lista de nombres masculinos
    /// </summary>
    public List<string> BoyNames
    {
        get { return new List<string>(_boyNames); }
    }

    /// <summary>
    /// Devuelve una copia de la lista de nombres femeninos
    /// </summary>
    public List<string> GirlNames
    {
        get { return new List<string>(_girlNames); }
    }
}
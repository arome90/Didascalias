using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuración de un complemento, incluyendo el hueso al que se adjunta, la probabilidad y las combinaciones de mesh y material.
/// </summary>
[CreateAssetMenu(fileName = "Complement", menuName = "Character Assets/Complement", order = 5)]
public class Complement : ScriptableObject
{
    [SerializeField] private string _boneName;                // Nombre del hueso al que se adjunta el complemento
    [Range(0f, 100f)]
    [SerializeField] private float _probability;              // Probabilidad de usar este complemento
    [SerializeField] private List<CharacterProps.MeshMaterialPair> _complements; // Lista de combinaciones de mesh y material para el complemento

    public string BoneName => _boneName;
    public float Probability => _probability;
    public List<CharacterProps.MeshMaterialPair> Complements => _complements;
}

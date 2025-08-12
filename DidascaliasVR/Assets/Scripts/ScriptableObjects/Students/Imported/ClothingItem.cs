
using UnityEngine;

/// <summary>
/// Configuración de un ítem de ropa para el personaje, incluyendo el mesh y colores.
/// </summary>
[CreateAssetMenu(fileName = "Garment", menuName = "Character Assets/Garment", order = 3)]
public class Garment : ScriptableObject
{
    [SerializeField] private SkinnedMeshRenderer _cloth;      // Mesh con animación para la ropa
    [SerializeField] private Color[] _colors;                 // Colores disponibles para la ropa
    [SerializeField] private Material[] _materials;                 // Colores disponibles para la ropa


    public SkinnedMeshRenderer Cloth => _cloth;
    public Color[] Colors => _colors;
    public Material[] Materials => _materials;

}
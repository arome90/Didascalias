
using UnityEngine;

/// <summary>
/// Configuración de un ítem de ropa para el personaje, incluyendo el mesh y colores.
/// </summary>
[CreateAssetMenu(fileName = "Garment2", menuName = "Character Assets/Garment2", order = 3)]
public class Garment2 : ScriptableObject
{
    [SerializeField] private SkinnedMeshRenderer _cloth;      // Mesh con animación para la ropa
    [SerializeField] private Color[] _colors;                 // Colores disponibles para la ropa

    public SkinnedMeshRenderer Cloth => _cloth;
    public Color[] Colors => _colors;
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que almacena los activos de ropa para los personajes.
/// </summary>
[CreateAssetMenu(fileName = "CharacterClothingAssets", menuName = "Character Assets/Clothing Assets", order = 1)]
public class CharacterSkinnedMeshes2 : ScriptableObject
{
    [System.Serializable]
    public class ClothingCategory2
    {
        /// <summary>
        /// Lista de ítems de ropa para esta categoría.
        /// </summary>
        [SerializeField]
        private List<Garment2> _items;

        /// <summary>
        /// Obtiene los ítems de ropa.
        /// </summary>
        public List<Garment2> Items => _items;
    }

    [System.Serializable]
    public class CharacterBodies2
    {
        /// <summary>
        /// Cuerpos para hombres.
        /// </summary>
        [SerializeField]
        private CharacterSkin2[] _men;

        /// <summary>
        /// Cuerpos para mujeres.
        /// </summary>
        [SerializeField]
        private CharacterSkin2[] _women;

        /// <summary>
        /// Obtiene los cuerpos para hombres.
        /// </summary>
        public CharacterSkin2[] Men => _men;

        /// <summary>
        /// Obtiene los cuerpos para mujeres.
        /// </summary>
        public CharacterSkin2[] Women => _women;
    }

    /// <summary>
    /// Cuerpos de personajes.
    /// </summary>
    [SerializeField]
    private CharacterBodies2 _bodies;

    /// <summary>
    /// Categorías de ropa.
    /// </summary>
    [SerializeField]
    private List<ClothingCategory2> _categories;

    /// <summary>
    /// Obtiene los cuerpos de los personajes.
    /// </summary>
    public CharacterBodies2 Bodies => _bodies;

    /// <summary>
    /// Obtiene las categorías de ropa.
    /// </summary>
    public List<ClothingCategory2> Categories => _categories;
}

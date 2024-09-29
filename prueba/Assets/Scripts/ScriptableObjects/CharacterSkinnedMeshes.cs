using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que almacena los activos de ropa para los personajes.
/// </summary>
[CreateAssetMenu(fileName = "CharacterClothingAssets", menuName = "Character Assets/Clothing Assets", order = 1)]
public class CharacterSkinnedMeshes : ScriptableObject
{
    [System.Serializable]
    public class ClothingCategory
    {
        /// <summary>
        /// Lista de ítems de ropa para esta categoría.
        /// </summary>
        [SerializeField]
        private List<Garment> _items;

        /// <summary>
        /// Obtiene los ítems de ropa.
        /// </summary>
        public List<Garment> Items => _items;
    }

    [System.Serializable]
    public class CharacterBodies
    {
        /// <summary>
        /// Cuerpos para hombres.
        /// </summary>
        [SerializeField]
        private CharacterSkin[] _men;

        /// <summary>
        /// Cuerpos para mujeres.
        /// </summary>
        [SerializeField]
        private CharacterSkin[] _women;

        /// <summary>
        /// Obtiene los cuerpos para hombres.
        /// </summary>
        public CharacterSkin[] Men => _men;

        /// <summary>
        /// Obtiene los cuerpos para mujeres.
        /// </summary>
        public CharacterSkin[] Women => _women;
    }

    /// <summary>
    /// Cuerpos de personajes.
    /// </summary>
    [SerializeField]
    private CharacterBodies _bodies;

    /// <summary>
    /// Categorías de ropa.
    /// </summary>
    [SerializeField]
    private List<ClothingCategory> _categories;

    /// <summary>
    /// Obtiene los cuerpos de los personajes.
    /// </summary>
    public CharacterBodies Bodies => _bodies;

    /// <summary>
    /// Obtiene las categorías de ropa.
    /// </summary>
    public List<ClothingCategory> Categories => _categories;
}

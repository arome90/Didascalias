using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterClothingAssets", menuName = "Character Assets/Clothing Assets")]
public class CharacterSkinnedMeshes : ScriptableObject
{
    [System.Serializable]
    public struct ClothingItem
    {
        public string name;
        public SkinnedMeshRenderer skinnedMesh;
        public bool tienePeloLargo; // Booleana para indicar si el personaje tiene pelo largo
        public SkinnedMeshRenderer pelo; // SkinnedMeshRenderer para el pelo
        public Color[] colors;

    }

    //[System.Serializable]
    //public struct BodyItem
    //{
    //    public string name;
    //    public SkinnedMeshRenderer skinnedMesh;
    //    public bool tienePeloLargo; // Booleana para indicar si el personaje tiene pelo largo
    //    public SkinnedMeshRenderer pelo; // SkinnedMeshRenderer para el pelo
    //    //[SerializeField] Color[] colors;

    //}


    [System.Serializable]
    public class ClothingCategory
    {
        public string categoryName;
        public List<ClothingItem> items = new List<ClothingItem>();
    }
    //public BodyItem[] CuerposPersonajes;
    public List<ClothingCategory> categories = new List<ClothingCategory>();
}

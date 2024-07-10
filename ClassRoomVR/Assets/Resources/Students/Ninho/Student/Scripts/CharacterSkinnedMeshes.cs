using System.Collections.Generic;
using UnityEngine;
using static CharacterProps;

[CreateAssetMenu(fileName = "CharacterClothingAssets", menuName = "Character Assets/Clothing Assets")]
public class CharacterSkinnedMeshes : ScriptableObject
{
    [System.Serializable]
    public struct ClothingItem
    {
        public string name;
        public SkinnedMeshRenderer skinnedMesh;
        public Color[] colors;

    }

    [System.Serializable]
    public struct HeadItem
    {
        public string name;
        public SkinnedMeshRenderer skinnedMesh;
        public bool tienePeloLargo; // Booleana para indicar si el personaje tiene pelo largo
        public SkinnedMeshRenderer pelo; // SkinnedMeshRenderer para el pelo
        public Material bodyMat;
        public Material HairMat;
        public BoneAttachment headBone;

    }


    [System.Serializable]
    public class ClothingCategory
    {
        public string categoryName;
        public List<ClothingItem> items = new List<ClothingItem>();
    }
    [System.Serializable]
    public class CuerposPersonajes
    {
        public HeadItem[] MenBody;
        public HeadItem[] WomenBody;
    }
    public CuerposPersonajes Characters;
    public List<ClothingCategory> categories = new List<ClothingCategory>();
}

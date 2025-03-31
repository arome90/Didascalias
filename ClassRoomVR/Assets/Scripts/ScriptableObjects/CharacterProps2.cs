using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProps2", menuName = "Character Assets/Character Props2", order = 0)]
public class CharacterProps2 : ScriptableObject
{
    [System.Serializable]
    public struct MeshMaterialPair
    {
        public Mesh Mesh; // The mesh
        public Material Material; // Corresponding material for the mesh
        public Color[] Color;

    }

    [System.Serializable]
    public struct ComplementAttachment
    {
        public string BoneName; // The name of the bone for attachment
        [Range(0f, 100f)]
        public float Probability; // Probability for this attachment to be used
        //public List<MeshMaterialPair> complements = new List<MeshMaterialPair>(); // Meshes with their materials for this bone
        public List<MeshMaterialPair> Complements; // Meshes with their materials for this bone
    }

    [SerializeField] private List<ComplementAttachment> _boneAttachments; // Complementos para huesos

    public List<ComplementAttachment> BoneAttachments => _boneAttachments;
}

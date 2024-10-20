using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProps", menuName = "Character Assets/Character Props")]
public class CharacterProps : ScriptableObject
{
    [System.Serializable]
    public class MeshMaterialPair
    {
        public Mesh mesh; // The mesh
        public Material material; // Corresponding material for the mesh
        public Color[] color;
        public Vector3 scaleOffset;
        public Vector3 rotationOffset;
        public Vector3 positionOffset;

    }


    //public List<BoneAttachment> boneAttachments = new List<BoneAttachment>(); // Attachments for bones
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

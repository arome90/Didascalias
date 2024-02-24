using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSwappingSystem : MonoBehaviour
{
    [System.Serializable] // Esto hace que puedas ver el struct en el Inspector de Unity
    public struct MeshMaterialPair
    {
        public SkinnedMeshRenderer mesh;
        public Material material;
    }

    [Header("Player Bones")]
    public Transform[] playerBonesArray;
    public Transform rootBone;
    public Dictionary<string, Transform> playerBonesDict;

    [Header("Attach Items")]
    public List<MeshMaterialPair> itemMeshMaterialPairs; // Lista de meshes y sus materiales

    private void Start()
    {
        InitializeBoneDictionary();
        foreach (var pair in itemMeshMaterialPairs)
        {
            AttachItemToPlayer(pair.mesh, pair.material); // Pasar el SkinnedMeshRenderer y el Material de cada par
        }
    }

    public void InitializeBoneDictionary()
    {
        playerBonesDict = new Dictionary<string, Transform>();

        foreach (Transform bone in playerBonesArray)
        {
            playerBonesDict.Add(bone.name, bone);
        }
    }

    public void AttachItemToPlayer(SkinnedMeshRenderer mesh, Material material)
    {
        SkinnedMeshRenderer newMesh = Instantiate(mesh); // Instanciar una copia del SkinnedMeshRenderer
        if (material != null)
        {
            newMesh.material = material; // Asignar el material específico
        }

        Transform[] newBones = new Transform[mesh.bones.Length];
        for (int i = 0; i < mesh.bones.Length; i++)
        {
            if (playerBonesDict.ContainsKey(mesh.bones[i].name))
            {
                newBones[i] = playerBonesDict[mesh.bones[i].name];
            }
            else
            {
                Debug.LogError("Player bones dictionary does not contain bone: " + mesh.bones[i].name);
            }
        }

        newMesh.bones = newBones;
        newMesh.rootBone = rootBone;
        newMesh.transform.SetParent(rootBone.parent, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMeshGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct MeshMaterialPair
    {
        public SkinnedMeshRenderer mesh;
        public Material material;
    }

    [Header("Student Bones")]
    Transform[] playerBonesArray;
    Transform rootBone;
    Dictionary<string, Transform> playerBonesDict;
    public string[] extraBonesBody;
    public string[] extraBonesHair;

    [Header("Meshes")]
    public CharacterSkinnedMeshes characterAssets;
    public string characterMeshes;

    void Awake()
    {
        if (transform.childCount > 0)
        {
            rootBone = transform.GetChild(0);
            List<Transform> bonesList = new List<Transform>();
            bonesList.Add(rootBone);
            PopulateBonesList(rootBone, bonesList);
            playerBonesArray = bonesList.ToArray();
        }

        InitializeBoneDictionary();
    }

    private void Start()
    {
        CharacterMeshes();
    }

    void CharacterMeshes()
    {
        foreach (var category in characterAssets.categories)
        {
            if (category.categoryName == characterMeshes)
            {
                Debug.Log("Category: " + category.categoryName);
                // Elegir un índice aleatorio dentro del rango de la lista de elementos
                int randomIndex = Random.Range(0, category.items.Count);
                // Obtener el elemento en el índice aleatorio
                var item = category.items[randomIndex];
                // Spawnear el SkinnedMeshRenderer
                AttachItemToPlayer(item.skinnedMesh, null, rootBone); // No necesitamos material, pasamos null

                // Si tiene pelo largo, instanciar también el pelo
                if (item.tienePeloLargo && item.pelo != null)
                {
                    AttachItemToPlayer(item.pelo, null, rootBone);
                    AdjustExtraBonesPositionHair(item.pelo);

                }

                // Ajustar la posición de los huesos extra entre meshes
                AdjustExtraBonesPositionBody(item.skinnedMesh);
                
                return;
            }
        }

        Debug.LogWarning("No se encontró la categoría " + characterMeshes);
    }

    void AdjustExtraBonesPositionBody(SkinnedMeshRenderer mesh)
    {
        Transform[] bodyBones = new Transform[extraBonesBody.Length];

        // Encontrar los huesos del cuerpo
        for (int i = 0; i < extraBonesBody.Length; i++)
        {
            bodyBones[i] = FindBoneByName(mesh, extraBonesBody[i]);
        }

        // Ajustar la posición y rotación de los huesos del cuerpo en playerBonesArray
        foreach (Transform bone in playerBonesArray)
        {
            for (int i = 0; i < extraBonesBody.Length; i++)
            {
                if (bone.name == extraBonesBody[i])
                {
                    bone.localPosition = bodyBones[i].localPosition;
                    bone.localRotation = bodyBones[i].localRotation;
                    break;
                }
            }
        }
    }


    void AdjustExtraBonesPositionHair(SkinnedMeshRenderer mesh)
    {
        Transform[] hairBones = new Transform[3];

        // Encontrar los huesos del pelo
        for (int i = 0; i < 3; i++)
        {
            string boneName = "Bip001Hair0" + (i + 1);
            hairBones[i] = FindBoneByName(mesh, boneName);

            // Ajustar la posición y rotación de los huesos del pelo en playerBonesArray
            foreach (Transform bone in playerBonesArray)
            {

                if (bone.name == boneName && hairBones[i]!=null) 
                {
                    bone.localPosition = hairBones[i].localPosition;
                    bone.localRotation = hairBones[i].localRotation;
                    break;
                }

            }
        }
    }

    Transform FindBoneByName(SkinnedMeshRenderer mesh, string boneName)
    {
        foreach (Transform bone in mesh.bones)
        {
            if (bone.name == boneName)
            {
                return bone;
            }
        }
        return null;
    }

    private void PopulateBonesList(Transform root, List<Transform> bonesList)
    {
        foreach (Transform child in root)
        {
            bonesList.Add(child);
            PopulateBonesList(child, bonesList);
        }
    }

    private void InitializeBoneDictionary()
    {
        playerBonesDict = new Dictionary<string, Transform>();

        foreach (Transform bone in playerBonesArray)
        {
            if (!playerBonesDict.ContainsKey(bone.name))
            {
                playerBonesDict.Add(bone.name, bone);
            }
        }
    }

    public void AttachItemToPlayer(SkinnedMeshRenderer mesh, Material material, Transform targetBone)
    {
        SkinnedMeshRenderer newMesh = Instantiate(mesh, targetBone.position, Quaternion.identity);

        // Configura los huesos del nuevo mesh para que coincidan con los del jugador
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
        newMesh.rootBone = targetBone; // Ajusta el rootBone al objetivo
        newMesh.transform.SetParent(transform, false);

    }

}

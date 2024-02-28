using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothSwapingSystem : MonoBehaviour
{
    [Header("Player Bones")]
    Transform[] playerBonesArray;
    Transform rootBone;
    Dictionary<string, Transform> playerBonesDict;

    [Header("Clothing Assets")]
    public CharacterSkinnedMeshes characterSkinnedMeshes; // Referencia al Scriptable Object

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
        AttachRandomItemsFromCategoriesSkippingFirst();
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

    // Método recursivo para agregar huesos al diccionario
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

    public void AttachRandomItemsFromCategoriesSkippingFirst()
    {
        // Comenzar desde el segundo elemento en characterSkinnedMeshes.categories
        for (int i = 1; i < characterSkinnedMeshes.categories.Count; i++)
        {
            var category = characterSkinnedMeshes.categories[i];

            if (category.items.Count > 0)
            {
                // Seleccionar un ítem aleatorio de la categoría
                int randomIndex = Random.Range(0, category.items.Count);
                var selectedItem = category.items[randomIndex];

                AttachItemToPlayer(selectedItem.skinnedMesh, null); // No se pasa un material específico
            }
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

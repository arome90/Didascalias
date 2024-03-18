using ClassRoomVR;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CharacterGenerator : MonoBehaviour
{

    [Header("Student Bones")]
    Transform[] playerBonesArray;
    Transform rootBone;
    Dictionary<string, Transform> playerBonesDict;

    public string[] extraBonesBody;


    private Student student;
    [Header("Meshes")]
    public CharacterSkinnedMeshes characterAssets;

    void Awake()
    {
        if (transform.childCount > 0)
        {
            rootBone = transform.GetChild(0);
            List<Transform> bonesList = new List<Transform>
            {
                rootBone
            };
            PopulateBonesList(rootBone, bonesList);
            playerBonesArray = bonesList.ToArray();
        }

        InitializeBoneDictionary();
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

    private void PopulateBonesList(Transform root, List<Transform> bonesList)
    {
        foreach (Transform child in root)
        {
            bonesList.Add(child);
            PopulateBonesList(child, bonesList);
        }
    }

    private void Start()
    {
        student = GetComponent<Student>();
        CharacterMeshes();
    }


    void CharacterMeshes()
    {
        CharacterSkinnedMeshes.HeadItem[] bodies;
        bodies = student.GetGender() == Gender.Women ? characterAssets.Characters.WomenBody : characterAssets.Characters.MenBody; 
        // Elegir un índice aleatorio dentro del rango de la lista de elementos
        int randomIndex = Random.Range(0, bodies.Length);
        // Obtener el elemento en el índice aleatorio
        var item = bodies[randomIndex];
        // Spawnear el SkinnedMeshRenderer
        AttachItemToPlayer(item.skinnedMesh, rootBone, new Material[] { item.bodyMat, item.HairMat });

        // Si tiene pelo largo, instanciar también el pelo
        if (item.tienePeloLargo && item.pelo != null)
        {
            AttachItemToPlayer(item.pelo, rootBone,null);
            AdjustExtraBonesPositionHair(item.pelo);

        }

        // Ajustar la posición de los huesos extra entre meshes
        AdjustExtraBonesPositionBody(item.skinnedMesh);


        for (int i = 0; i < characterAssets.categories.Count; i++)
        {
            var category = characterAssets.categories[i];

            if (category.items.Count > 0)
            {
                // Seleccionar un ítem aleatorio de la categoría
                randomIndex = Random.Range(0, category.items.Count);
                var selectedItem = category.items[randomIndex];

                AttachClothToPlayer(selectedItem.skinnedMesh, category.items[randomIndex].colors); // No se pasa un material específico
            }
        }

    }

    public void AttachItemToPlayer(SkinnedMeshRenderer mesh, Transform targetBone,Material[] mat)
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
        if (mat != null)
        {
            newMesh.materials = mat;
        }
    }

    public void AttachClothToPlayer(SkinnedMeshRenderer mesh, Color[] colors)
    {
        SkinnedMeshRenderer newMesh = Instantiate(mesh); // Instanciar una copia del SkinnedMeshRenderer
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
        int c = Random.Range(0, colors.Length);
        newMesh.material.SetColor("_Color", colors[c]);

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

                if (bone.name == boneName && hairBones[i] != null)
                {
                    bone.localPosition = hairBones[i].localPosition;
                    bone.localRotation = hairBones[i].localRotation;
                    break;
                }

            }
        }
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
}

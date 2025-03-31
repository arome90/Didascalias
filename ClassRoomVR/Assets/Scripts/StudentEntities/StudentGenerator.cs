using ClassRoomVR;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generador de estudiantes que configura las mallas del personaje y los complementos.
/// </summary>
public class StudentGenerator : MonoBehaviour
{
    [Header("Propiedades del Personaje")]
    [SerializeField] private CharacterProps _characterProps;
    [SerializeField] private CharacterSkinnedMeshes _characterAssets;

    private Student2 _student;
    private Transform[] _playerBonesArray;
    private Transform _rootBone;
    private Dictionary<string, Transform> _playerBonesDict;
    [SerializeField] private string[] _extraBonesBody;

    /// <summary>
    /// Inicializa los huesos del jugador y el diccionario de huesos.
    /// </summary>
    private void Awake()
    {
        InitializePlayerBonesAndDictionary();
    }

    /// <summary>
    /// Inicializa el arreglo de huesos y el diccionario de huesos.
    /// </summary>
    private void InitializePlayerBonesAndDictionary()
    {
        if (transform.childCount > 0)
        {
            _rootBone = transform.GetChild(0);
            List<Transform> bonesList = new List<Transform> { _rootBone };
            PopulateBonesList(_rootBone, bonesList);
            _playerBonesArray = bonesList.ToArray();
            _playerBonesDict = new Dictionary<string, Transform>();

            foreach (Transform bone in _playerBonesArray)
            {
                if (!_playerBonesDict.ContainsKey(bone.name))
                {
                    _playerBonesDict.Add(bone.name, bone);
                }
            }
        }
    }

    /// <summary>
    /// Llena la lista de huesos recursivamente.
    /// </summary>
    /// <param name="root">Hueso raíz.</param>
    /// <param name="bonesList">Lista de huesos.</param>
    private void PopulateBonesList(Transform root, List<Transform> bonesList)
    {
        foreach (Transform child in root)
        {
            bonesList.Add(child);
            PopulateBonesList(child, bonesList);
        }
    }

    /// <summary>
    /// Configura las mallas del personaje y genera los accesorios.
    /// </summary>
    private void Start()
    {
        _student = GetComponent<Student2>();
        SetupCharacterMeshes();
        SpawnProps();
    }

    /// <summary>
    /// Configura las mallas del personaje y ajusta la posición de los huesos.
    /// </summary>
    private void SetupCharacterMeshes()
    {
        var bodies = _student.GetGender() == Gender.Women ? _characterAssets.Bodies.Women : _characterAssets.Bodies.Men;
        var item = bodies[Random.Range(0, bodies.Length)];

        AttachMesh(item.Body, item.BodyMat, item.HairMat, _rootBone);

        if (item.Hair != null)
        {
            AttachMesh(item.Hair, null, null, _rootBone);
            AdjustBonesPosition(item.Hair, new[] { "Bip001Hair01", "Bip001Hair02", "Bip001Hair03" });
        }

        AdjustBonesPosition(item.Body, _extraBonesBody);

        foreach (var category in _characterAssets.Categories)
        {
            if (category.Items.Count > 0)
            {
                var selectedItem = category.Items[Random.Range(0, category.Items.Count)];
                AttachClothing(selectedItem.Cloth, item.BodyMat, selectedItem.Colors);
            }
        }

        var complement = item.HeadBone.Complements[Random.Range(0, item.HeadBone.Complements.Count)];
        int colorIndex = Random.Range(0, complement.Color.Length);

        TrySpawnComplement(_rootBone, item.HeadBone, complement, colorIndex, false);
    }

    /// <summary>
    /// Adjunta una malla a un hueso específico.
    /// </summary>
    /// <param name="mesh">Malla a adjuntar.</param>
    /// <param name="bodyMat">Material del cuerpo.</param>
    /// <param name="hairMat">Material del cabello.</param>
    /// <param name="targetBone">Hueso objetivo.</param>
    private void AttachMesh(SkinnedMeshRenderer mesh, Material bodyMat, Material hairMat, Transform targetBone)
    {
        SkinnedMeshRenderer newMesh = Instantiate(mesh, targetBone.position, Quaternion.identity);
        newMesh.bones = SetupBones(mesh.bones);
        newMesh.rootBone = targetBone;
        newMesh.transform.SetParent(transform, false);

        if (bodyMat != null || hairMat != null)
        {
            newMesh.materials = new Material[] { bodyMat, hairMat };
        }
    }

    /// <summary>
    /// Adjunta ropa a un personaje.
    /// </summary>
    /// <param name="mesh">Malla de ropa.</param>
    /// <param name="bodyMat">Material del cuerpo.</param>
    /// <param name="colors">Colores disponibles.</param>
    private void AttachClothing(SkinnedMeshRenderer mesh, Material bodyMat, Color[] colors)
    {
        SkinnedMeshRenderer newMesh = Instantiate(mesh);
        newMesh.bones = SetupBones(mesh.bones);
        newMesh.rootBone = _rootBone;
        newMesh.transform.SetParent(_rootBone.parent, false);

        int colorIndex = Random.Range(0, colors.Length);
        newMesh.materials[1].SetColor("_Color", colors[colorIndex]);
        newMesh.materials = new Material[] { newMesh.materials[1], bodyMat };
    }

    /// <summary>
    /// Configura los huesos de la malla.
    /// </summary>
    /// <param name="bones">Huesos a configurar.</param>
    /// <returns>Huesos configurados.</returns>
    private Transform[] SetupBones(Transform[] bones)
    {
        Transform[] newBones = new Transform[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            _playerBonesDict.TryGetValue(bones[i].name, out newBones[i]);
        }

        return newBones;
    }

    /// <summary>
    /// Ajusta la posición de los huesos en la malla.
    /// </summary>
    /// <param name="mesh">Malla que contiene los huesos.</param>
    /// <param name="boneNames">Nombres de los huesos a ajustar.</param>
    private void AdjustBonesPosition(SkinnedMeshRenderer mesh, string[] boneNames)
    {
        foreach (string boneName in boneNames)
        {
            var targetBone = _playerBonesDict[boneName];
            var sourceBone = FindBoneByName(mesh, boneName);

            if (sourceBone != null && targetBone != null)
            {
                targetBone.localPosition = sourceBone.localPosition;
                targetBone.localRotation = sourceBone.localRotation;
            }
        }
    }

    /// <summary>
    /// Encuentra un hueso por nombre dentro de la malla.
    /// </summary>
    /// <param name="mesh">Malla que contiene los huesos.</param>
    /// <param name="boneName">Nombre del hueso.</param>
    /// <returns>Hueso encontrado o null.</returns>
    private Transform FindBoneByName(SkinnedMeshRenderer mesh, string boneName)
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

    /// <summary>
    /// Genera los accesorios del personaje.
    /// </summary>
    private void SpawnProps()
    {
        Transform firstChild = transform.GetChild(0);

        foreach (var boneAttachment in _characterProps.BoneAttachments)
        {
            bool isFoot = boneAttachment.BoneName.ToLower().Contains("foot");
            bool spawnOnRight = DetermineSpawnSide(boneAttachment, isFoot);
            int randomIndex = Random.Range(0, boneAttachment.Complements.Count);
            var complement = boneAttachment.Complements[randomIndex];
            int color = Random.Range(0, complement.Color.Length);

            SpawnComplement(firstChild, boneAttachment, complement, color, spawnOnRight, isFoot);
        }
    }

    /// <summary>
    /// Determina si el accesorio debe ser generado en el lado derecho.
    /// </summary>
    /// <param name="boneAttachment">Adjunto de hueso.</param>
    /// <param name="isFoot">Si el accesorio es un pie.</param>
    /// <returns>Verdadero si debe ser generado en el lado derecho.</returns>
    private bool DetermineSpawnSide(CharacterProps.ComplementAttachment boneAttachment, bool isFoot)
    {
        if (boneAttachment.BoneName.Contains("R") && !isFoot)
        {
            return Random.Range(0, 2) == 0;
        }
        return true;
    }

    /// <summary>
    /// Genera un complemento en el hueso especificado.
    /// </summary>
    /// <param name="rootBone">Hueso raíz.</param>
    /// <param name="boneAttachment">Adjunto de hueso.</param>
    /// <param name="complement">Complemento a generar.</param>
    /// <param name="color">Índice de color.</param>
    /// <param name="spawnOnRight">Si el complemento debe generarse en el lado derecho.</param>
    /// <param name="isFoot">Si el complemento es un pie.</param>
    private void SpawnComplement(Transform rootBone, CharacterProps.ComplementAttachment boneAttachment, CharacterProps.MeshMaterialPair complement, int color, bool spawnOnRight, bool isFoot)
    {
        if (spawnOnRight || isFoot)
        {
            TrySpawnComplement(rootBone, boneAttachment, complement, color, false);
        }
        if (!spawnOnRight || isFoot)
        {
            TrySpawnComplement(rootBone, boneAttachment, complement, color, true);
        }
    }

    /// <summary>
    /// Intenta generar un complemento para el hueso especificado.
    /// </summary>
    /// <param name="rootBone">Hueso raíz.</param>
    /// <param name="boneAttachment">Adjunto de hueso.</param>
    /// <param name="complement">Complemento a generar.</param>
    /// <param name="color">Índice de color.</param>
    /// <param name="isMirrored">Si el complemento debe ser reflejado.</param>
    public static void TrySpawnComplement(Transform rootBone, CharacterProps.ComplementAttachment boneAttachment, CharacterProps.MeshMaterialPair complement, int color, bool isMirrored)
    {
        if (Random.Range(0f, 100f) <= boneAttachment.Probability)
        {
            string boneName = isMirrored && boneAttachment.BoneName.Contains("R")
                ? boneAttachment.BoneName.Replace("R", "L")
                : boneAttachment.BoneName;

            SpawnForBone(rootBone, boneName, complement, color, isMirrored);
        }
    }

    /// <summary>
    /// Genera un objeto en el hueso especificado.
    /// </summary>
    /// <param name="rootBone">Hueso raíz.</param>
    /// <param name="boneName">Nombre del hueso.</param>
    /// <param name="complement">Complemento a generar.</param>
    /// <param name="color">Índice de color.</param>
    /// <param name="isMirrored">Si el objeto debe ser reflejado.</param>
    public static void SpawnForBone(Transform rootBone, string boneName, CharacterProps.MeshMaterialPair complement, int color, bool isMirrored)
    {
        Transform bone = FindBoneInChildren(rootBone, boneName);
        if (bone == null)
        {
            Debug.LogWarning("Hueso no encontrado para el complemento '" + boneName + "'");
            return;
        }

        GameObject propObject = new GameObject(boneName + "_Prop");
        MeshFilter meshFilter = propObject.AddComponent<MeshFilter>();
        meshFilter.mesh = complement.Mesh;

        MeshRenderer meshRenderer = propObject.AddComponent<MeshRenderer>();
        meshRenderer.material = complement.Material;
        if (complement.Color.Length > 0)
        {
            meshRenderer.material.SetColor("_BaseColor", complement.Color[color]);
        }

        propObject.transform.SetParent(bone, false);
        if (isMirrored)
        {
            propObject.transform.localScale = new Vector3(
                propObject.transform.localScale.x,
                propObject.transform.localScale.y,
                -propObject.transform.localScale.z
            );
        }
    }

    /// <summary>
    /// Encuentra un hueso en los hijos del hueso padre.
    /// </summary>
    /// <param name="parent">Hueso padre.</param>
    /// <param name="boneName">Nombre del hueso.</param>
    /// <returns>Hueso encontrado o null.</returns>
    private static Transform FindBoneInChildren(Transform parent, string boneName)
    {
        Transform bone = parent.Find(boneName);
        if (bone != null) return bone;

        foreach (Transform child in parent)
        {
            bone = FindBoneInChildren(child, boneName);
            if (bone != null) return bone;
        }

        return null;
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema encargado de cambiar la ropa del personaje al azar.
/// </summary>
public class ClothSwappingSystem : MonoBehaviour
{
    [Header("Huesos del Jugador")]
    private Transform[] _playerBonesArray;
    private Transform _rootBone;
    private Dictionary<string, Transform> _playerBonesDict;

    [Header("Activos de Ropa")]
    [SerializeField] private CharacterSkinnedMeshes _characterSkinnedMeshes;

    private void Awake()
    {
        // Inicializa el hueso raíz y el array de huesos del jugador
        if (transform.childCount > 0)
        {
            _rootBone = transform.GetChild(0);
            _playerBonesArray = GetPlayerBonesArray(_rootBone);
        }

        InitializeBoneDictionary();
    }

    private void Start()
    {
        AttachRandomItemsFromCategories();
    }

    /// <summary>
    /// Obtiene un array de huesos del jugador a partir del hueso raíz.
    /// </summary>
    /// <param name="root">Hueso raíz del jugador.</param>
    /// <returns>Array de huesos del jugador.</returns>
    private Transform[] GetPlayerBonesArray(Transform root)
    {
        var bonesList = new List<Transform> { root };
        PopulateBonesList(root, bonesList);
        return bonesList.ToArray();
    }

    /// <summary>
    /// Llena la lista de huesos recursivamente.
    /// </summary>
    /// <param name="root">Hueso raíz para iniciar la búsqueda.</param>
    /// <param name="bonesList">Lista de huesos a llenar.</param>
    private void PopulateBonesList(Transform root, List<Transform> bonesList)
    {
        foreach (Transform child in root)
        {
            bonesList.Add(child);
            PopulateBonesList(child, bonesList);
        }
    }

    /// <summary>
    /// Inicializa el diccionario de huesos del jugador.
    /// </summary>
    private void InitializeBoneDictionary()
    {
        _playerBonesDict = new Dictionary<string, Transform>();

        foreach (Transform bone in _playerBonesArray)
        {
            _playerBonesDict[bone.name] = bone;
        }
    }

    /// <summary>
    /// Adjunta elementos aleatorios de las categorías al jugador.
    /// </summary>
    private void AttachRandomItemsFromCategories()
    {
        foreach (var category in _characterSkinnedMeshes.Categories)
        {
            if (category.Items.Count > 0)
            {
                var selectedItem = category.Items[Random.Range(0, category.Items.Count)];
                AttachItemToPlayer(selectedItem.Cloth, selectedItem.Colors);
            }
        }
    }

    /// <summary>
    /// Adjunta un ítem al jugador con un color aleatorio.
    /// </summary>
    /// <param name="mesh">Mesh del ítem a adjuntar.</param>
    /// <param name="colors">Colores disponibles para el ítem.</param>
    private void AttachItemToPlayer(SkinnedMeshRenderer mesh, Color[] colors)
    {
        var newMesh = Instantiate(mesh);
        var newBones = GetUpdatedBones(mesh.bones);

        newMesh.bones = newBones;
        newMesh.rootBone = _rootBone;
        newMesh.transform.SetParent(_rootBone.parent, false);

        ApplyRandomColor(newMesh.material, colors);
    }

    /// <summary>
    /// Obtiene los huesos actualizados para el nuevo mesh.
    /// </summary>
    /// <param name="originalBones">Huesos originales del mesh.</param>
    /// <returns>Array de huesos actualizados.</returns>
    private Transform[] GetUpdatedBones(Transform[] originalBones)
    {
        var newBones = new Transform[originalBones.Length];

        for (int i = 0; i < originalBones.Length; i++)
        {
            if (!_playerBonesDict.TryGetValue(originalBones[i].name, out newBones[i]))
            {
                Debug.LogError($"El diccionario de huesos del jugador no contiene el hueso: {originalBones[i].name}");
            }
        }

        return newBones;
    }

    /// <summary>
    /// Aplica un color aleatorio al material.
    /// </summary>
    /// <param name="material">Material al que se le aplicará el color.</param>
    /// <param name="colors">Colores disponibles para aplicar.</param>
    private void ApplyRandomColor(Material material, Color[] colors)
    {
        if (colors.Length > 0)
        {
            material.SetColor("_Color", colors[Random.Range(0, colors.Length)]);
        }
    }
}

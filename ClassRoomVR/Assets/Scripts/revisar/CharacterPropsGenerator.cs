using UnityEngine;

/// <summary>
/// Encargado de generar y asignar complementos a los huesos del personaje.
/// </summary>
public class CharacterPropsSpawner : MonoBehaviour
{
    [SerializeField] private CharacterProps _characterProps; // Propiedades de los complementos del personaje

    private void Start()
    {
        SpawnProps();
    }

    /// <summary>
    /// Genera los complementos para el personaje basándose en las propiedades definidas.
    /// </summary>
    private void SpawnProps()
    {
        Transform rootBone = transform.GetChild(0);

        foreach (var boneAttachment in _characterProps.BoneAttachments)
        {
            bool isFoot = boneAttachment.BoneName.ToLower().Contains("foot");
            bool spawnOnRight = DetermineSpawnSide(boneAttachment, isFoot);
            int randomIndex = Random.Range(0, boneAttachment.Complements.Count);
            var complement = boneAttachment.Complements[randomIndex];
            int colorIndex = Random.Range(0, complement.color.Length);

            SpawnComplement(rootBone, boneAttachment, complement, colorIndex, spawnOnRight, isFoot);
        }
    }

    /// <summary>
    /// Determina si el complemento debe aparecer en el lado derecho o izquierdo.
    /// </summary>
    /// <param name="boneAttachment">Adjunto de hueso con propiedades del complemento.</param>
    /// <param name="isFoot">Indica si el hueso es para el pie.</param>
    /// <returns>Devuelve verdadero si debe aparecer en el lado derecho.</returns>
    private bool DetermineSpawnSide(CharacterProps.ComplementAttachment boneAttachment, bool isFoot)
    {
        if (boneAttachment.BoneName.Contains("R") && !isFoot)
        {
            return Random.Range(0, 2) == 0;
        }
        return true;
    }

    /// <summary>
    /// Genera un complemento para un hueso específico.
    /// </summary>
    /// <param name="rootBone">Hueso raíz donde se debe adjuntar el complemento.</param>
    /// <param name="boneAttachment">Adjunto de hueso con propiedades del complemento.</param>
    /// <param name="complement">Información del complemento.</param>
    /// <param name="colorIndex">Índice del color para el complemento.</param>
    /// <param name="spawnOnRight">Indica si el complemento debe aparecer en el lado derecho.</param>
    /// <param name="isFoot">Indica si el hueso es para el pie.</param>
    private void SpawnComplement(Transform rootBone, CharacterProps.ComplementAttachment boneAttachment, CharacterProps.MeshMaterialPair complement, int colorIndex, bool spawnOnRight, bool isFoot)
    {
        if (spawnOnRight || isFoot)
        {
            TrySpawnComplement(rootBone, boneAttachment, complement, colorIndex, false);
        }
        if (!spawnOnRight || isFoot)
        {
            TrySpawnComplement(rootBone, boneAttachment, complement, colorIndex, true);
        }
    }

    /// <summary>
    /// Intenta generar un complemento en un hueso específico con posibilidad de espejado.
    /// </summary>
    /// <param name="rootBone">Hueso raíz donde se debe adjuntar el complemento.</param>
    /// <param name="boneAttachment">Adjunto de hueso con propiedades del complemento.</param>
    /// <param name="complement">Información del complemento.</param>
    /// <param name="colorIndex">Índice del color para el complemento.</param>
    /// <param name="isMirrored">Indica si el complemento debe ser espejado.</param>
    public static void TrySpawnComplement(Transform rootBone, CharacterProps.ComplementAttachment boneAttachment, CharacterProps.MeshMaterialPair complement, int colorIndex, bool isMirrored)
    {
        if (Random.Range(0f, 100f) <= boneAttachment.Probability)
        {
            string boneName = isMirrored && boneAttachment.BoneName.Contains("R")
                ? boneAttachment.BoneName.Replace("R", "L")
                : boneAttachment.BoneName;

            SpawnForBone(rootBone, boneName, complement, colorIndex, isMirrored);
        }
    }

    /// <summary>
    /// Genera un objeto para un hueso específico y lo configura.
    /// </summary>
    /// <param name="rootBone">Hueso raíz donde se debe adjuntar el complemento.</param>
    /// <param name="boneName">Nombre del hueso al que se debe adjuntar el complemento.</param>
    /// <param name="complement">Información del complemento.</param>
    /// <param name="colorIndex">Índice del color para el complemento.</param>
    /// <param name="isMirrored">Indica si el complemento debe ser espejado.</param>
    private static void SpawnForBone(Transform rootBone, string boneName, CharacterProps.MeshMaterialPair complement, int colorIndex, bool isMirrored)
    {
        Transform bone = FindBoneInChildren(rootBone, boneName);
        if (bone == null)
        {
            Debug.LogWarning($"Hueso '{boneName}' no encontrado para el complemento.");
            return;
        }

        GameObject propObject = new GameObject($"{boneName}_Prop");
        MeshFilter meshFilter = propObject.AddComponent<MeshFilter>();
        meshFilter.mesh = complement.mesh;

        MeshRenderer meshRenderer = propObject.AddComponent<MeshRenderer>();
        meshRenderer.material = complement.material;
        if (complement.material.HasProperty("_BaseColor"))
        {
            meshRenderer.material.SetColor("_BaseColor", complement.material.GetColor("_BaseColor"));
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
    /// Busca un hueso en los hijos del hueso raíz.
    /// </summary>
    /// <param name="parent">Transform raíz para buscar.</param>
    /// <param name="boneName">Nombre del hueso a buscar.</param>
    /// <returns>Transform del hueso encontrado o null si no se encuentra.</returns>
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

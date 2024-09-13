using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maps the bones of the current SkinnedMeshRenderer to the bones of the target SkinnedMeshRenderer.
/// </summary>
public class Equipmentizer : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer _targetMeshRenderer;

    private void Start()
    {
        if (_targetMeshRenderer == null)
        {
            Debug.LogError("TargetMeshRenderer is not assigned.");
            return;
        }

        Dictionary<string, Transform> boneMap = CreateBoneMap(_targetMeshRenderer);
        SkinnedMeshRenderer currentRenderer = GetComponent<SkinnedMeshRenderer>();

        if (currentRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component is missing.");
            return;
        }

        Transform[] mappedBones = MapBonesToTargetSkeleton(currentRenderer.bones, boneMap);
        currentRenderer.bones = mappedBones;
    }

    /// <summary>
    /// Creates a dictionary mapping bone names to their corresponding Transforms from the target SkinnedMeshRenderer.
    /// </summary>
    /// <param name="renderer">The SkinnedMeshRenderer containing the bones to map.</param>
    /// <returns>A dictionary mapping bone names to Transforms.</returns>
    private Dictionary<string, Transform> CreateBoneMap(SkinnedMeshRenderer renderer)
    {
        var boneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in renderer.bones)
        {
            boneMap[bone.gameObject.name] = bone;
        }
        return boneMap;
    }

    /// <summary>
    /// Maps the bones of the current SkinnedMeshRenderer to the target bones based on the provided bone map.
    /// </summary>
    /// <param name="currentBones">The current bones of the SkinnedMeshRenderer.</param>
    /// <param name="boneMap">The dictionary mapping bone names to Transforms of the target skeleton.</param>
    /// <returns>An array of Transforms mapped to the target skeleton.</returns>
    private Transform[] MapBonesToTargetSkeleton(Transform[] currentBones, Dictionary<string, Transform> boneMap)
    {
        Transform[] newBones = new Transform[currentBones.Length];
        for (int i = 0; i < currentBones.Length; i++)
        {
            GameObject bone = currentBones[i].gameObject;
            if (!boneMap.TryGetValue(bone.name, out Transform mappedBone))
            {
                Debug.LogWarning($"Unable to map bone \"{bone.name}\" to target skeleton.");
                newBones[i] = null;  // Optionally handle unmapped bones
            }
            else
            {
                newBones[i] = mappedBone;
            }
        }
        return newBones;
    }
}

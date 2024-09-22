using UnityEngine;
using Utilities.Extensions;

public class CharacterPropsSpawner : MonoBehaviour
{
    public CharacterProps characterProps; // Reference to the CharacterProps scriptable object

    void Start()
    {
        SpawnProps();
    }

    void SpawnProps()
    {
        Transform firstChild = transform.GetChild(0); // Get the first child of the script owner

        // Iterate through each bone attachment
        foreach (var boneAttachment in characterProps.boneAttachments)
        {

            bool isFoot = boneAttachment.boneName.ToLower().Contains("foot");
            // Decide if the asset should spawn only on one side (not both) for non-foot bones
            bool spawnOnOneSideOnly = boneAttachment.boneName.Contains("r") && !isFoot;

            // Randomly decide which side to spawn the asset on if spawnOnOneSideOnly is true
            bool spawnOnRight = spawnOnOneSideOnly ? Random.Range(0, 2) == 0 : true;
            int randomIndex = Random.Range(0, boneAttachment.complements.Count);
            var complement = boneAttachment.complements[randomIndex];
            int color = Random.Range(0, complement.color.Length);
            if (spawnOnRight || isFoot)
            {
                // Use the probability to determine if a mesh should be spawned for this bone attachment
                TrySpawnComplement(firstChild, boneAttachment, complement, color, false);
            }
            if (!spawnOnRight || isFoot)
            {
                // Try to spawn on the left side (mirror) if it's either a foot or chosen by probability
                TrySpawnComplement(firstChild, boneAttachment, complement, color, true);
            }

        }
    }

    static public void TrySpawnComplement(Transform rootBone, CharacterProps.BoneAttachment boneAttachment, CharacterProps.MeshMaterialPair complement, int color, bool isMirrored)
    {
        float probabilityRoll = Random.Range(0f, 100f);
        if (probabilityRoll <= boneAttachment.probability)
        {
            // Select a random MeshMaterialPair from the complements list
            string boneName = boneAttachment.boneName;

            // Adjust bone name for mirrored assets
            if (isMirrored && boneName.Contains("r"))
            {
                boneName = boneName.Replace("r", "l");
            }

            SpawnForBone(rootBone, boneName, complement, color, isMirrored);
        }
    }

    static public void SpawnForBone(Transform rootBone, string boneName, CharacterProps.MeshMaterialPair complement, int color, bool isMirrored)
    {
        // Search for the bone with the specified name
        Transform bone = FindBoneInChildren(rootBone, boneName);
        if (bone != null)
        {
            // Instantiate the prop with the selected MeshMaterialPair
            GameObject propObject = new GameObject(boneName + "_Prop");
            MeshFilter meshFilter = propObject.AddComponent<MeshFilter>();
            meshFilter.mesh = complement.mesh;

            MeshRenderer meshRenderer = propObject.AddComponent<MeshRenderer>();
            meshRenderer.material = complement.material;

            if (complement.color.Length > 0)
            {
                meshRenderer.material.SetColor("_BaseColor", complement.color[color]);
            }
            // Set the prop object as a child of the bone
            propObject.transform.SetParent(bone, false);

            if (complement.scaleOffset.x != 0 && complement.scaleOffset.y != 0 && complement.scaleOffset.z != 0)
                propObject.transform.localScale
                    = new Vector3(propObject.transform.localScale.x * complement.scaleOffset.x, propObject.transform.localScale.y * complement.scaleOffset.y, propObject.transform.localScale.z * complement.scaleOffset.z);

            propObject.transform.Rotate(complement.rotationOffset);

          
            if (isMirrored)
            {
                // If the object is on the "L" side, mirror it by scaling in Z-axis
                propObject.transform.localScale = new Vector3(propObject.transform.localScale.x, propObject.transform.localScale.y, -propObject.transform.localScale.z);
                propObject.transform.Translate(new Vector3(-complement.positionOffset.x, complement.positionOffset.y, complement.positionOffset.z), Space.World);

            }
            else propObject.transform.Translate(complement.positionOffset, Space.World);
            // Debug.Log("Spawned mesh for bone '" + boneName + "' with mirrored: " + isMirrored);
        }
        else
        {
            Debug.LogWarning("Bone not found for prop '" + boneName + "'");
        }
    }

    static Transform FindBoneInChildren(Transform parent, string boneName)
    {
        Transform bone = parent.Find(boneName); // Try to find the bone directly

        if (bone != null)
            return bone;

        // If the bone is not found directly, search recursively in child objects
        foreach (Transform child in parent)
        {
            bone = FindBoneInChildren(child, boneName);
            if (bone != null)
                return bone;
        }

        return null; // Bone not found in children
    }
}

using UnityEngine;

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

        foreach (var propSection in characterProps.propSections)
        {
            // Instantiate the prop
            GameObject propObject = new GameObject(propSection.propName);
            propObject.AddComponent<MeshFilter>().mesh = propSection.propMesh;
            propObject.AddComponent<MeshRenderer>();
            // Set material or other properties as needed

            // Search for the bone with the specified name
            Transform bone = FindBoneInChildren(firstChild, propSection.propName); // Use propSection.propName to search for the bone
            if (bone != null)
            {
                // Set the prop object as a child of the bone
                propObject.transform.SetParent(bone);
                propObject.transform.localPosition = Vector3.zero;
                propObject.transform.localRotation = Quaternion.identity;
                Debug.Log("Found bone for prop '" + propSection.propName + "': " + bone.name);
            }
            else
            {
                Debug.Log("Bone not found for prop '" + propSection.propName + "': " + propSection.propName);
            }
        }
    }

    Transform FindBoneInChildren(Transform parent, string boneName)
    {
        Transform bone = parent.Find(boneName); // Try to find the bone directly

        if (bone != null)
            return bone;

        // If bone not found directly, search recursively in child objects
        foreach (Transform child in parent)
        {
            bone = FindBoneInChildren(child, boneName);
            if (bone != null)
                return bone;
        }

        return null; // Bone not found in children
    }
}

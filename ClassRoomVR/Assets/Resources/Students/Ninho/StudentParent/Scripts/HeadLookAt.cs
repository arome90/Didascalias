using UnityEngine;
using UnityEngine.Animations;

public class ApplyAimConstraint : MonoBehaviour
{
    public string targetObjectName; // The name of the target object

    [Range(0f, 1f)] 
    public float constraintWeight = 1f;

    private AimConstraint aimConstraint;
    private ConstraintSource source;

    void Start()
    {
        // Attempt to find the target object in the scene
        GameObject targetObject = GameObject.Find(targetObjectName);

        if (targetObject != null)
        {
            // Ensure this GameObject has an AimConstraint component
            aimConstraint = gameObject.GetComponent<AimConstraint>();
            if (aimConstraint == null)
            {
                aimConstraint = gameObject.AddComponent<AimConstraint>();
            }

            // Set up the constraint source
            source.sourceTransform = targetObject.transform;
            source.weight = constraintWeight; // Use the specified weight
            aimConstraint.AddSource(source);
            aimConstraint.constraintActive = true;

            // Optionally configure more properties of aimConstraint here
        }
        else
        {
            Debug.LogError("Target object not found. Please make sure the targetObjectName is correctly spelled and present in the scene.");
        }
    }
    void Update()
    {
            // Directly modify the weight of the Aim Constraint's source
            source.weight = constraintWeight;
            aimConstraint.weight = constraintWeight; // Assumes the source of interest is at index 0
    }
}

using UnityEngine;

public class ClassMaterial : MonoBehaviour
{
    private void Start()
    {
        ClassManager.Instance.SetClassMaterial(this);
    }

    private void OnDestroy()
    {
        ClassManager.Instance.SetClassMaterial(null);
    }
}

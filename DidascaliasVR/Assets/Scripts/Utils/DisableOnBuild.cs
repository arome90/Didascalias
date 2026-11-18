using UnityEngine;

public class DisableOnBuild : MonoBehaviour
{
#if !UNITY_EDITOR
    private void Awake()
    {
        Destroy(gameObject);
    }
#endif
}
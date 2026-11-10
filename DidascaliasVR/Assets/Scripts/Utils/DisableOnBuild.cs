using UnityEngine;

public class DisableOnBuild : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}

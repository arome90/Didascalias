using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Destruye el objeto tras unos segundos
/// y hace que el componente imagen asociado al objeto
/// haga un fade durante esos segundos
/// </summary>
public class FadeAndDestroyAfterSeconds : MonoBehaviour
{
    [SerializeField]
    float _seconds = 5f;

    private void Start()
    {
        Destroy(gameObject, _seconds);
        GetComponent<Image>().CrossFadeAlpha(0, _seconds, false);
    }
}

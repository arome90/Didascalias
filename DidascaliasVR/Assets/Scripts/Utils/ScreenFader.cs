using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Componente que nos permite mostrar una imagen en pantalla para hacer
/// fade-in y fade-out
/// </summary>
public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField,
        Tooltip("Imagen a la que modificaremos la transparencia para hacer el fade")]
    Image _fadeImage;

    [Header("Parameters")]
    [SerializeField,
        Tooltip("Si comenzamos con la aplicación con un fade-in o no")]
    bool _fadeOnStart = true;
    [SerializeField,
        Tooltip("Tiempo que se tarda en hacer fade-in o fade-out")]
    float _fadeTime = 1.0f;

    /// <summary>
    /// Tiempo que se tarda en hacer fade-in o fade-out
    /// </summary>
    public float FadeTime { get { return _fadeTime; } set { _fadeTime = value; } }

    // eventos

    /// <summary>
    /// Evento invocado al final de un FadeIn
    /// </summary>
    private UnityEvent _onFadeIn;
    /// <summary>
    /// Evento invocado al final de un FadeOut
    /// </summary>
    private UnityEvent _onFadeOut;

    private void Start()
    {
        _onFadeIn = new UnityEvent();
        _onFadeOut = new UnityEvent();

        if (_fadeImage == null)
        {
            _fadeImage = GetComponentInChildren<Image>();
            if(_fadeImage == null)
            {
                Debug.LogError("Fade Image was not found in " + gameObject.name);
            }
            else
            {
                Debug.LogWarning("Fade Image was found in " + _fadeImage.name +"\nIs it a correct image?");
            }
        }
        if (_fadeOnStart)
        {
            FadeIn();
        }
    }

    /// <summary>
    /// Modifica el valor alfa de la imagen asociada al componente de 1 a 0 durante el valor
    /// FadeTime de este mismo componente
    /// Además, ejecuta la acción enviada como parámetro una vez termine la acción (cuando
    /// la imagen llegue al valor alpha 0.0f)
    /// </summary>
    /// <param name="action"> Acción a ejecutar cuando termine el fade-in </param>
    public void FadeIn(UnityAction action = null)
    {
        if (action != null) _onFadeIn.AddListener(action);
        StartCoroutine(FadeForTime(_fadeTime, 1.0f, 0.0f));
    }

    /// <summary>
    /// Modifica el valor alfa de la imagen asociada al componente de 0 a 1 durante el valor
    /// FadeTime de este mismo componente
    /// Además, ejecuta la acción enviada como parámetro una vez termine la acción (cuando
    /// la imagen llegue al valor alpha 1.0f)
    /// </summary>
    /// <param name="action"> Acción a ejecutar cuando termine el fade-out </param>
    public void FadeOut(UnityAction action = null)
    {
        if (action != null) _onFadeOut.AddListener(action);
        StartCoroutine(FadeForTime(_fadeTime, 0.0f, 1.0f));
    }
    
    /// <summary>
    /// Corrutina que cambia el valor alpha de la imagen de start a end y tarda fadeTime segundos
    /// en hacerlo
    /// </summary>
    /// <param name="fadeTime"> Tiempo en segundos que toma la acción </param>
    /// <param name="start"> Valor inicial del canal alpha de la imagen </param>
    /// <param name="end"> Valor objetivo del canal alpha de la imagen </param>
    /// <returns></returns>
    IEnumerator FadeForTime(float fadeTime, float start, float end)
    {
        // El color actual, pero con el Alpha al máximo
        Color currentColor = new Color(_fadeImage.color.r,
            _fadeImage.color.g, _fadeImage.color.b, start);
        _fadeImage.color = currentColor;

        float currentTime = 0.0f;
        while(_fadeImage.color.a != end)
        {
            currentColor = new Color(currentColor.r, currentColor.g, currentColor.b, 
                Mathf.Lerp(start, end, currentTime / fadeTime));
            _fadeImage.color = currentColor;
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        }

        if(start > end)
        {
            _onFadeIn.Invoke();
            _onFadeIn.RemoveAllListeners();
        }
        else
        {
            _onFadeOut.Invoke();
            _onFadeOut.RemoveAllListeners();
        }
    }
}

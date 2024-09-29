using System.Collections;
using UnityEngine;

/// <summary>
/// Manages the fading in and out of a screen effect using a Renderer component.
/// </summary>
public class FadeScreen : MonoBehaviour
{
    [SerializeField] private bool _fadeOnStart = true;
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private Color _fadeColor = Color.black;
    [SerializeField] private AnimationCurve _fadeCurve;
    [SerializeField] private string _colorPropertyName = "_Color";

    public float FadeDuration => _fadeDuration;

    private Renderer _renderer;

    /// <summary>
    /// Initializes the Renderer component and starts the fade-in if specified.
    /// </summary>
    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        _renderer.enabled = false;

        if (_fadeOnStart)
        {
            FadeIn();
        }
    }

    /// <summary>
    /// Initiates the fade-in effect.
    /// </summary>
    public void FadeIn()
    {
        Fade(1f, 0f);
    }

    /// <summary>
    /// Initiates the fade-out effect.
    /// </summary>
    public void FadeOut()
    {
        Fade(0f, 1f);
    }

    /// <summary>
    /// Starts the fading effect from one alpha value to another.
    /// </summary>
    /// <param name="alphaIn">Starting alpha value.</param>
    /// <param name="alphaOut">Ending alpha value.</param>
    /// <param name="onComplete">Callback action to invoke upon completion.</param>
    public void Fade(float alphaIn, float alphaOut, System.Action onComplete = null)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut, onComplete));
    }

    /// <summary>
    /// Coroutine that handles the fading effect over time.
    /// </summary>
    /// <param name="alphaIn">Starting alpha value.</param>
    /// <param name="alphaOut">Ending alpha value.</param>
    /// <param name="onComplete">Callback action to invoke upon completion.</param>
    /// <returns>Enumerator for coroutine.</returns>
    private IEnumerator FadeRoutine(float alphaIn, float alphaOut, System.Action onComplete = null)
    {
        _renderer.enabled = true;

        float timer = 0f;
        while (timer <= _fadeDuration)
        {
            Color newColor = _fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, _fadeCurve.Evaluate(timer / _fadeDuration));

            _renderer.material.SetColor(_colorPropertyName, newColor);

            timer += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _renderer.material.SetColor(_colorPropertyName, finalColor);
        onComplete?.Invoke();

        if (alphaOut == 0f)
            _renderer.enabled = false;
        
    }
}

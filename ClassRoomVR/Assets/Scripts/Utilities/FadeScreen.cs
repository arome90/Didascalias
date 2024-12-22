using BehaviorDesigner.Runtime.Tasks.Unity.SharedVariables;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem;
using ClassRoomVR;
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

    private SpriteRenderer _renderer;

    /// <summary>
    /// Initializes the Renderer component and starts the fade-in if specified.
    /// </summary>
    private void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();

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
    {//TODO
       // Fade(0f, 1f);
    }

    /// <summary>
    /// Starts the fading effect from one alpha value to another.
    /// </summary>
    /// <param name="alphaIn">Starting alpha value.</param>
    /// <param name="alphaOut">Ending alpha value.</param>
    /// <param name="onComplete">Callback action to invoke upon completion.</param>
    public void Fade(float alphaIn, float alphaOut, System.Action onComplete = null)
    {
     
        gameObject.SetActive(true);
        if (gameObject.active)
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
        float timer = 0f;
        while (timer <= _fadeDuration)
        {
            Color newColor = _fadeColor;
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, _fadeCurve.Evaluate(timer / _fadeDuration));

            _renderer.color = newColor;

            timer += Time.deltaTime;
            yield return null;
        }

        Color finalColor = _fadeColor;
        finalColor.a = alphaOut;
        _renderer.color = finalColor;
        onComplete?.Invoke();
    }
}

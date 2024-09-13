using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor de transiciones entre escenas con efectos de desvanecimiento.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private FadeScreen _fadeScreen;

    /// <summary>
    /// Instancia singleton del gestor de transiciones.
    /// </summary>
    public static SceneTransitionManager Singleton { get; private set; }
    public FadeScreen FadeScreen => _fadeScreen;


    private void Awake()
    {
        // Asegura que solo haya una instancia del gestor de transiciones.
        if (Singleton && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Cambia a la escena especificada de manera sincrónica.
    /// </summary>
    /// <param name="sceneIndex">Índice de la escena a cargar.</param>
    public void GoToScene(int sceneIndex)
    {
        StartCoroutine(GoToSceneRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneRoutine(int sceneIndex)
    {
        if (_fadeScreen == null)
        {
            Debug.LogError("FadeScreen no está asignado.");
            yield break;
        }

        _fadeScreen.FadeOut();
        yield return new WaitForSeconds(_fadeScreen.FadeDuration);

        // Carga la nueva escena.
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Cambia a la escena especificada de manera asíncrona.
    /// </summary>
    /// <param name="sceneIndex">Índice de la escena a cargar.</param>
    public void GoToSceneAsync(int sceneIndex)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        if (_fadeScreen == null)
        {
            Debug.LogError("FadeScreen no está asignado.");
            yield break;
        }

        _fadeScreen.FadeOut();

        // Carga la nueva escena de manera asíncrona.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0;
        while (timer <= _fadeScreen.FadeDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}

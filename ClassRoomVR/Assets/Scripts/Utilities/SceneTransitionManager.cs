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
        SceneManager.sceneLoaded += OnSceneLoaded;
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("ScnTransLoad");
        if (FadeScreen == null)
        {
            //Debug.Log("ScnTransLoadNohacecosas");
            _fadeScreen = FindFirstObjectByType<FadeScreen>();
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
            //Debug.LogError("FadeScreen no está asignado.");
            yield break;
        }

        //Debug.Log(_fadeScreen);//.GameObject.SetActive(true);
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
        //Debug.Log("ScnTransGotoLlamada");
        Debug.Log(_fadeScreen);//.GameObject.SetActive(true);
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    private IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        if (_fadeScreen == null)
        {
            //Debug.LogError("FadeScreen no está asignado.");
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

        //Debug.Log("GoToSceneAsyncRoutine acabada");
        operation.allowSceneActivation = true;
    }



    public void GoToSceneAsync(string sceneName)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneName));
    }

    private IEnumerator GoToSceneAsyncRoutine(string sceneName)
    {
        if (_fadeScreen == null)
        {
            Debug.LogError("FadeScreen no está asignado.");
            yield break;
        }

        _fadeScreen.FadeOut();

        // Carga la nueva escena de manera asíncrona.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
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

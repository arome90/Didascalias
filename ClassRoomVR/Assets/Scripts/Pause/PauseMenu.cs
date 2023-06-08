using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    private Canvas pauseMenuUI;
    public Button resumeButton;
    public Button optionsButton;
    public Button quitButton;

    //private bool isPaused = false;
    [SerializeField] private InputActionProperty showButtonAction;
    [SerializeField] private InputActionProperty thinkAction;

    private void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
       // optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);
        pauseMenuUI = GetComponent<Canvas>();
        showButtonAction.action.performed += ToggleMenu;
        thinkAction.action.performed += ThinkPause;
    }


    private void OnDestroy()
    {
        showButtonAction.action.performed -= ToggleMenu;
    }

    public void ToggleMenu(InputAction.CallbackContext context)
    {
        //pauseMenuUI.enabled = !pauseMenuUI.enabled;
        if (!pauseMenuUI.enabled) PauseGame();
        else ResumeGame();
    }

   
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        if (isPaused)
    //            ResumeGame();
    //        else
    //            PauseGame();
    //    }
    //}

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Reanudar el tiempo normal
        pauseMenuUI.enabled=false; // Ocultar el menú de pausa
    }

    public void OpenOptions()
    {
        // Implementa tu lógica para abrir las opciones del juego
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Pausar el tiempo (detener todas las actualizaciones)
        pauseMenuUI.enabled=true; // Mostrar el menú de pausa
    }

    private void ThinkPause(InputAction.CallbackContext context)
    {
        Time.timeScale = Time.timeScale!=0f ? 0f:1f;
        //pauseMenuUI.enabled = !pauseMenuUI.enabled;

    }
}

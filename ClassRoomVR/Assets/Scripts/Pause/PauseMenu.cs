//using HurricaneVR.Framework.ControllerInput;
//using HurricaneVR.Framework.Core.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class PauseMenu : MonoBehaviour
    {
        private Canvas pauseMenuUI;
        public Button resumeButton;
        public Button optionsButton;
        public Button quitButton;

        [SerializeField] private InputActionProperty showButtonAction;
        [SerializeField] private InputActionProperty thinkAction;
        private void Start()
        {
            GameManager.Instance.IsPause = false;
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
            thinkAction.action.performed -= ThinkPause;
        }

        public void ToggleMenu(InputAction.CallbackContext context)
        {
            //pauseMenuUI.enabled = !pauseMenuUI.enabled;
            if (!GameManager.Instance.IsPause) PauseGame();
            else ResumeGame();
        }

        public void ResumeGame()
        {
            //Time.timeScale = 1f; // Reanudar el tiempo normal
            pauseMenuUI.enabled = false; // Ocultar el menú de pausa

            GameManager.Instance.Continue();
        }

        public void OpenOptions()
        {
            // Implementa tu lógica para abrir las opciones del juego
        }

        public void QuitGame()
        {
            quitButton.interactable=false;
            GameManager.Instance.LoadMainMenu();
        }

        private void PauseGame()
        {
            //Time.timeScale = 0f; // Pausar el tiempo (detener todas las actualizaciones)
            pauseMenuUI.enabled = true; // Mostrar el menú de pausa
            GameManager.Instance.Pause(false);
        }

        private void ThinkPause(InputAction.CallbackContext context)
        {
            if (Time.timeScale != 0)
            {
                SceneTransitionManager.singleton.fadeScreen.Fade(0, 0.5f,thinkControl);
            }
            else
            {
                Time.timeScale = 1f;
                SceneTransitionManager.singleton.fadeScreen.Fade(0.5f, 0);
            }
        }

        void thinkControl() 
        {
            Time.timeScale =  0f;
        }
     
    }

}

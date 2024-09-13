using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú de pausa del juego.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private Canvas _pauseMenuUI; // UI del menú de pausa
        [SerializeField] private Button _resumeButton; // Botón para reanudar el juego
        [SerializeField] private Button _quitButton; // Botón para salir del juego
        [SerializeField] private InputActionProperty _showButtonAction; // Acción para mostrar/ocultar el menú
        [SerializeField] private InputActionProperty _thinkAction; // Acción para activar la pausa mediante el pensamiento

        private void Start()
        {
            _pauseMenuUI = GetComponent<Canvas>();
            GameManager.Instance.IsPause = false;

            _resumeButton.onClick.AddListener(ResumeGame);
            _quitButton.onClick.AddListener(QuitGame);

            _showButtonAction.action.performed += ToggleMenu;
            _thinkAction.action.performed += ThinkPause;
        }

        private void OnDestroy()
        {
            _showButtonAction.action.performed -= ToggleMenu;
            _thinkAction.action.performed -= ThinkPause;
        }

        /// <summary>
        /// Alterna el estado del menú de pausa.
        /// </summary>
        /// <param name="context">Contexto del evento de entrada.</param>
        private void ToggleMenu(InputAction.CallbackContext context)
        {
            if (GameManager.Instance.IsPause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Reanuda el juego y oculta el menú de pausa.
        /// </summary>
        private void ResumeGame()
        {
            _pauseMenuUI.enabled = false;
            GameManager.Instance.Continue();
        }

        /// <summary>
        /// Pausa el juego y muestra el menú de pausa.
        /// </summary>
        private void PauseGame()
        {
            _pauseMenuUI.enabled = true;
            GameManager.Instance.Pause(false);
        }

        /// <summary>
        /// Alterna la pausa del juego mediante una acción de entrada.
        /// </summary>
        /// <param name="context">Contexto del evento de entrada.</param>
        private void ThinkPause(InputAction.CallbackContext context)
        {
            SceneTransitionManager.Singleton.FadeScreen.Fade(Time.timeScale != 0 ? 0 : 0.5f, 0.5f, ThinkControl);
            Time.timeScale = Time.timeScale == 0 ? 1f : 0f;
        }

        /// <summary>
        /// Controla la pausa del juego en el contexto de la acción de pensamiento.
        /// </summary>
        private void ThinkControl()
        {
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Sale del juego y carga el menú principal.
        /// </summary>
        private void QuitGame()
        {
            _quitButton.interactable = false;
            GameManager.Instance.LoadMainMenu();
        }
    }
}

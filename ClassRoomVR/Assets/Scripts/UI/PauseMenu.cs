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
        [SerializeField] private Button _resumeButton; // Botón para reanudar el juego
        [SerializeField] private Button _quitButton; // Botón para salir del juego
        
        private void Start()
        {
            GameManager.Instance.IsPause = false;

            _resumeButton.onClick.AddListener(ResumeGame);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy()
        {
            ResumeGame();
        }

        /// <summary>
        /// Reanuda el juego
        /// </summary>
        public void ResumeGame()
        {
            Time.timeScale = 1.0f;
            GameManager.Instance.Continue();
            SceneTransitionManager.Singleton.FadeScreen.Fade(0.8f, 0.0f);
        }

        /// <summary>
        /// Pausa el juego
        /// </summary>
        public void PauseGame()
        {
            SceneTransitionManager.Singleton.FadeScreen.Fade(0.0f, 0.8f, StopTime);
            GameManager.Instance.Pause(false);
        }

        private void StopTime()
        {
            Time.timeScale = 0.0f;
        }

        /// <summary>
        /// Alterna la pausa del juego mediante una acción de entrada.
        /// </summary>
        public void TogglePause()
        {
            if(GameManager.Instance.IsPause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
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
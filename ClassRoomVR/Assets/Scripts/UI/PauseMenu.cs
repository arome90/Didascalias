using Unity.VisualScripting;
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
        [SerializeField] private Canvas _canvas = null;

        bool _quitting = false;

        private void Start()
        {
            GameManager.Instance.IsPause = false;
            if(_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
            }
            _canvas.enabled = false;
            _resumeButton.onClick.AddListener(ResumeGameNoFade);
            _quitButton.onClick.AddListener(QuitGame);
        }

        /// <summary>
        /// hay que refactorizar un poco lo de la pausa porque hay 120 métodos llamándose entre sí
        /// </summary>
        private void ResumeGameNoFade()
        {
            ResumeGame(false);
        }

        private void OnDestroy()
        {
            ResumeGame(true);
        }

        private void Update()
        {
            _quitButton.interactable = !_quitting;
        }

        /// <summary>
        /// Reanuda el juego
        /// </summary>
        public void ResumeGame(bool fade)
        {
            _canvas.enabled = false;
            GameManager.Instance.Continue(fade);
        }

        /// <summary>
        /// Pausa el juego
        /// </summary>
        public void PauseGame(bool fade)
        {
            _canvas.enabled = fade;
            GameManager.Instance.Pause(false, fade);
        }

        /// <summary>
        /// Alterna la pausa del juego mediante una acción de entrada.
        /// </summary>
        public void TogglePause(bool fade)
        {
            if(GameManager.Instance.IsPause)
            {
                ResumeGame(fade);
            }
            else
            {
                PauseGame(fade);
            }
        }

        /// <summary>
        /// Sale del juego y carga el menú principal.
        /// </summary>
        private void QuitGame()
        {
            if(GameManager.Instance.IsPause)
            {
                ResumeGame(true);
            }
            _quitting = true;
            GameManager.Instance.LoadMainMenu();
        }


        [ContextMenu("Ejecutar MiMetodo")]
        public void MiMetodo()
        {
            QuitGame();
        }
    }
}
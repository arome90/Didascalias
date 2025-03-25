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
            _resumeButton.onClick.AddListener(ResumeGame);
            _quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy()
        {
            ResumeGame();
        }

        private void Update()
        {
            _quitButton.interactable = GameManager.Instance.IsPause && !_quitting;
        }

        /// <summary>
        /// Reanuda el juego
        /// </summary>
        public void ResumeGame()
        {
            _canvas.enabled = false;
            GameManager.Instance.Continue();
        }

        /// <summary>
        /// Pausa el juego
        /// </summary>
        public void PauseGame()
        {
            _canvas.enabled = true;
            GameManager.Instance.Pause(false);
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
            if(GameManager.Instance.IsPause)
            {
                ResumeGame();
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
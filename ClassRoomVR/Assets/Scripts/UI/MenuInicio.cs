using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú de inicio.
    /// </summary>
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] private Button _enter; // Botón para entrar
        [SerializeField] private Button _tutorial; // Botón para el tutorial
        [SerializeField] private Button _quitButton; // Botón para salir
        [SerializeField] private GameObject _nextScreen; // Pantalla siguiente
        [SerializeField] private Vector3 _playerDestination; // Destino del jugador
        [SerializeField] private Vector3 _playerInitialPosition; // Posición inicial del jugador
        [SerializeField] private Transform _player; // Transform del jugador

        private void Start()
        {
            AddButtonListeners();
            DeskManager.Instance.DestroyChildren();
            DontDestroyOnLoad(GameObject.Find("DeskManager"));
        }

        /// <summary>
        /// Agrega los listeners a los botones.
        /// </summary>
        private void AddButtonListeners()
        {
            _enter.onClick.AddListener(OnEnterButtonClick);
            _tutorial.onClick.AddListener(OnTutorialButtonClick);
            _quitButton.onClick.AddListener(QuitButton);
        }

        /// <summary>
        /// Maneja el clic en el botón de entrar.
        /// </summary>
        private void OnEnterButtonClick()
        {
            PlayButton();
            GoNextScreen();
        }

        /// <summary>
        /// Maneja el clic en el botón de tutorial.
        /// </summary>
        private void OnTutorialButtonClick()
        {
            _tutorial.interactable = false;
            GameManager.Instance.LoadTutorial();
        }

        /// <summary>
        /// Mueve a la pantalla siguiente.
        /// </summary>
        private void GoNextScreen()
        {
            _nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Maneja el clic en el botón de salir.
        /// </summary>
        public void QuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Mueve al jugador a la posición y rotación especificadas.
        /// </summary>
        public void PlayButton()
        {
            SetPlayerPositionAndRotation(_playerDestination, Quaternion.identity);
        }

        /// <summary>
        /// Regresa al jugador a la posición inicial y rotación especificadas.
        /// </summary>
        public void ReturnButton()
        {
            SetPlayerPositionAndRotation(_playerInitialPosition, Quaternion.Euler(Vector3.up * 90.0f));
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            ReturnButton();
            _nextScreen.SetActive(false);
        }

        /// <summary>
        /// Establece la posición y rotación del jugador.
        /// </summary>
        /// <param name="position">La nueva posición del jugador.</param>
        /// <param name="rotation">La nueva rotación del jugador.</param>
        private void SetPlayerPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (_player != null)
            {
                _player.position = position;
                _player.rotation = rotation;
            }
        }
    }
}

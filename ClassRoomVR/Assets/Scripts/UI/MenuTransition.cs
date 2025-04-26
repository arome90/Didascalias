using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la transición entre menús en la interfaz.
    /// </summary>
    public class MenuTransition : SceneSingleton<MenuTransition>
    {
        [SerializeField] private InputActionProperty _helpAction; // Acción de entrada para mostrar/ocultar texto de ayuda
        [SerializeField] private List<GameObject> _menus; // Lista de menús en la interfaz
        [SerializeField] private Button _backButton; // Botón para volver al menú anterior
        [SerializeField] private Button _startButton; // Botón para iniciar la partida
        [SerializeField] private int _currentMenuIndex; // Índice del menú actual
        [SerializeField] private MenuInicio _menuInicio; // Referencia al menú de inicio
        [SerializeField] private GameObject _player; // Objeto del jugador
        [SerializeField] private TextMeshProUGUI _textSession; // Texto para mostrar la sesión actual
        [SerializeField] private TextMeshProUGUI _text; // Texto para mostrar mensajes en la interfaz
        [SerializeField] private string[] _texts; // Array de textos para los mensajes en la interfaz

        private bool _isTextVisible; // Estado de visibilidad del texto de ayuda

        public override void Awake()
        {
            base.Awake();
            _backButton.onClick.AddListener(GoBackScreen);
            _startButton.onClick.AddListener(GoStart);
            WsClient.Instance.StartConnection();
        }

        private void OnEnable()
        {
            DisplayInitialPage();
            if (_helpAction != null)
            {
                _helpAction.action.performed += ToggleTextVisibility;
            }
        }

        private void OnDisable()
        {
            if (_helpAction != null)
            {
                _helpAction.action.performed -= ToggleTextVisibility;
            }
        }

        /// <summary>
        /// Muestra la página inicial y establece el menú activo.
        /// </summary>
        private void DisplayInitialPage()
        {
            _menus.ForEach(menu => menu.SetActive(false));
            SetActiveMenu(_currentMenuIndex);
            ChangeScreen(0);
        }

        /// <summary>
        /// Establece el menú activo basado en el índice proporcionado.
        /// </summary>
        /// <param name="menuIndex">Índice del menú a activar.</param>
        private void SetActiveMenu(int menuIndex)
        {
            _menus[menuIndex].SetActive(true);
            _menus[menuIndex].transform.parent.gameObject.SetActive(true);
        }

        /// <summary>
        /// Cambia al menú anterior.
        /// </summary>
        public void GoBackScreen()
        {
            if (_currentMenuIndex <= 0)
            {
                _currentMenuIndex = 0;
                _menus[_currentMenuIndex].SetActive(false);
                _menuInicio.ReturnButton();
                return;
            }

            _menus[_currentMenuIndex].SetActive(false);
            _currentMenuIndex--;
            _menus[_currentMenuIndex].SetActive(true);
            ChangeScreen(_currentMenuIndex);
        }

        /// <summary>
        /// Cambia al siguiente menú.
        /// </summary>
        public void GoNextScreen()
        {
            if (_currentMenuIndex < _menus.Count - 1)
            {
                _menus[_currentMenuIndex].SetActive(false);
                _currentMenuIndex++;
                _menus[_currentMenuIndex].SetActive(true);
                ChangeScreen(_currentMenuIndex);
            }
        }

        /// <summary>
        /// Inicia la partida y carga la escena principal.
        /// </summary>
        private void GoStart()
        {
            /////if (GameManager.Instance.GetWsConnection())
            /////{
                ToggleUIElements(false);
                _menus[_currentMenuIndex].SetActive(false);
                if (GameManager.Instance.GetCurrentSettings().name != "Personalizado")
                {
                    DeskManager.Instance.DestroyChildren();
                }
                _textSession.gameObject.SetActive(true);
                _textSession.text = WsClient.Instance.Session;
                GameManager.Instance.LoadMainScene();
            /////}
            /////else
            /////{
            /////if (GameManager.Instance.GetWsTryingToConnect())
            /////{
            /////Debug.Log("Ya estamos intentando conectar con el servidor.");
            /////return;
            /////}
            /////}
        }

        /// <summary>
        /// Establece la rotación del jugador para mostrar la pizarra.
        /// </summary>
        public void MovePizarra() => SetPlayerRotation(Vector3.zero);

        /// <summary>
        /// Establece la rotación del jugador para mostrar la clase.
        /// </summary>
        public void MoveClase() => SetPlayerRotation(new Vector3(0, 180, 0));

        /// <summary>
        /// Establece la rotación del jugador.
        /// </summary>
        /// <param name="rotation">Rotación a aplicar al jugador.</param>
        private void SetPlayerRotation(Vector3 rotation)
        {
            _player.transform.eulerAngles = rotation;
        }

        /// <summary>
        /// Muestra u oculta los elementos de la interfaz de usuario.
        /// </summary>
        /// <param name="isActive">Estado de visibilidad.</param>
        private void ToggleUIElements(bool isActive)
        {
            _startButton.gameObject.SetActive(isActive);
            _backButton.gameObject.SetActive(isActive);
        }

        /// <summary>
        /// Alterna la visibilidad del texto de ayuda.
        /// </summary>
        /// <param name="context">Contexto de la acción de entrada.</param>
        private void ToggleTextVisibility(InputAction.CallbackContext context)
        {
            _isTextVisible = !_isTextVisible;
            _text.transform.parent.gameObject.SetActive(_isTextVisible);
        }

        /// <summary>
        /// Cambia el texto mostrado en la interfaz según el índice.
        /// </summary>
        /// <param name="index">Índice del texto a mostrar.</param>
        public void ChangeScreen(int index)
        {
            _text.text = _texts[index];
        }
    }
}

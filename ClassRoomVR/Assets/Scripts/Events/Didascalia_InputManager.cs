using UnityEngine;
using UnityEngine.InputSystem;
namespace ClassRoomVR
{
    public class Didascalia_InputManager : MonoBehaviour
    {
        [SerializeField] InputActionReference menu;
        [SerializeField] InputActionReference pause;
        [SerializeField] InputActionReference primaryButton;

        [SerializeField] PauseMenu pauseMenu;

        private bool actionsInitialized = false;

        private void Start()
        {
            InitializeActions();
        }

        private void OnEnable()
        {
            InitializeActions();
        }

        private void OnDisable()
        {
            DeinitializeActions();
        }

        /// <summary>
        /// Método para inicializar las acciones y registrar los eventos.
        /// </summary>
        private void InitializeActions()
        {
            if (!actionsInitialized)
            {
                menu.action.Enable();
                pause.action.Enable();
                menu.action.performed += ToggleHandMenu;
                pause.action.performed += TogglePause;
                primaryButton.action.performed += TogglePauseNoFade;
                actionsInitialized = true;
            }
        }

        private void DeinitializeActions()
        {
            DisablePause();
            menu.action.performed -= ToggleHandMenu;
            pause.action.performed -= TogglePause;
            primaryButton.action.performed -= TogglePauseNoFade;
            menu.action.Disable();
            pause.action.Disable();
            actionsInitialized = false;
        }

        // <summary>
        /// Método para alternar el menú de la mano y el estado de pausa.
        /// El menú esta activo si esta en estado de pausa.
        /// </summary>
        private void ToggleHandMenu(InputAction.CallbackContext ctx)
        {
            if (GameManager.Instance.IsPause)
            {
                pauseMenu.ResumeGame(true);
            }
            else
            {
                pauseMenu.PauseGame(true);
            }
        }

        /// <summary>
        /// Método para alternar el estado de pausa.
        /// </summary>
        private void TogglePause(InputAction.CallbackContext ctx)
        {
            pauseMenu.TogglePause(true);
        }

        /// <summary>
        /// Método para alternar el estado de pausa.
        /// </summary>
        private void TogglePauseNoFade(InputAction.CallbackContext ctx)
        {
            pauseMenu.TogglePause(false);
        }

        /// <summary>
        /// Método para reanudar el juego si está en pausa.
        /// </summary>
        private void DisablePause()
        {
            pauseMenu.ResumeGame(true);
        }
    }

}

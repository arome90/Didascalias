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
            //HVRInputSystemController.Init();
            //HVRInputSystemController.InputActions.LeftHand.Menu.performed += ToggleMenu;
            //HVRInputSystemController.InputActions.RightHand.PrimaryButton.performed += ThinkPause;


        }


        private void OnDestroy()
        {
            showButtonAction.action.performed -= ToggleMenu;
            //HVRInputSystemController.InputActions.LeftHand.Menu.performed -= ToggleMenu;

        }

        public void ToggleMenu(InputAction.CallbackContext context)
        {
            //pauseMenuUI.enabled = !pauseMenuUI.enabled;
            if (!pauseMenuUI.enabled) PauseGame();
            else ResumeGame();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ThinkPause();
            }
        }

        public void ResumeGame()
        {
            //Time.timeScale = 1f; // Reanudar el tiempo normal
            pauseMenuUI.enabled = false; // Ocultar el menú de pausa

            GameManager.Instance.IsPause = false;
        }

        public void OpenOptions()
        {
            // Implementa tu lógica para abrir las opciones del juego
        }

        public void QuitGame()
        {
            quitButton.gameObject.SetActive(false);
            GameManager.Instance.LoadMainMenu();
        }

        private void PauseGame()
        {
            //Time.timeScale = 0f; // Pausar el tiempo (detener todas las actualizaciones)
            pauseMenuUI.enabled = true; // Mostrar el menú de pausa
            GameManager.Instance.IsPause = true;

        }

        private void ThinkPause(InputAction.CallbackContext context)
        {
            // Time.timeScale = Time.timeScale!=0f ? 0f:1f;
            //pauseMenuUI.enabled = !pauseMenuUI.enabled;
            TimeSelect();

        }
        private void ThinkPause()
        {
            TimeSelect();
            //pauseMenuUI.enabled = !pauseMenuUI.enabled;
        }

        void TimeSelect()
        {

            Time.timeScale = (Time.timeScale != 0) ? 0f : 1f;


            //if (Time.timeScale != 0)
            //{
            //    //AudioRecorder.PauseRecording();
            //    Time.timeScale = 0f;

            //}
            //else
            //{
            //    Time.timeScale = 1f;
            //    //AudioRecorder.ResumeRecording();
            //}
        }
    }

}

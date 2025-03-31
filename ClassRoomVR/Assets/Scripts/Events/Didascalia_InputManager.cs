using ClassRoomVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Didascalia_InputManager : MonoBehaviour
{
    [SerializeField] InputActionReference menu;
    [SerializeField] InputActionReference pause;

    [SerializeField] Canvas handMenu;
    [SerializeField] PauseMenu pauseMenu = null;

    private void Start()
    {
        if (pauseMenu == null) pauseMenu = handMenu.GetComponent<PauseMenu>();
        menu.action.Enable();
        pause.action.Enable();

        menu.action.performed += ToggleHandMenu;
        pause.action.performed += TogglePause;
    }

    private void OnEnable()
    {
        Start();
    }

    private void OnDisable()
    {
        DisableHandMenu();
        DisablePause();
        menu.action.performed -= ToggleHandMenu;
        pause.action.performed -= TogglePause;

        menu.action.Disable();
        pause.action.Disable();
    }

    private void ToggleHandMenu(InputAction.CallbackContext ctx)
    {
        bool isActive = !handMenu.enabled;
        handMenu.enabled = isActive;

        if (!GameManager2.Instance.IsPause && isActive)
        {
            pauseMenu.PauseGame();
        }
        else if (GameManager2.Instance.IsPause && !isActive)
        {
            pauseMenu.ResumeGame();
        }
    }

    private void DisableHandMenu()
    {
        handMenu.enabled = false;
    }

    private void TogglePause(InputAction.CallbackContext ctx)
    {
        pauseMenu.TogglePause();
    }

    private void DisablePause()
    {
        pauseMenu.ResumeGame();
    }
}

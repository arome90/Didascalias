using UnityEngine;
using UnityEngine.InputSystem;

public class ActivatePauseMenu : MonoBehaviour
{
    [SerializeField]
    InputActionReference _action;

    [SerializeField]
    GameObject _pauseMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _action.action.performed += TogglePauseMenu;
        _pauseMenu.SetActive(false);
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        bool activate = !_pauseMenu.activeSelf;
        _pauseMenu.SetActive(activate);
        if (activate) ClassManager.PauseGame();
        else ClassManager.ResumeGame();
    }

    private void OnDestroy()
    {
        _action.action.performed -= TogglePauseMenu;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

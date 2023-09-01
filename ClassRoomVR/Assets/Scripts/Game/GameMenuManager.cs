using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private Transform headTransform; // The player's head position
    [SerializeField] private float spawnDistance = 2f; // Distance from the head to spawn the menu
    [SerializeField] private GameObject menuObject; // The menu object to toggle
    [SerializeField] private InputActionProperty showButtonAction; // Input action to show/hide the menu

    private void Update()
    {
        CheckToggleMenuInput(); // Check if the menu toggle input is triggered
        UpdateMenuPosition(); // Update the menu's position and rotation
    }

    private void CheckToggleMenuInput()
    {
        if (ShouldToggleMenu())
        {
            ToggleMenu(); // Toggle the menu on/off
        }
    }

    private bool ShouldToggleMenu()
    {
        // Check if the showButtonAction was pressed or the Q key was pressed
        bool inputPressed = showButtonAction != null && showButtonAction.action.WasPressedThisFrame();
        bool qKeyPressed = Input.GetKeyDown(KeyCode.Q);
        return inputPressed || qKeyPressed;
    }

    private void ToggleMenu()
    {
        if (menuObject != null)
        {
            menuObject.SetActive(!menuObject.activeSelf); // Toggle the menu's active state
        }
    }

    private void UpdateMenuPosition()
    {
        if (IsMenuActiveAndHeadTransformValid())
        {
            Vector3 menuPosition = CalculateMenuPosition(); // Calculate the menu's position
            SetMenuPositionAndRotation(menuPosition); // Set the menu's position and rotation
        }
    }

    private bool IsMenuActiveAndHeadTransformValid()
    {
        // Check if the menu is active, the head transform is valid, and the menu object is not null
        return menuObject != null && menuObject.activeSelf && headTransform != null;
    }

    private Vector3 CalculateMenuPosition()
    {
        Vector3 spawnDirection = GetHorizontalForwardDirection(headTransform.forward);
        return headTransform.position + spawnDirection * spawnDistance; // Calculate the position based on head position and direction
    }

    private Vector3 GetHorizontalForwardDirection(Vector3 forward)
    {
        return new Vector3(forward.x, 0f, forward.z).normalized; // Get the forward direction ignoring the vertical component
    }

    private void SetMenuPositionAndRotation(Vector3 position)
    {
        // Set the menu's position and make it look at the player's head position
        menuObject.transform.position = position;
        menuObject.transform.LookAt(new Vector3(headTransform.position.x, menuObject.transform.position.y, headTransform.position.z));
        menuObject.transform.forward *= -1f; // Invert the forward direction to face the player
    }
}

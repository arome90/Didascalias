using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenuManager : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private GameObject menuObject;
    [SerializeField] private InputActionProperty showButtonAction;

    private void Update()
    {
        if (showButtonAction.action.WasPressedThisFrame() || Input.GetKeyDown(KeyCode.Q))
        {
            ToggleMenu();
        }

        if (menuObject.activeSelf)
        {
            UpdateMenuPosition();
        }
    }

    private void ToggleMenu()
    {
        menuObject.SetActive(!menuObject.activeSelf);
    }

    private void UpdateMenuPosition()
    {
        Vector3 menuPosition = CalculateMenuPosition();
        menuObject.transform.position = menuPosition;
        menuObject.transform.LookAt(new Vector3(headTransform.position.x, menuObject.transform.position.y, headTransform.position.z));
        menuObject.transform.forward *= -1f;
    }

    private Vector3 CalculateMenuPosition()
    {
        Vector3 spawnDirection = new Vector3(headTransform.forward.x, 0f, headTransform.forward.z).normalized;
        Vector3 menuPosition = headTransform.position + spawnDirection * spawnDistance;
        return menuPosition;
    }
}

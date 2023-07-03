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
        if (showButtonAction != null && showButtonAction.action.WasPressedThisFrame() || Input.GetKeyDown(KeyCode.Q))
        {
            ToggleMenu();
        }

        if (menuObject != null && menuObject.activeSelf)
        {
            UpdateMenuPosition();
        }
    }

    private void ToggleMenu()
    {
        if (menuObject != null)
        {
            menuObject.SetActive(!menuObject.activeSelf);
        }
    }

    private void UpdateMenuPosition()
    {
        if (menuObject != null && headTransform != null)
        {
            Vector3 menuPosition = CalculateMenuPosition();
            menuObject.transform.position = menuPosition;
            menuObject.transform.LookAt(new Vector3(headTransform.position.x, menuObject.transform.position.y, headTransform.position.z));
            menuObject.transform.forward *= -1f;
        }
    }

    private Vector3 CalculateMenuPosition()
    {
        if (headTransform != null)
        {
            Vector3 spawnDirection = new Vector3(headTransform.forward.x, 0f, headTransform.forward.z).normalized;
            Vector3 menuPosition = headTransform.position + spawnDirection * spawnDistance;
            return menuPosition;
        }

        return Vector3.zero;
    }
}

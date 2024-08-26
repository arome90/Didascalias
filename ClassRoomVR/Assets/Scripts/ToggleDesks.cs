using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDesks : MonoBehaviour
{
    [SerializeField] protected Toggle prefab; // Toggle prefab for controlling desk visibility
    private Dictionary<Toggle, Desk> toggleToDeskMap; // Map to associate toggles with desks
    private GameObject parent;
    private void Start()
    {
        toggleToDeskMap = new Dictionary<Toggle, Desk>(); // Initialize the dictionary
    }
    public void CreateToggles(List<Vector2>positions, List<Desk>desks)
    {
        Destroy(parent);
        parent = new GameObject("Toggles");
        parent.transform.SetParent(transform, false);
        toggleToDeskMap.Clear();
        Toggle toggle = null;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = transform.position + new Vector3(positions[i].x / 5.0f, -positions[i].y / 5.0f);
            toggle = Instantiate(prefab, pos, Quaternion.identity, parent.transform);
            toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
            toggleToDeskMap.Add(toggle, desks[i]);
        }
    }
    // Method to handle toggle value change
    void ChangeDesk(Toggle toggle)
    {
        toggleToDeskMap[toggle].gameObject.SetActive(toggle.isOn);
    }
}

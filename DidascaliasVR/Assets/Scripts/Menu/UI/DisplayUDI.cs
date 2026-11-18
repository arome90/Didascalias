using UnityEngine;
using TMPro;

public class DisplayUDI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = GetComponent<TextMeshProUGUI>().text.Replace("{0}", UnityEngine.SystemInfo.deviceUniqueIdentifier.ToString());
    }
}

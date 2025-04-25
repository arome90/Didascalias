using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClassRoomVR;

public class WsTxt : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Seting ws txt");
        GameManager.Instance.SetWsTxt(gameObject); // Configura la barra de carga en el GameManager
    }
    public void SetText(string s)
    {
        if (GetComponentInChildren<TMPro.TextMeshProUGUI>() != null)
        {
            GetComponentInChildren<TMPro.TextMeshProUGUI>().text = s;
        }
        else
        {
            Debug.LogError("No se encontró el componente Text en el objeto.");
        }
    }
}

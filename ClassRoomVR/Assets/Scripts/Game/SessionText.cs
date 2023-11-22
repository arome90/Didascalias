using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionText : MonoBehaviour
{
    TMPro.TextMeshProUGUI textSession; // Reference to the session text

    private void Start()
    {
        GetComponent<TMPro.TextMeshProUGUI>().text = WsClient.Instance.session;
    }
}

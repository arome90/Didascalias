using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;


public class ToggleConnection : MonoBehaviour
{
    [SerializeField] Button button; // Reference to the toggle UI element
    [SerializeField] TextMeshProUGUI text; // Reference to the text of the toggle
    bool value;
    void Start()
    {
        button.onClick.AddListener(Click);
        WsClient.Instance.Disconnect();
        text.text = "Conectarse"; // Update the text of the toggle

    }

    // Called when the toggle value changes (connection state changes)
    private void Click()
    {
        value = !value;
        WsClient.Instance.ToggleCon();
        if (value && WsClient.Instance.isAlive())
        {
            // Start the WebSocket connection
            text.text = "Desconectarse"; // Update the text of the toggle
            // Schedule a method to update the session text after a delay
           
        }
        else
        {
            // Disconnect the WebSocket client
            text.text = "Conectarse"; // Update the text of the toggle
        }
    }

}

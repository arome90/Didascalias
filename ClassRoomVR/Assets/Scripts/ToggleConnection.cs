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
    [SerializeField] TextMeshProUGUI textSession; // Reference to the session text
    bool value;
    void Start()
    {
        button.onClick.AddListener(Click);
        value = WsClient.Instance.isAlive();
        if (value)
        {
            // Start the WebSocket connection
            text.text = "Desconectarse"; // Update the text of the toggle
            textSession.text = WsClient.Instance.session;

        }
        else
        {
            // Disconnect the WebSocket client
            text.text = "Conectarse"; // Update the text of the toggle
            textSession.text = string.Empty; // Clear the session text
        }
    }

    // Called when the toggle value changes (connection state changes)
    private void Click()
    {
        Debug.Log("ahh");
        value = !value;
        WsClient.Instance.ToggleCon();
        if (value)
        {
            // Start the WebSocket connection
            text.text = "Desconectarse"; // Update the text of the toggle
            // Schedule a method to update the session text after a delay
            Debug.Log(WsClient.Instance.session+"oooooooo");
            Invoke("ChangeSession", 1f);
        }
        else
        {
            // Disconnect the WebSocket client
            text.text = "Conectarse"; // Update the text of the toggle
            textSession.text = string.Empty; // Clear the session text
        }
    }

    // Update the session text based on the WebSocket client's session
    private void ChangeSession()
    {
        textSession.text = WsClient.Instance.session;
    }
}

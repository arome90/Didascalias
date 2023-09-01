using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleConnection : MonoBehaviour
{
    [SerializeField] WsClient client; // Reference to the WebSocket client
    [SerializeField] Toggle toggle; // Reference to the toggle UI element
    [SerializeField] Text textToggle; // Reference to the text of the toggle
    [SerializeField] TextMeshProUGUI textSession; // Reference to the session text

    void Start()
    {
        // client.onSessionChanged.AddListener(ChangeSession);

        toggle.onValueChanged.AddListener(Change);
    }

    // Called when the toggle value changes (connection state changes)
    private void Change(bool value)
    {
        if (value)
        {
            // Start the WebSocket connection
            client.StartConnection();
            textToggle.text = "Desconectarse"; // Update the text of the toggle
            // Schedule a method to update the session text after a delay
            Invoke("ChangeSession", 0.5f);
        }
        else
        {
            // Disconnect the WebSocket client
            client.Disconnect();
            textToggle.text = "Conectarse"; // Update the text of the toggle
            textSession.text = string.Empty; // Clear the session text
        }
    }

    // Update the session text based on the WebSocket client's session
    private void ChangeSession()
    {
        textSession.text = client.session;
    }
}

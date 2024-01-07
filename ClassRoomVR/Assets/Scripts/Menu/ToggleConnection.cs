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
    void Start()
    {
        button.onClick.AddListener(Click);
        WsClient.Instance.Disconnect();
        text.text = "Conectarse"; // Update the text of the toggle

    }

    // Called when the toggle value changes (connection state changes)
    private void Click()
    {
        bool before = WsClient.Instance.connected;
        WsClient.Instance.ToggleCon();
        StartCoroutine(SetText(!before));
    }


    IEnumerator SetText(bool connected)
    {

        yield return new WaitUntil(() => WsClient.Instance.isAlive() == connected);
        if (connected)
        {
            // Start the WebSocket connection
            text.text = "Desconectarse"; // Update the text of the toggle
                                    
        }
        else
        {
            // Disconnect the WebSocket client
            text.text = "Conectarse"; // Update the text of the toggle
        }
    }

}

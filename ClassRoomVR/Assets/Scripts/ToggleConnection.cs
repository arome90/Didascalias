using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleConnection : MonoBehaviour
{
    [SerializeField] WsClient client;
    [SerializeField] Toggle toggle;
    [SerializeField] Text textToggle;
    [SerializeField] TextMeshProUGUI textSession;
    void Start()
    {
        toggle.onValueChanged.AddListener(Change);
       // client.onSessionChanged.AddListener(ChangeSession);
        
    }
    private void Change(bool value)
    {
        if (value)
        {
            client.StartConnection();
            textToggle.text = "Desconectarse";
            Invoke("ChangeSession",0.5f);
        }
        else
        {
            client.Disconnect();
            textToggle.text = "Conectarse";
            textSession.text = string.Empty;
        }
    }
  
    private void ChangeSession()
    {
        textSession.text = client.session;
    }





}

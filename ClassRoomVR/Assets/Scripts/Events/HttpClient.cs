using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

public class HttpClient  : GenericSingleton<HttpClient>
{
    private DateTime currentAppDate = new DateTime();

    public DateTime getCurrentAppTime() { return currentAppDate; }

    public void sendJson(string jsonText)
    {
        StartCoroutine(sendJsonNet(jsonText));
        Debug.Log("SendJson " + jsonText);
    }

    
    IEnumerator sendJsonNet(string jsonText)
    {
        using (UnityWebRequest www = UnityWebRequest.Post("https://cyclops-dev.uab.cat/data/vr", jsonText, "application/json"))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log("ERROR enviando json");
            }
            else
            {
                Debug.Log("UnityWebRequest result: " + www.result);
                Debug.Log(www.downloadHandler.text);
                Debug.Log("json enviado correctamente!");
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

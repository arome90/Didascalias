using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class HttpClient : GenericSingleton<HttpClient>
{
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
}

using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AzureManagerCLU : MonoBehaviour
{
    [Header("Credenciales de Azure CLU")]
    [Tooltip("La URL del Endpoint de tu recurso de Lenguaje. DEBE terminar con '/'")]
    public string endpointAzure = "https://<TU_RECURSO>.cognitiveservices.azure.com/";
    [Tooltip("Clave de suscripción (Key 1 o Key 2) del recurso de Lenguaje")]
    public string claveAzure = "<TU_CLAVE_AQUI>";

    [Header("Configuración del Modelo")]
    public string nombreProyecto = "<NOMBRE_DEL_PROYECTO>";
    public string nombreDespliegue = "<NOMBRE_DEL_DESPLIEGUE>";
    public string apiVersion = "2024-11-01";

    /// <summary>
    /// Llama a esta función pasando el texto reconocido para obtener la intención.
    /// </summary>
    public void SendAzureCommand(string texto)
    {
        StartCoroutine(RequestCLU(texto));
    }

    private IEnumerator RequestCLU(string texto)
    {
        // Montaje de la URL para la API REST de Azure
        string url = $"{endpointAzure}language/:analyze-conversations?api-version={apiVersion}";

        string jsonBody = $@"
            {{
                ""kind"": ""Conversation"",
                ""analysisInput"": {{
                    ""conversationItem"": {{
                        ""id"": ""1"",
                        ""participantId"": ""Usuario"",
                        ""text"": ""{texto}""
                    }}
                }},
                ""parameters"": {{
                    ""projectName"": ""{nombreProyecto}"",
                    ""deploymentName"": ""{nombreDespliegue}"",
                    ""stringIndexType"": ""TextElement_V8""
                }}
            }}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Ocp-Apim-Subscription-Key", claveAzure);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcessAzureAnswer(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"[Azure CLU] Error en la petición: {request.error}");
            }
        }
    }

    private void ProcessAzureAnswer(string rawJson)
    {
        try
        {
            CluResponse answer = JsonUtility.FromJson<CluResponse>(rawJson);
            string topIntent = answer.result.prediction.topIntent;

            Debug.Log($"[IA] Intención identificada: {topIntent}");

            // TODO: Reemplazar este switch con la lógica de animaciones o eventos de vuestro proyecto
            ExecuteAction(topIntent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Azure CLU] Error al decodificar la respuesta: {e.Message}");
        }
    }

    private void ExecuteAction(string intencion)
    {
        switch (intencion)
        {
            case "<NOMBRE_INTENCION_1>":
                Debug.Log("Acción 1 ejecutada.");
                break;
            case "<NOMBRE_INTENCION_2>":
                Debug.Log("Acción 2 ejecutada.");
                break;
            default:
                Debug.LogWarning($"Intención '{intencion}' reconocida, pero sin acción mapeada.");
                break;
        }
    }
}

// Estructuras de datos para leer el JSON devuelto por Azure
[Serializable]
public class CluResponse { public CluResult result; }
[Serializable]
public class CluResult { public CluPrediction prediction; }
[Serializable]
public class CluPrediction { public string topIntent; public float confidenceScore; }
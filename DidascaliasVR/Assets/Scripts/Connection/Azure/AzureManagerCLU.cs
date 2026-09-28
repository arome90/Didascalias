using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public enum PlayerAction
{
    // Portuguese
    Acalmar,
    Acolher,
    Adiar,
    Advertencia,
    AjustarRitmo,
    Castigo,
    ChamarAluno,
    Compreensao,
    DarApoio,
    Despedida,
    EloqioPositivo,
    EstablecerLimites,
    ExplicarNovamente,
    Expulsão,
    FalarBaixo,
    IncentivarAutonomia,
    MoverAluno,
    NegociarAcordo,
    Parabenizar,
    PararConflito,
    PausarAula,
    PedirApoio,
    PegarMaterial,
    Perguntar,
    PromoverRespeito,
    ProporAlternativa,
    ReforçarRegra,
    RegularEmocao,
    Saudações,
    Sentarse,
    Silencio,
    Trabalhar,
    TrocarAluno,
    TrocarAtividade,

    // Spanish

}

public class AzureManagerCLU : MonoBehaviour
{
    [Header("Credenciales de Azure CLU")]
    [Tooltip("Language resource Azure endpoint. Must end with '/'")]
    public string endpointAzure = "https://<RESOURCE>.cognitiveservices.azure.com/";
    [Tooltip("Subscription key for the resource")]
    public string claveAzure = "<TU_CLAVE_AQUI>";

    [Header("Model Settings")]
    public string projectName   = "<NOMBRE_DEL_PROYECTO>";
    public string displayName   = "<NOMBRE_DEL_DESPLIEGUE>";
    public string apiVersion    = "2024-11-01";

    /// <summary>
    /// Llama a esta función pasando el texto reconocido para obtener la intención.
    /// </summary>
    public void SendAzureCommand(string texto)
    {
        StartCoroutine(RequestCLU(texto));
    }

    private IEnumerator RequestCLU(string texto)
    {
        // URL for api calls
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
                    ""projectName"": ""{projectName}"",
                    ""deploymentName"": ""{displayName}"",
                    ""stringIndexType"": ""TextElement_V8""
                }}
            }}";

        // sending the request
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
                Debug.LogError($"[Azure CLU] Error on request: {request.error}");
            }
        }
    }

    private void ProcessAzureAnswer(string rawJson)
    {
        try
        {
            CluResponse answer = JsonUtility.FromJson<CluResponse>(rawJson);
            string topIntent = answer.result.prediction.topIntent;

            if (!Enum.TryParse<PlayerAction>(topIntent, out PlayerAction action))
            {
                Debug.Log($"[AzureManagerCLU] Intent of type '{topIntent}' does not have a correspoding" +
                    $" PlayerActions value. It was no correctly parsed. Intent will not be executed.");
                return;
            }

            ExecuteAction(action);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Azure CLU] Error al decodificar la respuesta: {e.Message}");
        }
    }

    private void ExecuteAction(PlayerAction action) => Player.Instance.ProcessAction(action);
}

// Estructuras de datos para leer el JSON devuelto por Azure
[Serializable]
public class CluResponse { public CluResult result; }
[Serializable]
public class CluResult { public CluPrediction prediction; }
[Serializable]
public class CluPrediction { public string topIntent; public float confidenceScore; }
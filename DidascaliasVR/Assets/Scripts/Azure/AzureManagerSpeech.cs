using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AzureManagerSpeech : MonoBehaviour
{
    [Header("Modo de Pruebas / Consumo")]
    [Tooltip("Si está desactivado, NO se realizarán llamadas a Azure para ahorrar tokens/crédito.")]
    public bool useAzure = true;

    [Header("Referencias del Sistema")]
    [SerializeField] private MicrophoneInputHandler micHandler;
    [SerializeField] private AzureManagerCLU cluController;

    [Header("Credenciales de Azure Speech")]
    public string claveSpeech = "<TU_CLAVE_DE_VOZ_AQUI>";
    public string regionSpeech = "<REGION_EJEMPLO_eastus>";

    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI displayText = null;

    private void OnEnable()
    {
        if (micHandler != null)
        {
            micHandler.OnSpeechRecorded += HandleSpeechRecorded;
        }
    }

    private void OnDisable()
    {
        if (micHandler != null)
        {
            micHandler.OnSpeechRecorded -= HandleSpeechRecorded;
        }
    }

    private void HandleSpeechRecorded(byte[] wavData)
    {
        // Si no queremos gastar llamadas a la API, simulamos el flujo y reanudamos la escucha
        if (!useAzure)
        {
            Debug.LogWarning("[Azure Speech] 'useAzure' está DESACTIVADO. Audio capturado correctamente pero NO enviado a la nube.");
            
            if (displayText != null) 
                displayText.text = "[Modo prueba: Audio capturado localmente]";

            // Reanudamos la escucha del micrófono sin hacer llamadas HTTP
            if (micHandler != null) 
                micHandler.ResumeListening();

            return;
        }

        StartCoroutine(SendAudioAzureREST(wavData));
    }

    private IEnumerator SendAudioAzureREST(byte[] wavData)
    {
        string url = $"https://{regionSpeech}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language=es-ES";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(wavData);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Ocp-Apim-Subscription-Key", claveSpeech);
            request.SetRequestHeader("Content-Type", "audio/wav; codecs=audio/pcm; samplerate=16000");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AzureSpeechResponse respuesta = JsonUtility.FromJson<AzureSpeechResponse>(request.downloadHandler.text);

                if (!string.IsNullOrEmpty(respuesta.DisplayText))
                {
                    Debug.Log($"[Texto Reconocido]: {respuesta.DisplayText}");
                    
                    if (cluController != null && cluController.enabled) 
                        cluController.SendAzureCommand(respuesta.DisplayText);
                        
                    if (displayText != null) 
                        displayText.text = respuesta.DisplayText;
                }
            }
            else
            {
                Debug.LogError($"[Azure Speech] Error en la nube: {request.error}");
            }

            // Notifica al micrófono que la petición terminó y puede volver a escuchar
            if (micHandler != null)
            {
                micHandler.ResumeListening();
            }
        }
    }
}

[Serializable]
public class AzureSpeechResponse 
{ 
    public string DisplayText; 
}
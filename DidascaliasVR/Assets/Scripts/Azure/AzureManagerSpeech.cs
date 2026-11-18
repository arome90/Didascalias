using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Didascalia;

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
        // Calcular la duración en segundos directamente a partir del tamaño del WAV
        // Encabezado WAV = 44 bytes. 16000 muestras/seg * 2 bytes/muestra (16 bits por muestra / 8 = 2) = 32000 bytes/segundo
        float durationSeconds = (wavData.Length > 44) ? (wavData.Length - 44) / 32000f : 0f;

#if UNITY_EDITOR
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
#endif
        StartCoroutine(SendAudioAzureREST(wavData, durationSeconds));
    }

    private IEnumerator SendAudioAzureREST(byte[] wavData, float durationSeconds)
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

                    // Cálculo de métricas prosódicas de tasa de habla (Speech Rate)
                    float wpm = CalculateWordsPerMinute(respuesta.DisplayText, durationSeconds);
                    float sps = CalculateSyllablesPerSecond(respuesta.DisplayText, durationSeconds);

                    Debug.Log($"[Prosodia] Duración: {durationSeconds:F2}s | WPM: {wpm:F1} | SPS: {sps:F2}");

                    // Guardar la transcripción en la sesión persistente
                    SaveSpeechToPersistence(respuesta.DisplayText, "Success", durationSeconds, wpm, sps);
                    
                    if (cluController != null && cluController.enabled) 
                        cluController.SendAzureCommand(respuesta.DisplayText);
                        
                    if (displayText != null) 
                        displayText.text = respuesta.DisplayText;
                }
            }
            else
            {
                Debug.LogError($"[Azure Speech] Error en la nube: {request.error}");

                // Guarda también los intentos fallidos si se requiere trazabilidad
                SaveSpeechToPersistence($"[Error]: {request.error}", "Error", durationSeconds, 0f, 0f);
            }

            // Notifica al micrófono que la petición terminó y puede volver a escuchar
            if (micHandler != null)
            {
                micHandler.ResumeListening();
            }
        }
    }

    /// <summary>
    /// Calcula Palabras por Minuto (WPM).
    /// Rango docente normal: 130 - 160 WPM.
    /// </summary>
    private float CalculateWordsPerMinute(string text, float durationSeconds)
    {
        if (durationSeconds <= 0f || string.IsNullOrWhiteSpace(text)) return 0f;

        string[] words = text.Split(new char[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        float minutes = durationSeconds / 60f;

        return (float)Math.Round(words.Length / minutes, 2);
    }

    /// <summary>
    /// Estimación aproximada de Sílabas por Segundo (SPS)
    /// contando los grupos vocálicos del texto transcrito.
    /// Rango docente óptimo en español: 4.5 - 6.5 SPS.
    /// </summary>
    private float CalculateSyllablesPerSecond(string text, float durationSeconds)
    {
        if (durationSeconds <= 0f || string.IsNullOrWhiteSpace(text)) return 0f;

        // Expresión regular para contar núcleos vocálicos en español
        MatchCollection matches = Regex.Matches(text.ToLower(), @"[aáeéiíoóuúü]+");
        int estimatedSyllables = matches.Count;

        return (float)Math.Round(estimatedSyllables / durationSeconds, 2);
    }

    /// <summary>
    /// Instancia SpeechData y la envía al GameDataManager
    /// </summary>
    private void SaveSpeechToPersistence(string text, string status, float duration, float wpm, float sps)
    {
        if (GameDataManager.Instance != null)
        {
            SpeechData speechSample = new SpeechData(text, duration, wpm, sps, status);
            GameDataManager.Instance.SendData(speechSample);
        }
    }
}

[Serializable]
public class AzureSpeechResponse 
{ 
    public string DisplayText; 
}
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class AzureManagerSpeech : MonoBehaviour
{
    [Header("Credenciales de Azure Speech")]
    public string claveSpeech = "<TU_CLAVE_DE_VOZ_AQUI>";
    public string regionSpeech = "<REGION_EJEMPLO_eastus>";

    [Header("Conexión con el Cerebro (CLU)")]
    public AzureManagerCLU cluController;

    [Header("Configuración de Escucha Libre")]
    [Tooltip("Sensibilidad del micrófono (ej: 0.02). Aumentar si hay ruido de fondo.")]
    public float volumeLimit = 0.02f;
    [Tooltip("Segundos de silencio para asumir que el usuario ha terminado de hablar.")]
    public float maxSilenceTime = 1.5f;

    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI displayText = null;

    private string microphoneID;
    private AudioClip monitorClip;
    private AudioClip commandClip;

    private bool isTalking = false;
    private float silenceCounter = 0f;
    private bool awaitingAnswer = false;

    void Start()
    {
        microphoneID = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (microphoneID == null)
        {
            Debug.LogError("[Azure Speech] ¡Ningún micrófono detectado!");
            return;
        }
        else
        {
            Debug.Log("[Azure Speech] Microphone detected: " + microphoneID);
        }

        StartAmbientMonitorization();
    }

    private void StartAmbientMonitorization()
    {
        if (awaitingAnswer) return;
        monitorClip = Microphone.Start(microphoneID, true, 1, 16000);
        isTalking = false;
    }

    void Update()
    {
        if (microphoneID == null || awaitingAnswer) return;

        float volume = GetMicrophoneVolume();

        // 1. El usuario EMPIEZA a hablar
        if (!isTalking && volume > volumeLimit)
        {
            isTalking = true;
            silenceCounter = 0f;
            Debug.Log("[Azure Speech] Speech detected. Recording...");

            Microphone.End(microphoneID);
            commandClip = Microphone.Start(microphoneID, false, 15, 16000);
        }
        // 2. El usuario hace SILENCIO
        else if (isTalking && volume < volumeLimit)
        {
            silenceCounter += Time.deltaTime;

            if (silenceCounter >= maxSilenceTime)
            {
                Debug.Log("[Azure Speech] Speech concluded. Sending...");
                isTalking = false;
                awaitingAnswer = true;

                int posicionFinal = Microphone.GetPosition(microphoneID);
                // stop recording
                Microphone.End(microphoneID);

                SendAudio(posicionFinal);
            }
        }
        // 3. El usuario CONTINÚA hablando
        else if (isTalking && volume >= volumeLimit)
        {
            silenceCounter = 0f;
        }
    }

    private float GetMicrophoneVolume()
    {
        int position = Microphone.GetPosition(microphoneID);
        int sampleSize = 256;

        if (position < sampleSize) return 0;

        AudioClip actualClip = isTalking ? commandClip : monitorClip;
        if (actualClip == null) return 0;

        int offset = position - sampleSize;
        if (offset < 0 || offset + sampleSize > actualClip.samples) return 0;

        float[] samples = new float[sampleSize];

        try { actualClip.GetData(samples, offset); }
        catch { return 0; }

        float suma = 0;
        for (int i = 0; i < samples.Length; i++) suma += samples[i] * samples[i];

        return Mathf.Sqrt(suma / samples.Length);
    }

    private void SendAudio(int posicionFinal)
    {
        if (posicionFinal > 0)
        {
            AudioClip cutClip = AudioClip.Create("Comando", posicionFinal, commandClip.channels, commandClip.frequency, false);
            float[] datosAudio = new float[posicionFinal * commandClip.channels];
            commandClip.GetData(datosAudio, 0);
            cutClip.SetData(datosAudio, 0);

            StartCoroutine(SendAudioAzureREST(cutClip));
        }
        else
        {
            awaitingAnswer = false;
            StartAmbientMonitorization();
        }
    }

    private IEnumerator SendAudioAzureREST(AudioClip clip)
    {
        byte[] wavData = ConvertToWav(clip);
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
                    if (cluController != null) cluController.SendAzureCommand(respuesta.DisplayText);
                    if (displayText != null) displayText.text = respuesta.DisplayText;
                }
            }
            else
            {
                Debug.LogError($"[Azure Speech] Error en la nube: {request.error}");
            }

            awaitingAnswer = false;
            StartAmbientMonitorization();
        }
    }

    private byte[] ConvertToWav(AudioClip clip)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            stream.Write(new byte[44], 0, 44);

            float[] sampleData = new float[clip.samples * clip.channels];
            clip.GetData(sampleData, 0);

            Int16[] intData = new Int16[sampleData.Length];
            byte[] bytesData = new byte[sampleData.Length * 2];
            for (int i = 0; i < sampleData.Length; i++)
            {
                intData[i] = (short)(sampleData[i] * 32767);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }
            stream.Write(bytesData, 0, bytesData.Length);

            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"), 0, 4);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 8), 0, 4);
            stream.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"), 0, 4);
            stream.Write(System.Text.Encoding.UTF8.GetBytes("fmt "), 0, 4);
            stream.Write(BitConverter.GetBytes(16), 0, 4);
            stream.Write(BitConverter.GetBytes((short)1), 0, 2);
            stream.Write(BitConverter.GetBytes((short)clip.channels), 0, 2);
            stream.Write(BitConverter.GetBytes(clip.frequency), 0, 4);
            stream.Write(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, 4);
            stream.Write(BitConverter.GetBytes((short)(clip.channels * 2)), 0, 2);
            stream.Write(BitConverter.GetBytes((short)16), 0, 2);
            stream.Write(System.Text.Encoding.UTF8.GetBytes("data"), 0, 4);
            stream.Write(BitConverter.GetBytes((int)stream.Length - 44), 0, 4);

            return stream.ToArray();
        }
    }
}

[Serializable]
public class AzureSpeechResponse { public string DisplayText; }
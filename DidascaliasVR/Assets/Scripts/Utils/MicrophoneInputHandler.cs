using System;
using System.IO;
using UnityEngine;

public class MicrophoneInputHandler : MonoBehaviour
{
    [Header("Configuración de Escucha Libre")]
    [Tooltip("Sensibilidad del micrófono (ej: 0.02). Aumentar si hay ruido de fondo.")]
    public float volumeLimit = 0.02f;

    [Tooltip("Segundos de silencio para asumir que el usuario ha terminado de hablar.")]
    public float maxSilenceTime = 1.5f;

    [Tooltip("Duración máxima de la grabación en segundos.")]
    public int maxRecordTime = 15;

    // Evento que notifica cuando una frase ha sido capturada y convertida a WAV
    public event Action<byte[]> OnSpeechRecorded;

    private string microphoneID;
    private AudioClip monitorClip;
    private AudioClip commandClip;

    private bool isTalking = false;
    private float silenceCounter = 0f;
    private bool isProcessing = false;

    void Start()
    {
        microphoneID = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;

        if (microphoneID == null)
        {
            Debug.LogError("[MicrophoneHandler] ¡Ningún micrófono detectado!");
            return;
        }

        Debug.Log("[MicrophoneHandler] Micrófono detectado: " + microphoneID);
        StartAmbientMonitorization();
    }

    public void StartAmbientMonitorization()
    {
        if (isProcessing || microphoneID == null) return;

        monitorClip = Microphone.Start(microphoneID, true, 1, 16000);
        isTalking = false;
        silenceCounter = 0f;
    }

    void Update()
    {
        if (microphoneID == null || isProcessing) return;

        float volume = GetMicrophoneVolume();

        // Debug.Log("[MicrophoneHandler] Micro: " + microphoneID);
        // Debug.Log("[MicrophoneHandler] Volumen: " + volume);

        // 1. El usuario EMPIEZA a hablar
        if (!isTalking && volume > volumeLimit)
        {
            isTalking = true;
            silenceCounter = 0f;
            Debug.Log("[MicrophoneHandler] Voz detectada. Grabando...");

            Microphone.End(microphoneID);
            commandClip = Microphone.Start(microphoneID, false, maxRecordTime, 16000);
        }
        // 2. El usuario hace SILENCIO tras hablar
        else if (isTalking && volume < volumeLimit)
        {
            silenceCounter += Time.deltaTime;

            if (silenceCounter >= maxSilenceTime)
            {
                Debug.Log("[MicrophoneHandler] Voz finalizada. Procesando audio...");
                isTalking = false;
                isProcessing = true;

                int posicionFinal = Microphone.GetPosition(microphoneID);
                Microphone.End(microphoneID);

                ProcessAndEmitAudio(posicionFinal);
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

    private void ProcessAndEmitAudio(int posicionFinal)
    {
        if (posicionFinal > 0)
        {
            AudioClip cutClip = AudioClip.Create("Comando", posicionFinal, commandClip.channels, commandClip.frequency, false);
            float[] datosAudio = new float[posicionFinal * commandClip.channels];
            commandClip.GetData(datosAudio, 0);
            cutClip.SetData(datosAudio, 0);

            byte[] wavData = ConvertToWav(cutClip);

            // Notifica a los suscriptores (AzureManagerSpeech)
            OnSpeechRecorded?.Invoke(wavData);
        }
        else
        {
            ResumeListening();
        }
    }

    public void ResumeListening()
    {
        isProcessing = false;
        StartAmbientMonitorization();
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
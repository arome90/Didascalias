using System;
using System.IO;
using UnityEngine;
using Didascalia;

public class MicrophoneInputHandler : MonoBehaviour
{
    [Header("Configuración de Escucha Libre")]
    [Tooltip("Sensibilidad del micrófono (ej: 0.02). Aumentar si hay ruido de fondo.")]
    public float volumeLimit = 0.02f;

    [Tooltip("Segundos de silencio para asumir que el usuario ha terminado de hablar.")]
    public float maxSilenceTime = 1.5f;

    [Tooltip("Duración máxima de la grabación en segundos.")]
    public int maxRecordTime = 15;

    [Header("Telemetría de Voz")]
    [Tooltip("Activar el envío continuo de métricas de voz a la telemetría.")]
    public bool enableTelemetry = true;

    [Tooltip("Intervalo en segundos para enviar muestras de VoiceData (ej: 0.2s = 5 muestras por segundo).")]
    public float telemetryInterval = 0.2f;

    // Evento que notifica cuando una frase ha sido capturada y convertida a WAV
    public event Action<byte[]> OnSpeechRecorded;

    private string microphoneID;
    private AudioClip monitorClip;
    private AudioClip commandClip;

    private bool isTalking = false;
    private float silenceCounter = 0f;
    private bool isProcessing = false;

    // Buffers y temporizador para la telemetría
    private float telemetryTimer = 0f;
    private readonly float[] telemetrySamples = new float[1024];

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

        // Telemetría periódica de voz (Volumen en dB y Pitch F0 en Hz)
        if (enableTelemetry)
        {
            telemetryTimer += Time.deltaTime;
            if (telemetryTimer >= telemetryInterval)
            {
                telemetryTimer = 0f;
                ProcessAndSendTelemetry();
            }
        }

        // El usuario EMPIEZA a hablar
        if (!isTalking && volume > volumeLimit)
        {
            isTalking = true;
            silenceCounter = 0f;
            Debug.Log("[MicrophoneHandler] Voz detectada. Grabando...");

            Microphone.End(microphoneID);
            commandClip = Microphone.Start(microphoneID, false, maxRecordTime, 16000);
        }
        // El usuario hace SILENCIO tras hablar
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
        // El usuario CONTINÚA hablando
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

    /// <summary>
    /// Procesa el buffer actual del micrófono y envía VoiceData al GameDataManager.
    /// </summary>
    private void ProcessAndSendTelemetry()
    {
        AudioClip activeClip = isTalking ? commandClip : monitorClip;
        if (activeClip == null) return;

        int position = Microphone.GetPosition(microphoneID);
        if (position < telemetrySamples.Length) return;

        int offset = position - telemetrySamples.Length;
        if (offset < 0 || offset + telemetrySamples.Length > activeClip.samples) return;

        try
        {
            activeClip.GetData(telemetrySamples, offset);
        }
        catch
        {
            return;
        }

        // Volumen RMS -> Convertir a Decibelios dBFS
        float rms = 0f;

        // RMS: Se eleva al cuadrado cada muestra, las suma, calcula la media y saca la raíz cuadrada. 
        //      Mide la energía real promedio de la onda de sonido.
        for (int i = 0; i < telemetrySamples.Length; i++)
        {
            rms += telemetrySamples[i] * telemetrySamples[i];
        }
        rms = Mathf.Sqrt(rms / telemetrySamples.Length);

        // Conversión a Decibelios (dBFS).
        // 0 dB es la saturación máxima digital.
        // Silencio o valores extremadamente pequeños (RMS < 0.0001) se fuerzan a -80.0 dB 
        // para evitar que el algoritmo calcule log{10}(0) = -infinito.
        float volumeDb = (rms < 0.0001f) ? -80.0f : 20f * Mathf.Log10(rms);

        // Si el volumen está por debajo de -50 dB (ruido de fondo ambiente), se asigna pitchHz = 0 para evitar falsos positivos
        float pitchHz = 0f;
        if (volumeDb > -50.0f)
        {
            pitchHz = CalculateAutocorrelationPitch(telemetrySamples, activeClip.frequency);
        }

        // Redondeo a 2 decimales para optimizar JSON
        float roundVol = (float)Math.Round(volumeDb, 2);
        float roundPitch = (float)Math.Round(pitchHz, 2);

        // Persistencia
        if (GameDataManager.Instance != null)
        {
            VoiceData voiceSample = new VoiceData(roundVol, roundPitch);
            GameDataManager.Instance.SendData(voiceSample);
        }
    }

    /// <summary>
    /// Estimación de Frecuencia Fundamental (F0) por autocorrelación en el dominio del tiempo.
    /// Acotado a voz humana (80 Hz - 500 Hz).
    /// </summary>
    private float CalculateAutocorrelationPitch(float[] samples, int sampleRate, float minFreq = 80f, float maxFreq = 500f)
    {
        int n = samples.Length;
        int minLag = Mathf.FloorToInt(sampleRate / maxFreq);
        int maxLag = Mathf.FloorToInt(sampleRate / minFreq);

        float maxCorrelation = 0f;
        int bestLag = -1;

        for (int lag = minLag; lag <= maxLag; lag++)
        {
            float sum = 0f;
            for (int i = 0; i < n - lag; i++)
            {
                sum += samples[i] * samples[i + lag];
            }

            if (sum > maxCorrelation)
            {
                maxCorrelation = sum;
                bestLag = lag;
            }
        }

        return (bestLag != -1) ? (float)sampleRate / bestLag : 0f;
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
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Clase para medir variables de voz como volumen y tono (pitch).
/// </summary>
public class VoiceVariables : MonoBehaviour
{
    [SerializeField]
    private int sampleRate = 44100; // Tasa de muestreo del audio.

    [SerializeField]
    private int recordingTime = 1; // Tiempo de grabación en segundos.

    private int bufferLength;

    [SerializeField]
    private AudioSource audioSource;

    float[] audioFragment;
    float[] spectrum;

    [SerializeField]
    private float maxVolume;

    private float lastVolume;

    void Start()
    {
        if (!enabled) return;
        maxVolume = -35.0f;
        audioSource.loop = true;

        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 10000f;

        initializeVariables();
        audioSource.clip = Microphone.Start(null, true, recordingTime, sampleRate);

        StartCoroutine(WaitForMicrophoneAndPlay());
        InvokeRepeating(nameof(AnalyzeVoice), recordingTime, recordingTime);

    }

    private IEnumerator WaitForMicrophoneAndPlay()
    {
        // Espera hasta que el micrófono tenga datos disponibles.
        while (!(Microphone.GetPosition(null) > 0))
        {
            yield return null;
        }
        // Reproduce el audio.
        audioSource.Play();
    }

    /// <summary>
    /// Inicializa las variables de medición para la voz.
    /// </summary>
    private void initializeVariables()
    {
        bufferLength = sampleRate * recordingTime;
        audioFragment = new float[bufferLength];
        spectrum = new float[2048];
    }

    /// <summary>
    /// Analiza la voz capturada por el micrófono, calcula el volumen en decibelios y el tono (pitch).
    /// </summary>
    void AnalyzeVoice()
    {
        // Calcula el nivel de intensidad en decibelios.
        float volumeLevel = CalculateVolumeLevel();
        lastVolume= volumeLevel;
        maxVolume = math.max(volumeLevel, maxVolume);
        // análisis de pitch.
        float dominantFrequency = AnalyzePitch();

        // Muestra el nivel de intensidad en decibelios en consola.
        Debug.Log($"Volume: {volumeLevel}, Dominant Frequency: {dominantFrequency}");

        Microphone.End(null);
        audioSource.clip = Microphone.Start(null, true, recordingTime, sampleRate);
        StartCoroutine(WaitForMicrophoneAndPlay());
        //audioSource.Play();

        SendData(volumeLevel, dominantFrequency);
    }

    /// <summary>
    /// Calcula el nivel de volumen (intensidad sonora) a partir de los datos de audio.
    /// </summary>
    /// <returns>Nivel de volumen en decibelios.</returns>
    float CalculateVolumeLevel()
    {
        audioSource.clip.GetData(audioFragment, 0);
        int audio_length= Mathf.Max(audioFragment.Length, audioSource.clip.samples);
        // Calcula el valor RMS (Root Mean Square) para obtener el nivel de intensidad.
        float rms = 0;
        for (int i = 0; i < audio_length; i++)
        {
            rms += audioFragment[i] * audioFragment[i];
        }
        rms = Mathf.Sqrt(rms / audio_length);
        // Convierte el valor RMS a decibelios.
        float decibels = 20 * Mathf.Log10(rms);
        return decibels;
    }

    /// <summary>
    /// Analiza el tono (frecuencia dominante) en los datos de audio.
    /// </summary>
    /// <returns>Frecuencia dominante en Hz.</returns>
    float AnalyzePitch()
    {
        // Obtiene el espectro de frecuencia.
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        // Encuentra la frecuencia dominante.
        float maxFrequency = 0f;
        int maxIndex = 0;

        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxFrequency)
            {
                maxFrequency = spectrum[i];
                maxIndex = i;
            }
        }
        float delta = 0.0f;
        // Interpolación parabólica para mejorar precisión
        if (maxIndex > 0 && maxIndex < spectrum.Length - 1)
        {
            float left = spectrum[maxIndex - 1];
            float center = spectrum[maxIndex];
            float right = spectrum[maxIndex + 1];
            delta = (left - right) / (2 * (left - 2 * center + right));
        }


        // Calcula la frecuencia dominante en Hz.
        float dominantFrequency = (maxIndex+delta) * AudioSettings.outputSampleRate / spectrum.Length;
        // Retorna la frecuencia dominante.
        return dominantFrequency;
    }

    public void SendData(float v,float p)
    {
        Didascalia.VoiceData d = new Didascalia.VoiceData( v ,p);
        Didascalia.GameDataManager.Instance.SendData(d);
    }
    public void ResetVolume()
    {
        maxVolume = lastVolume;
    }

    private void OnDestroy()
    {
        Microphone.End(null);
    }

}

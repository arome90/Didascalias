using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Clase para medir variables de voz como volumen y tono (pitch).
/// </summary>
public class VoiceVariables : MonoBehaviour
{
    private int sampleRate = 44100; // Tasa de muestreo del audio.
    private int recordingTime = 1; // Tiempo de grabación en segundos.
    private int bufferLength;
    public AudioSource audioSource;
    float[] audioFragment;
    float[] spectrum;
    void Start()
    {
        audioSource.loop = true;
        initializeVariables();
        audioSource.clip = Microphone.Start(null, true, recordingTime, sampleRate);
        // Invocar la función de análisis de voz periódicamente.
        InvokeRepeating(nameof(AnalyzeVoice), recordingTime, recordingTime);
    }

    /// <summary>
    /// Inicializa las variables de medición para la voz.
    /// </summary>
    private void initializeVariables()
    {
        bufferLength = sampleRate * recordingTime;
        audioFragment = new float[bufferLength];
        spectrum = new float[1024];
        //Todo lk: luego si eso, carga datos desde json
    }

    /// <summary>
    /// Analiza la voz capturada por el micrófono, calcula el volumen en decibelios y el tono (pitch).
    /// </summary>
    void AnalyzeVoice()
    {
        Microphone.End(null);
        // Calcula el nivel de intensidad en decibelios.
        float volumeLevel = CalculateVolumeLevel();

        // análisis de pitch.
        float dominantFrequency = AnalyzePitch();
        // Muestra el nivel de intensidad en decibelios en consola.
        Debug.Log($"Volume: {volumeLevel}, Dominant Frequency: {dominantFrequency}");
        audioSource.clip = Microphone.Start(null, true, recordingTime, sampleRate);

    }

    /// <summary>
    /// Calcula el nivel de volumen (intensidad sonora) a partir de los datos de audio.
    /// </summary>
    /// <returns>Nivel de volumen en decibelios.</returns>
    float CalculateVolumeLevel()
    {
        audioSource.clip.GetData(audioFragment, 0);
        int audio_length= math.max(audioFragment.Length, audioSource.clip.samples);
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
            if (audioFragment[i] > maxFrequency)
            {
                maxFrequency = audioFragment[i];
                maxIndex = i;
            }
        }
        // Calcula la frecuencia dominante en Hz.
        float dominantFrequency = maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length;
        // Retorna la frecuencia dominante.
        return dominantFrequency;
    }

    private void OnDestroy()
    {
        Microphone.End(null);
    }
}

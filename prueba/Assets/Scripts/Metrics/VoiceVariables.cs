//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// Clase para medir variables de voz como volumen y tono (pitch).
///// </summary>
//public class VoiceVariables : MonoBehaviour
//{
//    [SerializeField] private int sampleRate = 44100; // Tasa de muestreo del audio.
//    [SerializeField] private int recordingTime = 5; // Tiempo de grabación en segundos.

//    private VariableMeasurement volumen; // Medición del volumen.
//    private VariableMeasurement pitch; // Medición del tono.
//    void Start()
//    {
//        // Invocar la función de análisis de voz periódicamente.
//        // InvokeRepeating(nameof(AnalyzeVoice), 10, 5);
//    }

//    /// <summary>
//    /// Inicializa las variables de medición para la voz.
//    /// </summary>
//    public void initializeVariables()
//    {
//        volumen = new VariableMeasurement();
//    }

//    /// <summary>
//    /// Analiza la voz capturada por el micrófono, calcula el volumen en decibelios y el tono (pitch).
//    /// </summary>
//    void AnalyzeVoice()
//    {
//        // Inicia la grabación del micrófono.
//        AudioClip audioClip = Microphone.Start(null, true, recordingTime, sampleRate);
//        while (Microphone.GetPosition(null) <= 0) { } // Espera hasta que se capture algún audio.

//        // Crea un arreglo de muestras para almacenar los datos del audio.
//        float[] audioData = new float[audioClip.samples];
//        audioClip.GetData(audioData, 0);

//        // Calcula el nivel de intensidad en decibelios.
//        float volumeLevel = CalculateVolumeLevel(audioData);

//        // Comentado temporalmente: análisis de pitch.
//        // AudioSource audioAux = new AudioSource();
//        // audioAux.clip = audioClip;
//        // float pitchLevel = AnalyzePitch(audioAux, sampleRate);

//        // Detiene la grabación del micrófono.
//        Microphone.End(null);

//        // Muestra el nivel de intensidad en decibelios en consola.
//        Debug.Log("Volume Level (dB): " + volumeLevel);
//    }

//    /// <summary>
//    /// Calcula el nivel de volumen (intensidad sonora) a partir de los datos de audio.
//    /// </summary>
//    /// <param name="audioData">Arreglo de datos del audio grabado.</param>
//    /// <returns>Nivel de volumen en decibelios.</returns>
//    float CalculateVolumeLevel(float[] audioData)
//    {
//        // Calcula el valor RMS (Root Mean Square) para obtener el nivel de intensidad.
//        float rms = 0;

//        for (int i = 0; i < audioData.Length; i++)
//        {
//            rms += audioData[i] * audioData[i];
//        }

//        rms = Mathf.Sqrt(rms / audioData.Length);

//        // Convierte el valor RMS a decibelios.
//        float decibels = 20 * Mathf.Log10(rms);

//        return decibels;
//    }

//    /// <summary>
//    /// Analiza el tono (frecuencia dominante) en los datos de audio.
//    /// </summary>
//    /// <param name="audioSource">Fuente de audio que contiene el clip grabado.</param>
//    /// <param name="sampleSize">Número de muestras a analizar.</param>
//    /// <returns>Frecuencia dominante en Hz.</returns>
//    float AnalyzePitch(AudioSource audioSource, int sampleSize)
//    {
//        float[] spectrumData = new float[sampleSize];

//        // Obtiene el espectro de frecuencia.
//        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

//        // Encuentra la frecuencia dominante.
//        float maxFrequency = 0f;
//        int maxIndex = 0;

//        for (int i = 0; i < sampleSize; i++)
//        {
//            if (spectrumData[i] > maxFrequency)
//            {
//                maxFrequency = spectrumData[i];
//                maxIndex = i;
//            }
//        }

//        // Calcula la frecuencia dominante en Hz.
//        float dominantFrequency = maxIndex * AudioSettings.outputSampleRate / 2 / sampleSize;

//        // Retorna la frecuencia dominante.
//        return dominantFrequency;
//    }
//}

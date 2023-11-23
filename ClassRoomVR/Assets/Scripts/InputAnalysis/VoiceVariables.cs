using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
/// <summary>
/// Agregar las variables de voz que podamos medir 
/// </summary>
public class VoiceVariables : MonoBehaviour
{
    [SerializeField ]private int sampleRate = 44100; // Tasa de muestreo del audio
    [SerializeField] private int recordingTime = 5; // Tiempo de grabación en segundos


    private VariableMeasurement volumen;
    private VariableMeasurement pitch;


    void Start()
    {
        //AnalyzeVoiceVolume();
    }

    public void initializeVariables()
    {
        volumen = new VariableMeasurement(sampleRate);
    }

    void AnalyzeVoice()
    {
        // Inicia la grabación del micrófono
        AudioClip audioClip = Microphone.Start(null, true, recordingTime, sampleRate);
        while (Microphone.GetPosition(null) <= 0) { } // Espera hasta que se capture algún audio

        // Crea un arreglo de muestras para almacenar los datos del audio
        float[] audioData = new float[audioClip.samples];
        audioClip.GetData(audioData, 0);

        // Calcula el nivel de intensidad en decibelios
        float volumeLevel = CalculateVolumeLevel(audioData);

        AudioSource audioAux = new AudioSource();
        audioAux.clip = audioClip;
        float pitchLevel = AnalyzePitch(audioAux, sampleRate);
        
        // Detén la grabación del micrófono
        Microphone.End(null);

        // Muestra el nivel de intensidad en decibelios
        Debug.Log("Volume Level (dB): " + volumeLevel);
    }

    float CalculateVolumeLevel(float[] audioData)
    {
        // Calcula el valor RMS (Root Mean Square) para obtener el nivel de intensidad
        float rms = 0;

        for (int i = 0; i < audioData.Length; i++)
        {
            rms += audioData[i] * audioData[i];
        }

        rms = Mathf.Sqrt((float)(rms / audioData.Length));

        // Convierte el valor RMS a decibelios
        float decibels = 20 * Mathf.Log10(rms);

        return decibels;
    }

    float AnalyzePitch(AudioSource audioSource, int sampleSize)
    {
        float[] spectrumData = new float[sampleSize];

        // Obtiene el espectro de frecuencia
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        // Encuentra la frecuencia dominante
        float maxFrequency = 0f;
        int maxIndex = 0;

        for (int i = 0; i < sampleSize; i++)
        {
            if (spectrumData[i] > maxFrequency)
            {
                maxFrequency = spectrumData[i];
                maxIndex = i;
            }
        }

        // Calcula la frecuencia dominante en Hz
        float dominantFrequency = maxIndex * AudioSettings.outputSampleRate / 2 / sampleSize;

        // Muestra la frecuencia dominante
       return dominantFrequency;
    }

}

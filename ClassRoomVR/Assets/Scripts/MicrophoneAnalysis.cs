using UnityEngine;
public class MicrophoneAnalysis : MonoBehaviour
{
    public float volumeThreshold = 0.1f; // Umbral de volumen para detectar sonido
    public float pitchThreshold = 1000f; // Umbral de tono para detectar cambios de tono


    AudioClip audioClip;
    float[] _samples;
    private float[] _spectrum;
    void Start()
    {
        string micDevice = Microphone.devices[0];
        audioClip = Microphone.Start(micDevice, true, 1, AudioSettings.outputSampleRate);
        while (!(Microphone.GetPosition(null) > 0)) { } // Esperar a que se inicialice el micrófono

        // Obtener los datos de audio del micrófono una vez para descartar el primer bloque de datos no confiables
        //float[] samples = new float[audioClip.samples];
        //audioClip.GetData(samples, 0);

        _samples = new float[audioClip.samples];
        _spectrum = new float[audioClip.samples];
    }

    public float rmsVal;
    public float dbVal;
    public float pitchVal;

    void Update()
    {

        AnalyzeSound();

        Debug.Log("RMS: " + rmsVal.ToString("F2"));
        Debug.Log(dbVal.ToString("F1") + " dB");
        Debug.Log(pitchVal.ToString("F0") + " Hz");
        //// Obtener los datos de audio del micrófono
        //float[] samples = new float[audioClip.samples];
        //audioClip.GetData(samples, 0);



        //// Aplicar la transformada de Fourier (FFT) para analizar el espectro de frecuencia
        //float[] spectrum = new float[64];
        //AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        //// Encontrar la frecuencia dominante (tono)
        //float maxAmplitude = 0f;
        //int maxIndex = 0;
        //for (int i = 0; i < spectrum.Length; i++)
        //{
        //    if (spectrum[i] > maxAmplitude)
        //    {
        //        maxAmplitude = spectrum[i];
        //        maxIndex = i;
        //    }
        //}
        //float dominantFrequency = maxIndex * AudioSettings.outputSampleRate / spectrum.Length;

        //// Verificar si el tono supera el umbral
        //if (dominantFrequency > pitchThreshold)
        //{
        //    Debug.Log("¡Cambio de tono detectado!");
        //}
    }




    void AnalyzeSound()
    {
       
        const float RefValue = 0.1f;
         const float Threshold = 0.02f;

         audioClip.GetData(_samples, 0); // fill array with samples
       float QSamples = _samples.Length;
        int i;
        float sum = 0;
        for (i = 0; i < QSamples; i++)
        {
            sum += _samples[i] * _samples[i]; // sum squared samples
        }
        rmsVal = Mathf.Sqrt(sum / QSamples); // rms = square root of average
        dbVal = 20 * Mathf.Log10(rmsVal / RefValue); // calculate dB
        if (dbVal < -160) dbVal = -160; // clamp it to -160dB min
                                        // get sound spectrum
        AudioListener.GetSpectrumData(_spectrum, 0, FFTWindow.BlackmanHarris);
        float maxV = 0;
        var maxN = 0;
        for (i = 0; i < QSamples; i++)
        { // find max 
            if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                continue;

            maxV = _spectrum[i];
            maxN = i; // maxN is the index of max
        }
        float freqN = maxN; // pass the index to a float variable
        if (maxN > 0 && maxN < QSamples - 1)
        { // interpolate index using neighbours
            var dL = _spectrum[maxN - 1] / _spectrum[maxN];
            var dR = _spectrum[maxN + 1] / _spectrum[maxN];
            freqN += 0.5f * (dR * dR - dL * dL);
        }
        pitchVal = freqN * (AudioSettings.outputSampleRate / 2) / QSamples; // convert index to frequency
    }

    private float GetAmplitude(float[] samples)
    {
        // Calcular y devolver la amplitud promedio de los valores de muestra
        float sum = 0f;
        foreach (float sample in samples)
        {
            sum += Mathf.Abs(sample);
        }
        return sum / samples.Length;
    }
}

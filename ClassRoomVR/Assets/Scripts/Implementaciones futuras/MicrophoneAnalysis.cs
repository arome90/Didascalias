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

        //Debug.Log("RMS: " + rmsVal.ToString("F2"));
        //Debug.Log(dbVal.ToString("F1") + " dB");
        //Debug.Log(pitchVal.ToString("F0") + " Hz");
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


//using UnityEngine;
//using NAudio.Wave;
//using NAudio.Wave.SampleProviders;
//using NAudio.Dsp;

//public class MicrophoneAnalysis : MonoBehaviour
//{
//    private int sampleRate = 44100;
//    private int bufferSize = 1024;
//    private WaveInEvent waveIn;
//    private float previousPitch = 0f;
//    private float frequencyVariability = 0f;

//    private void Start()
//    {
//        waveIn = new WaveInEvent();
//        waveIn.BufferMilliseconds = (int)((double)bufferSize / (double)sampleRate * 1000.0);
//        waveIn.DataAvailable += WaveIn_DataAvailable;
//        Debug.Log("hak");

//        waveIn.StartRecording();
//    }

//    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
//    {
//        Debug.Log("hik");
//        float[] audioBuffer = new float[e.BytesRecorded / 4]; // 4 bytes per float
//        for (int i = 0; i < audioBuffer.Length; i++)
//        {
//            audioBuffer[i] = System.BitConverter.ToSingle(e.Buffer, i * 4);
//        }

//        float pitch = DetectPitch(audioBuffer);
//        float rms = CalculateRMS(audioBuffer);

//        float db = 20f * Mathf.Log10(rms);
//        Debug.Log("Microphone Detected Pitch: " + pitch.ToString("F2") + " Hz");
//        Debug.Log("Microphone RMS Amplitude: " + rms.ToString("F2"));
//        Debug.Log("Microphone Decibels: " + db.ToString("F2"));

//        CalculateFrequencyVariability(pitch);
//        Debug.Log("Microphone Frequency Variability: " + frequencyVariability.ToString("F2"));
//    }

//    private float DetectPitch(float[] audioBuffer)
//    {
//        // Implement pitch detection algorithm using FFT or other methods
//        // Return the detected pitch frequency in Hz
//        return YourPitchDetectionAlgorithm(audioBuffer);
//    }

//    private float CalculateRMS(float[] samples)
//    {
//        double sum = 0;
//        foreach (var sample in samples)
//        {
//            sum += sample * sample;
//        }
//        double mean = sum / samples.Length;
//        return Mathf.Sqrt((float)mean);
//    }

//    private void CalculateFrequencyVariability(float currentPitch)
//    {
//        frequencyVariability = Mathf.Abs(currentPitch - previousPitch);
//        previousPitch = currentPitch;
//    }

//    private float YourPitchDetectionAlgorithm(float[] audioBuffer)
//    {
//        int maxSamples = audioBuffer.Length;

//        Complex[] fftBuffer = new Complex[bufferSize];
//        for (int i = 0; i < bufferSize; i++)
//        {
//            fftBuffer[i].X = audioBuffer[i];
//            fftBuffer[i].Y = 0;

//        }

//        FastFourierTransform.FFT(true, (int)Mathf.Log(bufferSize, 2), fftBuffer);

//        // Find the peak frequency index in the spectrum
//        int peakIndex = FindPeakIndex(fftBuffer);

//        // Convert the index to frequency in Hz
//        float detectedFrequency = peakIndex * sampleRate / bufferSize;

//        return detectedFrequency;
//    }


//    private int FindPeakIndex(Complex[] spectrum)
//    {
//        int peakIndex = 0;
//        double maxMagnitude = 0;

//        for (int i = 0; i < spectrum.Length; i++)
//        {
//            double magnitude = Mathf.Sqrt(spectrum[i].X * spectrum[i].X + spectrum[i].Y * spectrum[i].Y);
//            if (magnitude > maxMagnitude)
//            {
//                maxMagnitude = magnitude;
//                peakIndex = i;
//            }
//        }

//        return peakIndex;
//    }
//    private void OnDisable()
//    {
//        if (waveIn != null)
//        {
//            waveIn.StopRecording();
//            waveIn.Dispose();
//        }
//    }
//}

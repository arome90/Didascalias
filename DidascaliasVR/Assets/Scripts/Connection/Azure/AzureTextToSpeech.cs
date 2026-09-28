using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AzureTextToSpeech : Singleton<AzureTextToSpeech>
{
#if UNITY_EDITOR
    [SerializeField]
    private bool _worksOnDebug = false;
#endif

    public string SpeakTextEditor = "";

    private string mascVoice = "es-ES-TristanMultilingualNeural";
    private string femVoice_1 = "es-ES-ElviraNeural";
    private string femVoice_2 = "es-ES-XimenaNeural";

    private const int SampleRate = 8000;

    private const string subscriptionKey = "2RCbXjGc1xCelZAa1NXghhY5miqkASeHN7e5TPmdwaUPgUez2iXzJQQJ99CGAC5RqLJXJ3w3AAAAACOGHkJH";

    public async Task Speak(string what, Gender gender, AudioSource source)
    {
#if UNITY_EDITOR
        if (!_worksOnDebug) return;
#endif 
        Uri endpoint = new Uri("https://didascalia-tts-spanish-resource.cognitiveservices.azure.com/");

        // var config = SpeechConfig.FromEndpoint(endpoint, subscriptionKey);

        var config = SpeechConfig.FromEndpoint(endpoint, subscriptionKey);
        if (gender == Gender.Girl) config.SpeechSynthesisVoiceName = (UnityEngine.Random.Range(0, 2) == 0) ? femVoice_1 : femVoice_2;
        else config.SpeechSynthesisVoiceName = mascVoice;

        // 1. Forzamos un formato de audio RIFF PCM de 24kHz y 16 bits mono
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Riff8Khz16BitMonoPcm);

        using (var synthesizer = new SpeechSynthesizer(config, null))
        {
            using (var result = await synthesizer.SpeakTextAsync(what))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    byte[] audioBytes = result.AudioData;

                    AudioClip clip = ConvertPcmToAudioClip(audioBytes, "AzureTTS_Clip");

                    if (clip != null && source != null)
                    {
                        source.clip = clip;
                        source.Play();
                    }

                    Console.WriteLine($"Speech synthesized for text [{what}]");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Convierte bytes en formato WAV/PCM (16-bit Mono) a un AudioClip de Unity.
    /// </summary>
    private AudioClip ConvertPcmToAudioClip(byte[] wavBytes, string clipName)
    {
        // La cabecera WAV estándar ocupa los primeros 44 bytes.
        int headerSize = 44;

        if (wavBytes == null || wavBytes.Length <= headerSize)
        {
            Debug.LogWarning("Los datos de audio recibidos están vacíos o corruptos.");
            return null;
        }

        // Calculamos la cantidad de muestras (cada muestra de 16 bits = 2 bytes)
        int pcmLength = wavBytes.Length - headerSize;
        int sampleCount = pcmLength / 2;

        float[] samples = new float[sampleCount];

        // Convertimos cada par de bytes (Int16) a float normalizado (-1.0f a 1.0f)
        for (int i = 0; i < sampleCount; i++)
        {
            short pcmSample = BitConverter.ToInt16(wavBytes, headerSize + (i * 2));
            samples[i] = (pcmSample / 32768f) * 2;
        }

        // Creamos el AudioClip (1 canal = Mono)
        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AzureTextToSpeech))]
public class AzureTextToSpeechEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Referencia al script original
        AzureTextToSpeech script = (AzureTextToSpeech)target;
        
        if (GUILayout.Button("Try"))
        {
            Student st = StudentManager.Instance.TryGetStudentByNameOrGetRandom(null);
            script.Speak(script.SpeakTextEditor, st.Gender, st._audioSource);
        }

        // Dibuja el resto de variables públicas por defecto si las hubiera
        DrawDefaultInspector();
    }
}
#endif
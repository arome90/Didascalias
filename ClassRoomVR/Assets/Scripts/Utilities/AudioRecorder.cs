using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Linq;

public static class AudioRecorder
{
    #region Constantes y Variables Estáticas

    /// <summary>
    /// Las muestras son floats que van de -1.0f a 1.0f, representando los datos en el audio clip.
    /// </summary>
    private static float[] _samplesData;

    /// <summary>
    /// Tamaño del encabezado del archivo WAV.
    /// </summary>
    private const int HEADER_SIZE = 44;

    private static AudioClip _recording;
    private static bool _isRecording = false;

    #endregion

    /// <summary>
    /// Comienza la grabación de audio desde el micrófono.
    /// </summary>
    public static void StartRecording()
    {
        _isRecording = true;

        // Detiene cualquier grabación anterior antes de comenzar una nueva.
        Microphone.End(Microphone.devices[0]);
        _recording = Microphone.Start(Microphone.devices[0], false, 600, 44100);
    }

    /// <summary>
    /// Guarda la grabación actual en un archivo WAV.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a guardar.</param>
    public static void SaveRecording(string fileName = "Audio")
    {
        if (_isRecording)
        {
            // Espera a que el micrófono esté listo.
            while (!(Microphone.GetPosition(Microphone.devices[0]) > 0)) { }

            _samplesData = new float[_recording.samples * _recording.channels];
            _recording.GetData(_samplesData, 0);

            // Recorta el silencio al final de la grabación.
            var samples = _samplesData.ToList();
            int recordedSamples = (int)(_samplesData.Length * (Time.timeSinceLevelLoad / 600f));

            if (recordedSamples < _samplesData.Length - 1)
            {
                samples.RemoveRange(recordedSamples, _samplesData.Length - recordedSamples);
                _samplesData = samples.ToArray();
            }

            // Crea un nuevo clip de audio con los datos de las muestras recortadas.
            AudioClip audioClip = AudioClip.Create(fileName, _samplesData.Length, _recording.channels, 44100, false);
            audioClip.SetData(_samplesData, 0);

            string filePath = Path.Combine(Application.persistentDataPath,
                $"{fileName} {DateTime.UtcNow:yyyy_MM_dd HH_mm_ss_ffff}.wav");

            // Si el archivo ya existe, se elimina antes de guardar el nuevo.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            try
            {
                WriteWAVFile(audioClip, filePath);
                Debug.Log("Archivo guardado correctamente en " + filePath);
            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("¡No se encontró el directorio de datos persistentes!");
            }

            _isRecording = false;
            Microphone.End(Microphone.devices[0]);
        }
    }

    /// <summary>
    /// Convierte un archivo WAV en un arreglo de bytes.
    /// </summary>
    /// <param name="filePath">Ruta del archivo WAV.</param>
    /// <returns>Arreglo de bytes que representa el archivo WAV.</returns>
    public static byte[] ConvertWAVtoByteArray(string filePath)
    {
        byte[] bytes = new byte[File.ReadAllBytes(filePath).Length];
        using (FileStream fs = File.OpenRead(filePath))
        {
            fs.Read(bytes, 0, bytes.Length);
        }
        return bytes;
    }

    /// <summary>
    /// Escribe un archivo WAV con los datos del AudioClip.
    /// </summary>
    /// <param name="clip">AudioClip a guardar.</param>
    /// <param name="filePath">Ruta donde se guardará el archivo.</param>
    private static void WriteWAVFile(AudioClip clip, string filePath)
    {
        float[] clipData = new float[clip.samples];

        using (Stream fs = File.Create(filePath))
        {
            int frequency = clip.frequency;
            int numOfChannels = clip.channels;
            int samples = clip.samples;
            fs.Seek(0, SeekOrigin.Begin);

            // Escribir el encabezado WAV.
            WriteWAVHeader(fs, frequency, numOfChannels, samples);

            // Escribir los datos de audio.
            clip.GetData(clipData, 0);
            short[] intData = new short[clipData.Length];
            byte[] bytesData = new byte[clipData.Length * 2];

            int conversionFactor = 32767;

            for (int i = 0; i < clipData.Length; i++)
            {
                intData[i] = (short)(clipData[i] * conversionFactor);
                byte[] byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            fs.Write(bytesData, 0, bytesData.Length);
        }
    }

    /// <summary>
    /// Escribe el encabezado del archivo WAV en el flujo de salida.
    /// </summary>
    /// <param name="fs">Flujo de salida donde escribir el encabezado.</param>
    /// <param name="frequency">Frecuencia de muestreo del audio.</param>
    /// <param name="numOfChannels">Número de canales del audio.</param>
    /// <param name="samples">Número de muestras del audio.</param>
    private static void WriteWAVHeader(Stream fs, int frequency, int numOfChannels, int samples)
    {
        // Chunk ID
        byte[] riff = Encoding.ASCII.GetBytes("RIFF");
        fs.Write(riff, 0, 4);

        // ChunkSize
        byte[] chunkSize = BitConverter.GetBytes((HEADER_SIZE + samples * numOfChannels * 2) - 8);
        fs.Write(chunkSize, 0, 4);

        // Format
        byte[] wave = Encoding.ASCII.GetBytes("WAVE");
        fs.Write(wave, 0, 4);

        // Subchunk1ID
        byte[] fmt = Encoding.ASCII.GetBytes("fmt ");
        fs.Write(fmt, 0, 4);

        // Subchunk1Size
        byte[] subChunk1 = BitConverter.GetBytes(16);
        fs.Write(subChunk1, 0, 4);

        // AudioFormat
        byte[] audioFormat = BitConverter.GetBytes(1);
        fs.Write(audioFormat, 0, 2);

        // NumChannels
        byte[] numChannels = BitConverter.GetBytes(numOfChannels);
        fs.Write(numChannels, 0, 2);

        // SampleRate
        byte[] sampleRate = BitConverter.GetBytes(frequency);
        fs.Write(sampleRate, 0, 4);

        // ByteRate
        byte[] byteRate = BitConverter.GetBytes(frequency * numOfChannels * 2);
        fs.Write(byteRate, 0, 4);

        // BlockAlign
        ushort blockAlign = (ushort)(numOfChannels * 2);
        fs.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        // BitsPerSample
        ushort bps = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fs.Write(bitsPerSample, 0, 2);

        // Subchunk2ID
        byte[] datastring = Encoding.ASCII.GetBytes("data");
        fs.Write(datastring, 0, 4);

        // Subchunk2Size
        byte[] subChunk2 = BitConverter.GetBytes(samples * numOfChannels * 2);
        fs.Write(subChunk2, 0, 4);
    }
}

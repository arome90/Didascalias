using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Linq;

public static class AudioRecorder
{
    #region Constants &  Static Variables

   
    /// <summary>
    /// The samples are floats ranging from -1.0f to 1.0f, representing the data in the audio clip
    /// </summary>
    static float[] samplesData;
    /// <summary>
    /// WAV file header size
    /// </summary>
    const int HEADER_SIZE = 44;

    static AudioClip recording;
    static bool isRecording = false;

    #endregion

    //public static void StartRecording()
    //{
    //    recording = Microphone.Start(Microphone.devices[0], true, 4, 44100);

    //}


    //public static void StopRecordingAndSave()
    //{
    //    //Save();

    //    Microphone.End(null); // Detiene la grabación

    //    string fileName = "recording_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".wav"; // Genera un nombre de archivo único basado en la fecha y hora actual
    //    string filePath = Path.Combine(Application.persistentDataPath, fileName); // Combina la ruta de almacenamiento con el nombre de archivo generado

    //    SavWav.Save(filePath, recording); // Guarda el archivo de audio en la ruta especificada


    //}







    public static void StartRecording()
    {
        //recordingTime = 0f;
        isRecording = true;


        Microphone.End(Microphone.devices[0]);
        recording = Microphone.Start(Microphone.devices[0], false, 600, 44100);
    }


    public static void SaveRecording(string fileName = "Audio")
    {
        if (isRecording)
        {

            while (!(Microphone.GetPosition(null) > 0)) { }
            samplesData = new float[recording.samples * recording.channels];
            recording.GetData(samplesData, 0);

            // Trim the silence at the end of the recording
            var samples = samplesData.ToList();
            int recordedSamples = (int)(samplesData.Length * (Time.timeSinceLevelLoad / (float)600));

            if (recordedSamples < samplesData.Length - 1)
            {
                samples.RemoveRange(recordedSamples, samplesData.Length - recordedSamples);
                samplesData = samples.ToArray();
            }

            // Create the audio file after removing the silence
            AudioClip audioClip = AudioClip.Create(fileName, samplesData.Length, recording.channels, 44100, false);
            audioClip.SetData(samplesData, 0);

            string filePath = Path.Combine(Application.persistentDataPath, fileName + " " + DateTime.UtcNow.ToString("yyyy_MM_dd HH_mm_ss_ffff") + ".wav");

            // Delete the file if it exists.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            try
            {
                WriteWAVFile(audioClip, filePath);
                Debug.Log("File Saved Successfully at " + filePath);
            }
            catch (DirectoryNotFoundException)
            {
                Debug.LogError("Persistent Data Path not found!");
            }

            isRecording = false;
            Microphone.End(Microphone.devices[0]);
        }
    }

    public static byte[] ConvertWAVtoByteArray(string filePath)
    {
        //Open the stream and read it back.
        byte[] bytes = new byte[recording.samples + HEADER_SIZE];
        using (FileStream fs = File.OpenRead(filePath))
        {
            fs.Read(bytes, 0, bytes.Length);
        }
        return bytes;
    }

    static void WriteWAVFile(AudioClip clip, string filePath)
    {
        float[] clipData = new float[clip.samples];

        //Create the file.
        using (Stream fs = File.Create(filePath))
        {
            int frequency = clip.frequency;
            int numOfChannels = clip.channels;
            int samples = clip.samples;
            fs.Seek(0, SeekOrigin.Begin);

            //Header

            // Chunk ID
            byte[] riff = Encoding.ASCII.GetBytes("RIFF");
            fs.Write(riff, 0, 4);

            // ChunkSize
            byte[] chunkSize = BitConverter.GetBytes((HEADER_SIZE + clipData.Length) - 8);
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
            byte[] byteRate = BitConverter.GetBytes(frequency * numOfChannels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
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

            // Data

            clip.GetData(clipData, 0);
            short[] intData = new short[clipData.Length];
            byte[] bytesData = new byte[clipData.Length * 2];

            int convertionFactor = 32767;

            for (int i = 0; i < clipData.Length; i++)
            {
                intData[i] = (short)(clipData[i] * convertionFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            fs.Write(bytesData, 0, bytesData.Length);
        }
    }


}
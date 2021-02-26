using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneManager : MonoBehaviour
{
    private static AudioClip audioClip;
    private static string _device;


    public static int SAMPLERATE = 88200;
    public int deviceNumber = 0;

    public static AudioClip AudioClip
    {
        get
        {
            return audioClip;
        }

        set
        {
            audioClip = value;
        }
    }

    private void OnEnable()
    {
        InitMic();
        Debug.Log("Recording from: " + _device);
    }
    private void OnDisable()
    {
        StopMic();
    }
    private void OnDestroy()
    {
        StopMic();
    }
    void InitMic()
    {
        if (_device == null) _device = Microphone.devices[deviceNumber];
        AudioClip = Microphone.Start(_device, true, 999, SAMPLERATE);
    }
    void StopMic()
    {
        Microphone.End(_device);
    }
    public static int GetMicrophonePosition()
    {
        if (_device == null) return -1;
        return Microphone.GetPosition(_device);
    }
}
using ClassRoomVR;
using MathNet.Numerics.Statistics;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class VoiceVariablesWit : MonoBehaviour
{
    [SerializeField] 
    AppVoiceExperience appVoiceExperience;
    private float lastTime;
    private float accumulatedVolume;
    private float lastVolume;
    private float maxVolume;
   
    public float timer;


    void Awake()
    {
        lastTime = Time.realtimeSinceStartup;
        accumulatedVolume = 0.0f;
        lastVolume = -35.0f;
        maxVolume = -35.0f;
    }

    void Start()
    {
        if (appVoiceExperience == null || !appVoiceExperience.enabled || !appVoiceExperience.gameObject.activeSelf)
        {
            this.enabled = false;
            Debug.LogError("voiceActivation not found or not enabled");
            return;
        }

        appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener((value) =>
        {
            OnMicLevelChanged(value);
        });

    }


    public void OnMicLevelChanged(float a)
    {
        float dB = 20 * Mathf.Log10(a);  //LUFS
        //Debug.Log("Volumen de voz: " + dB + " dB");
        accumulatedVolume += (Time.realtimeSinceStartup - lastTime) * lastVolume;
        lastTime = Time.realtimeSinceStartup;
        maxVolume = math.max(maxVolume, dB);
        lastVolume = dB;
    }

    public void ResetVolume()
    {
        lastTime = Time.realtimeSinceStartup;
        accumulatedVolume = 0.0f;
        maxVolume = lastVolume;
    }

    public float AccumulatedVolume
    {
        get { return accumulatedVolume; }
        private set { accumulatedVolume = value; }
    }

    public float LastVolume
    {
        get { return lastVolume; }
        private set { lastVolume = value; }
    }

    public float MaxVolume
    {
        get { return maxVolume; }
        private set { maxVolume = value; }
    }

}

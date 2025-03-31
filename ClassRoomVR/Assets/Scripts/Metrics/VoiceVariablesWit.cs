using ClassRoomVR;
using MathNet.Numerics.Statistics;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace ClassRoomVR
{
    public class VoiceVariablesWit : MonoBehaviour
    {
        [SerializeField]
        AppVoiceExperience appVoiceExperience;
        private float lastTime;
        private float accumulatedVolume;
        private float lastVolume;
        private float maxVolume;
        private float volumeToSend;

        [SerializeField]
        private float sentTime = 1.0f;


        void Awake()
        {
            lastTime = Time.realtimeSinceStartup;
            accumulatedVolume = 0.0f;
            lastVolume = -35.0f;
            maxVolume = -35.0f;
            volumeToSend = -35.0f;
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
            InvokeRepeating(nameof(SendData), sentTime, sentTime);

        }


        public void OnMicLevelChanged(float a)
        {
            float dB = 20 * Mathf.Log10(a);  //LUFS
                                             //Debug.Log("Volumen de voz: " + dB + " dB");
            accumulatedVolume += (Time.realtimeSinceStartup - lastTime) * lastVolume;
            lastTime = Time.realtimeSinceStartup;
            maxVolume = math.max(maxVolume, dB);
            volumeToSend = math.max(volumeToSend, dB);
            lastVolume = dB;
        }

        public void ResetVolume()
        {
            lastTime = Time.realtimeSinceStartup;
            accumulatedVolume = 0.0f;
            maxVolume = lastVolume;
        }

        public void SendData()
        {
            VoiceData d = new VoiceData(lastVolume, 0);
            GameDataManager.Instance.SendData(d);
        }


        public float MaxVolume
        {
            get { return maxVolume; }
            private set { maxVolume = value; }
        }

    }

}

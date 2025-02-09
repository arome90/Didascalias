using UnityEngine;
using System.Linq;
namespace ClassRoomVR
{

    public class ToneAnalyzer : MonoBehaviour
    {
        public float silenceThreshold = 15.0f;
        public float whisperThreshold = 20f;
        public float normalThreshold = 40f;
        public float shoutThreshold = 70f;

        public float targetTime = 10.0f; // Time in seconds to reach
        private float timeCounter = 0.0f; // Time counter
        private float silencetimeCounter = 0.0f; // Time counter

        private float maxTone = 0;

        [SerializeField]
        private VoiceActivation voiceActivation;
        [SerializeField]
        private ExternalForceManager externalForceManager;

        void Start()
        {

        }

        void Update()
        {
            timeCounter += Time.deltaTime;
            float dB = voiceActivation.getLevelAudio();
            maxTone = Mathf.Max(maxTone, dB);
            if (dB < silenceThreshold)
            {
                silencetimeCounter += Time.deltaTime;
            }
            else
            {
                silencetimeCounter = 0;
            }
            if(silencetimeCounter > targetTime)
            {
                silencetimeCounter = 0;
                timeCounter = 0;
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                Debug.Log("Silencio");

            }
            else if(timeCounter> targetTime)
            {
                timeCounter = 0;
                ClasificarTono(maxTone);
                maxTone = 0;
            }
        }

        void ClasificarTono(double dB)
        {
            if (dB < silenceThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                Debug.Log("Silencio");
            }
            else if (dB < whisperThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooQuiet);
                Debug.Log("Susurro");
            }
            else if (dB < normalThreshold)
            {
                Debug.Log("Normal");
            }
            else
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooLoud);
                Debug.Log("Grito");
            }
          
        }


    }
}

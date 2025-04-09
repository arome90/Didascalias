using Meta.WitAi.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Extensions;

namespace ClassRoomVR
{
    public class VoiceEvent : MonoBehaviour
    {
        [SerializeField]
        private VoiceVariables voiceVariables;
        [SerializeField]
        private VoiceVariablesWit voiceVariablesWit;

        [SerializeField]
        private bool usemicrophone = true;

        [SerializeField]
        private ExternalForceManager externalForceManager;


        [SerializeField]
        private float silenceThreshold = -45.0f;
        [SerializeField]
        private float whisperThreshold = -32.0f; // Umbral de susurro en dB
        [SerializeField]
        private float normalThreshold = -15.5f; // Umbral de habla normal en dB
        [SerializeField]
        private float shoutThreshold = 0.0f; // Umbral de grito en dB (no se usa actualmente)
        [SerializeField]
        private float targetTime = 10.0f; // Tiempo objetivo para enviar evento de estado


        // Start is called before the first frame update
        void Start()
        {
           
            InvokeRepeating(nameof(CheckVolume), targetTime, targetTime);
        }

        public void CheckVolume()
        {
            float dB;
            if (!usemicrophone)
            {
                dB = voiceVariablesWit.MaxVolume;
                voiceVariablesWit.ResetVolume();
            }
            else
            {
                dB = voiceVariables.maxVolume;
                voiceVariables.ResetVolume();
            }
            ClassifyVolume(dB);
        }

        // Clasifica el tono detectado según los umbrales definidos
        private void ClassifyVolume(float dB)
        {
            //Debug.Log("VoiceEvent: " + dB);
            if (dB <= silenceThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                Debug.Log("VoiceEvent: Silencio "+dB);
            }
            else if (dB <= whisperThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooQuiet);
                Debug.Log("VoiceEvent: Susurro "+dB);
            }
            else if (dB <= normalThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTalksNormal);
                Debug.Log("VoiceEvent: Normal "+dB);
            }
            else
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooLoud);
                Debug.Log("VoiceEvent: Grito "+dB);
            }
        }
    }

}

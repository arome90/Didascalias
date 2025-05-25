using Meta.WitAi.Speech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Extensions;

namespace ClassRoomVR
{
    /// <summary>
    /// Gestiona la detección y clasificación del nivel de volumen de voz,
    /// aplicando fuerzas externas según el umbral detectado.
    /// </summary>
    public class VoiceEvent : MonoBehaviour
    {
        [SerializeField]
        private VoiceVariables voiceVariables;// Referencia a variables de volumen de voz estándar (micrófono).
        [SerializeField]
        private VoiceVariablesWit voiceVariablesWit;// Referencia a variables de volumen de voz con WitAI.

        [SerializeField]
        private bool usemicrophone = true;// Indica si se debe usar el micrófono local para medir el volumen.

        [SerializeField]
        private ExternalForceManager externalForceManager;


        [SerializeField]
        private float silenceThreshold = -45.0f;
        [SerializeField]
        private float whisperThreshold = -32.0f; // Umbral de susurro en dB
        [SerializeField]
        private float normalThreshold = -15.5f; // Umbral de habla normal en dB
        //[SerializeField]
        //private float shoutThreshold = 0.0f; // Umbral de grito en dB 
        [SerializeField]
        private float targetTime = 10.0f; // Tiempo objetivo para enviar evento de estado


        void Start()
        {
            InvokeRepeating(nameof(CheckVolume), targetTime, targetTime);
        }

        /// <summary>
        /// Comprueba el nivel de volumen actual y lo clasifica.
        /// </summary>
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

        /// <summary>
        /// Clasifica el volumen detectado y aplica la fuerza externa correspondiente.
        /// </summary>
        /// <param name="dB">Volumen medido en decibelios.</param>
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

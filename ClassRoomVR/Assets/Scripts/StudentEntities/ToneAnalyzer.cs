using UnityEngine;
using System.Linq;
using Unity.Mathematics;
using System.Collections;
namespace ClassRoomVR
{

    public class ToneAnalyzer : MonoBehaviour
    {
        [SerializeField]
        private float silenceThreshold = -35.0f; // Umbral de silencio en dB
        [SerializeField]
        private float whisperThreshold = -25.0f; // Umbral de susurro en dB
        [SerializeField]
        private float normalThreshold = -12f; // Umbral de habla normal en dB
        [SerializeField]
        private float shoutThreshold = 0.0f; // Umbral de grito en dB (no se usa actualmente)
        [SerializeField]
        private float targetTime = 10.0f; // Tiempo objetivo para enviar evento de estado
        [SerializeField]
        private float silenceTime = 10.0f; // Tiempo de silencio
        [SerializeField]
        private float talksTooMuchTime = 15.0f; // Tiempo para detectar si se habla demasiado

        private float timeCounter = 0.0f; // Contador de tiempo para enviar evento
        private float silencetimeCounter = 0.0f; // Contador de tiempo para enviar evento de silencio
        private float talksTooMuchTimeCounter = 0.0f; // Contador de tiempo para detectar si se habla demasiado
        private float maxTone = 0; // Máximo nivel de tono detectado


        [SerializeField]
        private VoiceActivation voiceActivation;
        [SerializeField]
        private ExternalForceManager externalForceManager;

        void Start()
        {
            maxTone = silenceThreshold;
        }

        void Update()
        {
            timeCounter += Time.deltaTime;
            float dB = voiceActivation.getLevelAudio();

            maxTone = Mathf.Max(maxTone, dB);

            // Verifica si el nivel de audio está por debajo del umbral de silencio
            if (dB < silenceThreshold)
            {
                silencetimeCounter += Time.deltaTime;
            }
            else
            {
                silencetimeCounter = 0.0f;
            }

            // Verifica si se ha hablado demasiado
            if (dB <= silenceThreshold)
            {
                // Reinicia el contador de hablar demasiado si hay silencio prolongado
                if (silencetimeCounter> 5.0f) {
                    talksTooMuchTimeCounter = 0.0f;
                }
            }
            else
            {
                talksTooMuchTimeCounter += Time.deltaTime;
            }

            if(talksTooMuchTimeCounter > talksTooMuchTime)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTalksTooMuch);
                talksTooMuchTimeCounter = 0.0f;

            }

            if (silencetimeCounter > targetTime)
            {
                silencetimeCounter = 0;
                timeCounter = 0;
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                Debug.Log("Silencio");

            }
            else if(timeCounter> targetTime)
            {
                timeCounter = 0;
                voiceActivation.clearVolumeList();
                ClasificarTono(maxTone);
                maxTone = silenceThreshold;
            }
        }


        // Clasifica el tono detectado según los umbrales definidos
        void ClasificarTono(float dB)
        {
            if (dB <= silenceThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                Debug.Log("Silencio");
            }
            else if (dB <= whisperThreshold)
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooQuiet);
                Debug.Log("Susurro");
            }
            else if (dB <= normalThreshold)
            {
                Debug.Log("Normal");
            }
            else
            {
                externalForceManager.ApplyExternalForce(ExternalForces.TeacherTooLoud);
                Debug.Log("Grito");
            }

          
        }


        IEnumerator CheckVoiceVolume()
        {
            while (true)
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
                    silencetimeCounter = 0.0f;
                }

                if (silencetimeCounter > targetTime)
                {
                    silencetimeCounter = 0;
                    timeCounter = 0;
                    externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
                    Debug.Log("Silencio");
                }
                else if (timeCounter > targetTime)
                {
                    timeCounter = 0;
                    voiceActivation.clearVolumeList();
                    ClasificarTono(maxTone);
                    maxTone = silenceThreshold;
                }

                yield return null; // Espera hasta el siguiente frame
                //yield return new WaitForSeconds(targetTime);
            }
        }



    }
}

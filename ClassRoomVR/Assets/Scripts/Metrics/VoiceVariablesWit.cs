using ClassRoomVR;
using MathNet.Numerics.Statistics;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceVariablesWit : MonoBehaviour
{
    [SerializeField] 
    AppVoiceExperience appVoiceExperience;

    [SerializeField]
    private ExternalForceManager externalForceManager;

    List<float> volumeList;

    [SerializeField]
    private float silenceThreshold = -35.0f;
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

    void Awake()
    {
        appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener((value) =>
        {
            OnMicLevelChanged(value);
        });
        appVoiceExperience.VoiceEvents.OnMicStartedListening.AddListener(() =>
        {
            volumeList.RemoveRange(0, volumeList.Count);
        });
        volumeList = new List<float>(100);
        volumeList.Capacity = 100;
    }

    void Start()
    {
        maxTone = silenceThreshold;
        if (appVoiceExperience == null || !appVoiceExperience.enabled || !appVoiceExperience.gameObject.activeSelf)
        {
            this.enabled = false;
            Debug.LogError("voiceActivation not found or not enabled");
        }
    }

    void Update()
    {
        timeCounter += Time.deltaTime;
        float dB = getLevelAudio();

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
            if (silencetimeCounter > 5.0f)
            {
                talksTooMuchTimeCounter = 0.0f;
            }
        }
        else
        {
            talksTooMuchTimeCounter += Time.deltaTime;
        }

        if (talksTooMuchTimeCounter > talksTooMuchTime)
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
        else if (timeCounter > targetTime)
        {
            timeCounter = 0;
            clearVolumeList();
            ClasificarTono(maxTone);
            maxTone = silenceThreshold;
        }
    }


    // Clasifica el tono detectado según los umbrales definidos
    void ClasificarTono(float dB)
    {
        if (dB <= silenceThreshold)
        {
            //externalForceManager.ApplyExternalForce(ExternalForces.TeacherSilentTooLong);
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
            float dB = getLevelAudio();

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
                clearVolumeList();
                ClasificarTono(maxTone);
                maxTone = silenceThreshold;
            }

            yield return null; // Espera hasta el siguiente frame
                               //yield return new WaitForSeconds(targetTime);
        }
    }

   

    public float getLevelAudio()
    {
        if (volumeList.Count == 0) return silenceThreshold;
        return (float)volumeList.Maximum();
    }

    public void OnMicLevelChanged(float a)
    {
        float dB = 20 * Mathf.Log10(a);  //LUFS
                                         //Debug.Log("Volumen de voz: " + dB + " dB");
        if (dB > silenceThreshold)
        {
            if (volumeList.Count > 100)
            {
                volumeList.Remove(0);
            }
            volumeList.Add(dB);
        }
    }

    public void clearVolumeList()
    {
        volumeList.Clear();
    }



}

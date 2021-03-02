
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using System;

public class SoundLoudness : MonoBehaviour
{
    private float timeSinceSceneStarted;

    public static float step = 0.05f;

    // Sonidos de la primera reacción al comentario
    private List<float> soundAfterComment;

    // Sonidos del discurso normal del jugador antes del comentario
    private List<float> soundBeforeComment;

    public UnityEvent strongResponseEvent;

    private bool commentFinished = false;
    private bool playerHasSpoken = false;

    int _sampleWindow = 1024;
    private bool collect = false;

    float actTime;

    public double increase = 1.5;
    public double minNoiseTreshold = 0.02;
    public double noAverageThreshold = 0.1;
    public bool firstScenario;
    public bool secondScenario;


    void Start()
    {
        timeSinceSceneStarted = 0;

        soundAfterComment = new List<float>();

        soundBeforeComment = new List<float>();
        collect = false;
        actTime = Time.realtimeSinceStartup;
    }

    public void startCollecting()
    {
        collect = true;
    }

    void OnDisable()
    {
        collect = false;

    }

    public void setCommentFinished()
    {
        commentFinished = true;
    }

    /// <summary>
    /// Obtiene el valor medio cuadrático de 1024 muestras obtenidas en el instante.
    /// Con ese valor, calcula el valor en decibelios de la muestra. 
    /// </summary>
    /// <returns>Valor en decibelios del micrófono.</returns>
    float getLoudness()
    {

        float[] waveData = new float[_sampleWindow];
        int micPosition = MicrophoneManager.GetMicrophonePosition() - (_sampleWindow + 1); // null means the first microphone
        if (micPosition < 0) return 0;
        MicrophoneManager.AudioClip.GetData(waveData, micPosition);

        //Normalize(waveData);

        //Root Mean Square value calculation
        float rmsvalue = 0.0f;
        for (int i = 0; i < _sampleWindow; i++)
        {
            rmsvalue += waveData[i] * waveData[i];
        }
        rmsvalue = Mathf.Sqrt(rmsvalue / _sampleWindow);

        float decibels = 20 * Mathf.Log10(rmsvalue / 0.01f);

        return rmsvalue;
    }

    //Normalización multiplicado por 1/max del array
    void Normalize(float[] arr)
    {
        //Normalización de los datos
        float max = arr.Max();

        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = arr[i] * (1 / max);
        }
    }

    void Update()
    {
        timeSinceSceneStarted += Time.deltaTime;

        if (!collect) return;

        if (firstScenario)
        {
            CalculateWithAverage();
        }
        else if (secondScenario)
        {
            CalculateWithoutAverage();
        }
    }

    private void CalculateWithAverage()
    {

        float aux = -100f;
        if (timeSinceSceneStarted - actTime > step)
        {
            aux = getLoudness();
            // Debug.Log(aux);
            // Solo se añaden los sonidos en los que el jugador habla, para así impedir que los silencios bajen la media
            if (aux > minNoiseTreshold)
            {
                if (!commentFinished)
                    soundBeforeComment.Add(aux);
                else
                {
                    playerHasSpoken = true;
                    soundAfterComment.Add(aux);
                }
            }
            else if (playerHasSpoken)
            {
                collect = false;
                CalculateResponseStrengthWithAverage();
            }

            actTime = timeSinceSceneStarted;
        }
    }
    private void CalculateWithoutAverage()
    {
        float aux = -100f;
        if (timeSinceSceneStarted - actTime > step)
        {
            aux = getLoudness();
            // Debug.Log(aux);
            // Solo se añaden los sonidos en los que el jugador habla, para así impedir que los silencios bajen la media
            if (aux > minNoiseTreshold)
            {
                soundAfterComment.Add(aux);
            }

            actTime = timeSinceSceneStarted;
        }
    }
    public bool StopRecordingAndCalculate()
    {
        if (!playerHasSpoken)
        {
            collect = false;
            /*
            if (firstScenario)
            {
                CalculateResponseStrengthWithAverage();
            }
            else if (secondScenario)
            {
                CalculateResponseStrengthWithoutAverage();
            }
            */
            //return CalculateResponseStrengthWithoutAverage();
            return CalculateResponseStrengthWithAverage();
        }
        return false;
    }

    private bool CalculateResponseStrengthWithAverage()
    {
        // Media de la intensidad del sonido antes del comentario
        float[] soundBeforeComment = this.soundBeforeComment.ToArray();
        float sumaAntes = 0;
        for (int i = 0; i < soundBeforeComment.Length; i++)
        {
            sumaAntes += soundBeforeComment[i];
        }
        float mediaAntes = 0;
        if (soundBeforeComment.Length > 0)
            mediaAntes = sumaAntes / soundBeforeComment.Length;

        // Media de la intensidad del sonido después del comentario
        float[] soundAfterComment = this.soundAfterComment.ToArray();
        float sumaDespues = 0;
        for (int i = 0; i < soundAfterComment.Length; i++)
        {
            sumaDespues += soundAfterComment[i];
        }
        float mediaDespues = 0;
        if (soundAfterComment.Length > 0)
            mediaDespues = sumaDespues / soundAfterComment.Length;
        
        Debug.Log("Media de decibelios antes del comentario: " + mediaAntes);
        Debug.Log("Media de decibelios después del comentario: " + mediaDespues);

        if (mediaDespues > mediaAntes * increase)
        {
            Debug.Log("Respuesta firme detectada");
            return true;
            //strongResponseEvent.Invoke();
        }
        return false;
    }

    private bool CalculateResponseStrengthWithoutAverage()
    {
        // Media de la intensidad del sonido después del comentario
        float[] soundAfterComment = this.soundAfterComment.ToArray();
        float sumaDespues = 0;
        for (int i = 0; i < soundAfterComment.Length; i++)
        {
            sumaDespues += soundAfterComment[i];
        }
        float mediaDespues = 0;
        if (soundAfterComment.Length > 0)
            mediaDespues = sumaDespues / soundAfterComment.Length;

        Debug.Log("Media de decibelios después del comentario: " + mediaDespues);

        if (mediaDespues > noAverageThreshold)
        {
            Debug.Log("Respuesta firme detectada");
            //strongResponseEvent.Invoke();
            return true;
        }
        return false;
    }
}
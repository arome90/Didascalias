using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
public class Emotion
{
    private float[] _emotions;
    private System.Random random;

    public Emotion()
    {
        int emotionCount = System.Enum.GetValues(typeof(EmotionType)).Length;
        _emotions = new float[emotionCount]; // Crea un array para todas las emociones
        random = new System.Random();
    }

    // Inicializa las emociones con valores aleatorios entre 0 y max
    public void InitializeEmotions(float max)
    {
        for (int i = 0; i < _emotions.Length; i++)
        {
            _emotions[i] = ((float)random.NextDouble() * 2) - 1;
        }
    }

    // Obtiene el valor de una emoci�n
    public float GetEmotionValue(EmotionType emotion)
    {
        return _emotions[(int)emotion];
    }

    // Establece un nuevo valor para una emoci�n
    public void SetEmotionValue(EmotionType emotion, float value)
    {
        _emotions[(int)emotion] = Mathf.Clamp(value, -1, 1); 
    }

    // Devuelve todas las emociones como un array
    public float[] GetAllEmotions()
    {
        return (float[])_emotions.Clone();
    }

    // M�todo auxiliar para depuraci�n
    public override string ToString()
    {
        return string.Join(", ", System.Enum.GetValues(typeof(EmotionType))
            .Cast<EmotionType>()
            .Select(e => $"{e}: {_emotions[(int)e]:F2}"));
    }
}
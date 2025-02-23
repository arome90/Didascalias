using System;
using UnityEngine;
using System.Linq;
using BehaviorDesigner.Runtime;
public class Emotion
{
    private float[] _emotions;
    private System.Random random;
    private BehaviorTree _behaviorTree;
    private float sendEventThreshold = 0.3f;
    private float emoCounter;

    public Emotion(BehaviorTree bt = null)
    {
        int emotionCount = Enum.GetValues(typeof(EmotionType)).Length;
        _emotions = new float[emotionCount]; // Crea un array para todas las emociones
        random = new System.Random();
        _behaviorTree = bt;
        emoCounter = 0f;
    }

    // Inicializa las emociones con valores aleatorios entre 0 y max
    public void InitializeEmotions(float max)
    {
        for (int i = 0; i < _emotions.Length; i++)
        {
            _emotions[i] = (float)((random.NextDouble() * 2) - 1);
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
        float finalValue = Mathf.Clamp(value, -1, 1);
        //Cantidad que varia la emocion
        float change = Math.Abs(_emotions[(int)emotion] - finalValue);
        //Se guarda la cantidad
        emoCounter += change;
        _emotions[(int)emotion] = finalValue;

        //Se notifica de que ha cambiado el estado emocional si el BehaviorTree no es null
        if(emoCounter > sendEventThreshold)
        {
            emoCounter = 0;
            _behaviorTree?.SendEvent("EmoChange");
        }
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
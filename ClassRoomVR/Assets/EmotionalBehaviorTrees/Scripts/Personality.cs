using System.Collections;
using System.Collections.Generic;
using System;
using ClassRoomVR;
using UnityEngine;

// Based on the big 5 personality traits model
public class Personality
{
    // Array para almacenar los rasgos de personalidad
    private float[] _traits;
    private System.Random random;

    // Umbral de atención
    [SerializeField]
    private float attentionThreshold = -0.6f;

    // Constructor
    public Personality()
    {
        int traitCount = System.Enum.GetValues(typeof(PersonalityType)).Length;
        _traits = new float[traitCount]; // Crea un array para los rasgos
        random = new System.Random();
        InitializePersonality();
    }

    // Inicializa los rasgos de personalidad con valores aleatorios entre 0 y 1
    private void InitializePersonality()
    {
        for (int i = 0; i < _traits.Length; i++)
        {
            _traits[i] = (float)((random.NextDouble()* 2) - 1); // Rellenar con valores aleatorios entre -1 y 1
        }
    }

    // Obtener el valor de un rasgo de personalidad
    public float GetTraitValue(PersonalityType trait)
    {
        return _traits[(int)trait];
    }

    // Establecer el valor de un rasgo de personalidad
    public void SetTraitValue(PersonalityType trait, float value)
    {
        _traits[(int)trait] = Mathf.Clamp(value, -1f, 1f); // Limita el valor entre 0 y 1
    }

    // Método que influye en las emociones de un estudiante según su personalidad
    public void InfluenceEmotions(Emotion emotion, StudentBehavior studentBehavior)
    {
        if (studentBehavior.AttentionLevel < attentionThreshold) return;

        emotion.SetEmotionValue(EmotionType.Joy, GetTraitValue(PersonalityType.Extraversion) * 0.9f +
                      GetTraitValue(PersonalityType.Agreeableness) * 0.6f +
                      GetTraitValue(PersonalityType.Conscientiousness) * 0.6f +
                      (1 - GetTraitValue(PersonalityType.Neuroticism)) * 0.8f +
                      GetTraitValue(PersonalityType.Openness) * 0.6f);

        emotion.SetEmotionValue(EmotionType.Sadness, GetTraitValue(PersonalityType.Neuroticism) * 0.8f +
                          (1 - GetTraitValue(PersonalityType.Extraversion)) * 0.3f);

        emotion.SetEmotionValue(EmotionType.Fear, GetTraitValue(PersonalityType.Neuroticism) * 0.8f +
                       (1 - GetTraitValue(PersonalityType.Extraversion)) * 0.3f);
       
        emotion.SetEmotionValue(EmotionType.Anger, GetTraitValue(PersonalityType.Neuroticism) * 0.7f +
                        (1 - GetTraitValue(PersonalityType.Agreeableness)) * 0.3f);

        emotion.SetEmotionValue(EmotionType.Surprise, GetTraitValue(PersonalityType.Extraversion) * 0.8f +
                           GetTraitValue(PersonalityType.Openness) * 0.8f);
      
        emotion.SetEmotionValue(EmotionType.Disgust, GetTraitValue(PersonalityType.Neuroticism) * 0.7f +
                          (1 - GetTraitValue(PersonalityType.Agreeableness)) * 0.3f);

    }

    public void getString(ref string s)
    {
        for (int i = 0;i<_traits.Length;i++)
        {
            s += ((PersonalityType)(i)).ToString();
            s += ": " + _traits[i] + "\n";
        }
    }
}



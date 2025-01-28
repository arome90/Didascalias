using System.Collections;
using System.Collections.Generic;
using System;
using ClassRoomVR;
using UnityEngine;
using System.IO;

// Based on the big 5 personality traits model
public class Personality
{
    // Array para almacenar los rasgos de personalidad
    private float[] _traits;
    private System.Random random;

    // Umbral de atención
    [SerializeField]
    private float attentionThreshold = -0.6f;


    private Dictionary<PersonalityType, Dictionary<EmotionType, float>> personalityEmotionInfluence;

    // Constructor
    public Personality()
    {
        int traitCount = System.Enum.GetValues(typeof(PersonalityType)).Length;
        _traits = new float[traitCount]; // Crea un array para los rasgos
        random = new System.Random();
        InitializePersonality();
    }

    public void LoadPersonalityFromJson(string personalityEmotionJsonPath)
    {
        string filePath = Path.Combine(Application.dataPath, personalityEmotionJsonPath);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);

            try
            {
                // Deserializar el JSON a la estructura de datos
                EntryKeyValueDictionaryWrapper wrapper = JsonUtility.FromJson<EntryKeyValueDictionaryWrapper>(json);
                Dictionary<string, Dictionary<string, float>> tempImpacts = wrapper.ToDictionary();

                // Convertir claves a enumeradores
                personalityEmotionInfluence = new Dictionary<PersonalityType, Dictionary<EmotionType, float>>();

                foreach (var kvp in tempImpacts)
                {
                    if (System.Enum.TryParse(kvp.Key, out PersonalityType p))
                    {
                        var emotionImpacts = new Dictionary<EmotionType, float>();

                        foreach (var emotionKvp in kvp.Value)
                        {
                            if (System.Enum.TryParse(emotionKvp.Key, out EmotionType emotion))
                            {
                                emotionImpacts[emotion] = emotionKvp.Value;
                            }
                        }

                        personalityEmotionInfluence[p] = emotionImpacts;
                    }
                }

                Debug.Log("Personality loaded successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing JSON file: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError($"EPersonality file not found at path: {filePath}");
        }
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

    public float GetInfluenceEmotion(EmotionType emotion)
    {
        float value = 1.0f;
        foreach (PersonalityType personality in Enum.GetValues(typeof(PersonalityType)))
        {
            value += (_traits[(int)personality] - 0.5f)* personalityEmotionInfluence[personality][emotion];
        }
        return value;
    }

    // Método que influye en las emociones de un estudiante según su personalidad
    public void InfluenceEmotions(Emotion emotion, StudentBehavior studentBehavior)
    {
        if (studentBehavior.AttentionLevel < attentionThreshold) return;

        emotion.SetEmotionValue(EmotionType.AnxietyConfidence, GetTraitValue(PersonalityType.Extraversion) * 0.9f +
                      GetTraitValue(PersonalityType.Agreeableness) * 0.6f +
                      GetTraitValue(PersonalityType.Conscientiousness) * 0.6f +
                      (1 - GetTraitValue(PersonalityType.Neuroticism)) * 0.8f +
                      GetTraitValue(PersonalityType.Openness) * 0.6f);

        emotion.SetEmotionValue(EmotionType.DispiritedEncouraged, GetTraitValue(PersonalityType.Neuroticism) * 0.8f +
                          (1 - GetTraitValue(PersonalityType.Extraversion)) * 0.3f);

        emotion.SetEmotionValue(EmotionType.TerrorEnchantment, GetTraitValue(PersonalityType.Neuroticism) * 0.8f +
                       (1 - GetTraitValue(PersonalityType.Extraversion)) * 0.3f);
       
        emotion.SetEmotionValue(EmotionType.AnxietyConfidence, GetTraitValue(PersonalityType.Neuroticism) * 0.7f +
                        (1 - GetTraitValue(PersonalityType.Agreeableness)) * 0.3f);

        emotion.SetEmotionValue(EmotionType.BoredomFascination, GetTraitValue(PersonalityType.Extraversion) * 0.8f +
                           GetTraitValue(PersonalityType.Openness) * 0.8f);
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



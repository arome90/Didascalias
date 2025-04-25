using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using ClassRoomVR;
using System;

/// <summary>
/// Maneja fuerzas externas que afectan a los estudiantes.
/// Hereda de <see cref="SceneSingleton{ExternalForceManager}"/>.
/// </summary>
public class ExternalForceManager : SceneSingleton<ExternalForceManager>
{
    private Dictionary<ExternalForces, Dictionary<EmotionType, float>> externalForceEmotionImpacts;
    private Dictionary<ExternalForces, float> externalForceAttentionImpact;
    private Dictionary<string, Student> _students;

    // Umbral de atención
    [SerializeField]
    private float attentionThreshold = -0.6f;
    [SerializeField] private string externalForcesEmotionJsonPath;
    [SerializeField] private string externalForcesAttentionJsonPath;

    void Start()
    {
        LoadExternalForcesFromJson();
        _students = ClassManager.Instance.getStudents();

        // Aplicar una fuerza de ejemplo al inicio
        //ApplyExternalForce(ExternalForces.TeacherTooQuiet);
    }

    /// <summary>
    /// Carga las definiciones de fuerzas externas desde un archivo JSON.
    /// </summary>
    private void LoadExternalForcesFromJson()
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, externalForcesEmotionJsonPath);

        externalForceEmotionImpacts = new Dictionary<ExternalForces, Dictionary<EmotionType, float>>();
        foreach (ExternalForces force in Enum.GetValues(typeof(ExternalForces)))
        {
            var tempDict = new Dictionary<EmotionType, float>();
            LoadManager.Instance.FillDictionary(ref tempDict, 0.0f);
            externalForceEmotionImpacts[force] = tempDict;           
        }
        Dictionary<string, Dictionary<string, float>> tempImpacts = LoadManager.Instance.LoadDataFromJson<string, Dictionary<string, float>>(filePath);
        // Convertir claves a enumeradores
        if(tempImpacts != null)
        {
            foreach (var kvp in tempImpacts)
            {
                if (System.Enum.TryParse(kvp.Key, out ExternalForces force))
                {
                    var emotionImpacts = new Dictionary<EmotionType, float>();

                    foreach (var emotionKvp in kvp.Value)
                    {
                        if (System.Enum.TryParse(emotionKvp.Key, out EmotionType emotion))
                        {
                            emotionImpacts[emotion] = emotionKvp.Value;
                        }
                    }

                    externalForceEmotionImpacts[force] = emotionImpacts;
                }
            }
        }
        

        filePath = System.IO.Path.Combine(Application.persistentDataPath, externalForcesAttentionJsonPath);
        Dictionary<string, float> tempImpacts2 = LoadManager.Instance.LoadDataFromJson<string, float>(filePath);
        externalForceAttentionImpact = new Dictionary<ExternalForces, float>();
        LoadManager.Instance.FillDictionary(ref externalForceAttentionImpact, 0.0f);
        if (tempImpacts2 != null)
        {
            foreach (var kvp in tempImpacts2)
            {
                if (System.Enum.TryParse(kvp.Key, out ExternalForces force))
                {
                    externalForceAttentionImpact[force] = kvp.Value;
                }
            }
            Debug.Log("External forces loaded successfully.");
        }
    }

    /// <summary>
    /// Aplica una fuerza externa a las emociones de todos los estudiantes.
    /// </summary>
    /// <param name="force">Fuerza externa a aplicar.</param>
    public void ApplyExternalForce(ExternalForces force)
    {
        if (externalForceEmotionImpacts == null || externalForceEmotionImpacts.Count == 0)
        {
            Debug.LogError("External forces have not been loaded.");
            return;
        }

        if (externalForceEmotionImpacts.TryGetValue(force, out var emotionImpacts))
        {
            if (externalForceAttentionImpact.TryGetValue(force, out var attentionImpact))
            {
                foreach (var kvp in _students)
                {
                    Student student = kvp.Value;
                    Emotion studentEmotion = student.GetEmotion();
                    StudentBehavior studentAttention = student.GetBehavior();
                    studentAttention.ExternalForceInfluence(attentionImpact);
                    if (studentAttention.AttentionLevel >= attentionThreshold)
                    {
                        foreach (var emotionImpact in emotionImpacts)
                        {
                            EmotionType emotionType = emotionImpact.Key;
                            float impactValue = emotionImpact.Value;

                            // Modificar la emoción del estudiante

                            student.ModifyEmotion(emotionType, impactValue);
                        }
                    }
                }
                //Debug.Log($"Applied external force '{force}' to all students.");

            }
            else
            {
                Debug.LogWarning($"External force '{force}' has no defined attention impacts.");
            }

        }
        else
        {
            Debug.LogWarning($"External force '{force}' has no defined emotional impacts.");
        }

    }
}

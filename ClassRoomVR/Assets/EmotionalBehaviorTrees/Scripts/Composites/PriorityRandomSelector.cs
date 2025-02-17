using UnityEngine;
using System.Collections.Generic;
using System;
using System.Drawing.Printing;
using ClassRoomVR;
using System.IO;
using System.Linq;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Ordenar de acuerdo con los pesos y asociar un probi de probabilidad a cada Nodo secundario ")]
    [TaskIcon("{SkinColor}PrioritySelectorIcon.png")]
    public class PriorityRandomSelector : Action
    {
        [Tooltip("Probability factor [0,1]")]
        public float probability = 0.5f;

        [Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        [Tooltip("The value of the int parameter")]
        public SharedInt intValue;
        // The order to run its children in. 
        // first is priority, second is index id
        //private SortedDictionary<float, int> executionOrder = new SortedDictionary<float, int>();
        private List<KeyValuePair<float, int>> executionOrder = new List<KeyValuePair<float, int>>();

        private List<Dictionary<BehaviorInfluences, float>> behaviorInfluences;
        private string behaviorInfluencesJsonPath = "jsonResources/BehaviorInfluences.json";
        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed)
            {
                UnityEngine.Random.InitState(seed);
            }
            LoadExternalForcesFromJson();
        }

        public override void OnStart()
        {
            ComputeBehaviorPriorities();
            executionOrder.Sort((x, y) => y.Key.CompareTo(x.Key));

            float end_num = probability * Mathf.Pow(1 - probability, behaviorInfluences.Count - 1);
            float aux = UnityEngine.Random.Range(0, 1 - end_num);
            int k = 0;
            while (aux > 0)
            {
                k++;
                aux -= probability * Mathf.Pow(1 - probability, k - 1);
            }
            intValue.SetValue(executionOrder[k - 1].Value);
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            //intValue.Value =-1;
        }

        private void ComputeBehaviorPriorities()
        {
            // Make sure the list is empty before we add child indexes to it.
            executionOrder.Clear();

            // Loop through each child task and determine its priority. The higher the priority the lower it goes within the list. The task with the highest
            // priority will be first in the list and will be executed first.
            for (int i = 0; i < behaviorInfluences.Count; ++i)
            {
                executionOrder.Add(new KeyValuePair<float, int>(ComputePriority(behaviorInfluences[i]), i));
            }
        }

        private float ComputePriority(Dictionary<BehaviorInfluences, float> behavior)
        {
            GameObject targetGameObject = gameObject;
            //para generalizar se puede crear un componente especifico TODO
            Emotion emotion = targetGameObject.GetComponent<Student>().GetEmotion();
            Personality personality = targetGameObject.GetComponent<Student>().getPersonality();
            float emotionInfluence = emotion.GetEmotionValue(EmotionType.BoredomFascination) * behavior[BehaviorInfluences.BoredomFascination] +
                emotion.GetEmotionValue(EmotionType.DispiritedEncouraged) * behavior[BehaviorInfluences.DispiritedEncouraged] +
                emotion.GetEmotionValue(EmotionType.TerrorEnchantment) * behavior[BehaviorInfluences.TerrorEnchantment] +
                emotion.GetEmotionValue(EmotionType.FrustrationEuphoria) * behavior[BehaviorInfluences.FrustrationEuphoria] +
                emotion.GetEmotionValue(EmotionType.AnxietyConfidence) * behavior[BehaviorInfluences.AnxietyConfidence];

            float cont1 = (behavior[BehaviorInfluences.BoredomFascination] + behavior[BehaviorInfluences.DispiritedEncouraged] +
                behavior[BehaviorInfluences.TerrorEnchantment] + behavior[BehaviorInfluences.FrustrationEuphoria] +
                behavior[BehaviorInfluences.AnxietyConfidence]);

            float personalityInfluence = personality.GetTraitValue(PersonalityType.Openness) * behavior[BehaviorInfluences.Openness] +
                personality.GetTraitValue(PersonalityType.Agreeableness) * behavior[BehaviorInfluences.Agreeableness] +
               personality.GetTraitValue(PersonalityType.Conscientiousness) * behavior[BehaviorInfluences.Conscientiousness] +
               personality.GetTraitValue(PersonalityType.Extraversion) * behavior[BehaviorInfluences.Extraversion] +
               personality.GetTraitValue(PersonalityType.Neuroticism) * behavior[BehaviorInfluences.Neuroticism];

            float cont2 = behavior[BehaviorInfluences.Openness] + behavior[BehaviorInfluences.Agreeableness] + behavior[BehaviorInfluences.Conscientiousness] +
                behavior[BehaviorInfluences.Extraversion] + behavior[BehaviorInfluences.Neuroticism];


            return (emotionInfluence + personalityInfluence) * behavior[BehaviorInfluences.Priority];
        }

        /// <summary>
        /// Carga las definiciones de fuerzas externas desde un archivo JSON.
        /// </summary>
        private void LoadExternalForcesFromJson()
        {
            string filePath = Path.Combine(Application.dataPath, behaviorInfluencesJsonPath);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                try
                {
                    // Deserializar el JSON a la estructura de datos
                    EntryKeyValueDictionaryWrapper wrapper = JsonUtility.FromJson<EntryKeyValueDictionaryWrapper>(json);
                    Dictionary<string, Dictionary<string, float>> tempImpacts = wrapper.ToDictionary();

                    // Convertir claves a enumeradores
                    behaviorInfluences = new List<Dictionary<BehaviorInfluences, float>>();

                    foreach (var kvp in tempImpacts)
                    {

                        var behaviorImpacts = new Dictionary<BehaviorInfluences, float>();

                        foreach (var emotionKvp in kvp.Value)
                        {
                            if (System.Enum.TryParse(emotionKvp.Key, out BehaviorInfluences emotion))
                            {
                                behaviorImpacts[emotion] = emotionKvp.Value;
                            }
                        }

                        behaviorInfluences.Add(behaviorImpacts);

                    }

                    Debug.Log("Behavior influences loaded successfully.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error parsing JSON file: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"External forces file not found at path: {filePath}");
            }
        }
    }
}
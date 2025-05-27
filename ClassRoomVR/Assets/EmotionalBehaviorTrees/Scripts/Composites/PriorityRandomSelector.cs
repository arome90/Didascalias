using UnityEngine;
using System.Collections.Generic;
using System;
using ClassRoomVR;
using System.Drawing;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class EmoImpact
    {
        public EmoImpact()
        {
            Influences = new Dictionary<BehaviorInfluences, float>();
            //Valor por defecto
            Value = -1; 
            foreach (BehaviorInfluences influence in Enum.GetValues(typeof(BehaviorInfluences)))
            {
                Influences[influence] = 0;
            }
        }
        public int Value { get; set; }
        public Dictionary<BehaviorInfluences, float> Influences { get; set; }
        public List<EmoCondition> Conditions { get; set; }
    }

    [Serializable]
    public class EmoCondition
    {
        public BehaviorInfluences Influence { get; set; }
        public string Operator { get; set; }
        public float Value { get; set; }

        public bool Check(float v)
        {
            if (Operator == ">")
            {
                return Value > v;
            }
            else if (Operator == "<")
            {
                return Value < v;
            }
            else { return Value == v; }
        }
    }

    [Serializable]
    public class LoadImpact
    {
        public int Value { get; set; }
        public Dictionary<string, float> Influences { get; set; }
        public List<EmoCondition> Conditions { get; set;}
    }

    [TaskDescription("Ordenar de acuerdo con los pesos y asociar un probi de probabilidad a cada Nodo secundario ")]
    [TaskIcon("{SkinColor}PrioritySelectorIcon.png")]
    public class PriorityRandomSelector : Action
    {
        [Tooltip("Probability factor [0.5,1.0]")]
        public float probability = 0.5f;

        [Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        [Tooltip("The value of the int parameter")]
        public SharedInt intValue;
        // The order to run its children in. 
        // first is priority, second is index id
        private List<KeyValuePair<float, int>> executionOrder = new List<KeyValuePair<float, int>>();

        private List<EmoImpact> behaviorInfluences;

        private List<float> cumulativeProbabilities;

        [SerializeField]
        private string behaviorInfluencesJsonPath;
        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed)
            {
                UnityEngine.Random.InitState(seed);
            }
            LoadExternalForcesFromJson();
            cumulativeProbabilities = BuildCumulativeProbabilities(probability, behaviorInfluences.Count);

        }

        public override void OnStart()
        {
            ComputeBehaviorPriorities();
            executionOrder.Sort((x, y) => y.Key.CompareTo(x.Key));
            int n=executionOrder.Count;
            float aux = UnityEngine.Random.Range(0, cumulativeProbabilities[n-1]);
            int k = 0;
            while (aux > cumulativeProbabilities[k] && k < n) k++;
            intValue.SetValue(executionOrder[k].Value);
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
                executionOrder.Add(new KeyValuePair<float, int>(ComputePriority(behaviorInfluences[i]), behaviorInfluences[i].Value));
            }
        }

        private float ComputePriority(EmoImpact emoImpact)
        {
            GameObject targetGameObject = gameObject;
            Student student = gameObject.GetComponent<Student>();
            foreach (EmoCondition e in emoImpact.Conditions)
            {
                if (!e.Check(student.getBehaviorInfluences(e.Influence))) return 0.0f;
            }

            Dictionary<BehaviorInfluences, float> behavior = emoImpact.Influences;
            //para generalizar se puede crear un componente especifico TODO
            Emotion emotion = targetGameObject.GetComponent<Student>().GetEmotion();
            Personality personality = targetGameObject.GetComponent<Student>().getPersonality();
            float emotionInfluence = emotion.GetEmotionValue(EmotionType.BoredomFascination) * behavior[BehaviorInfluences.BoredomFascination] +
                emotion.GetEmotionValue(EmotionType.DispiritedEncouraged) * behavior[BehaviorInfluences.DispiritedEncouraged] +
                emotion.GetEmotionValue(EmotionType.TerrorEnchantment) * behavior[BehaviorInfluences.TerrorEnchantment] +
                emotion.GetEmotionValue(EmotionType.FrustrationEuphoria) * behavior[BehaviorInfluences.FrustrationEuphoria] +
                emotion.GetEmotionValue(EmotionType.AnxietyConfidence) * behavior[BehaviorInfluences.AnxietyConfidence];

            float personalityInfluence = personality.GetTraitValue(PersonalityType.Openness) * behavior[BehaviorInfluences.Openness] +
                personality.GetTraitValue(PersonalityType.Agreeableness) * behavior[BehaviorInfluences.Agreeableness] +
               personality.GetTraitValue(PersonalityType.Conscientiousness) * behavior[BehaviorInfluences.Conscientiousness] +
               personality.GetTraitValue(PersonalityType.Extraversion) * behavior[BehaviorInfluences.Extraversion] +
               personality.GetTraitValue(PersonalityType.Neuroticism) * behavior[BehaviorInfluences.Neuroticism];
           
            return (emotionInfluence + personalityInfluence + 1) * behavior[BehaviorInfluences.Priority];
        }

        /// <summary>
        /// Carga las definiciones de fuerzas externas desde un archivo JSON.
        /// </summary>
        private void LoadExternalForcesFromJson()
        {
            if (LoadManager.Instance.GetObject("behaviorInfluences", ref behaviorInfluences))
            {
                Debug.Log("Behavior influences loaded successfully.");
                return;
            }
            string path=System.IO.Path.Combine(Application.persistentDataPath, behaviorInfluencesJsonPath);
            Dictionary<string, LoadImpact> tempImpacts = LoadManager.Instance.LoadDataFromJson<string, LoadImpact>(path);
            if (tempImpacts == null) return;

            // Convertir claves a enumeradores
            behaviorInfluences = new List<EmoImpact>();

            foreach (var kvp in tempImpacts)
            {
                var behaviorImpacts = new EmoImpact();
                behaviorImpacts.Value = kvp.Value.Value;
                behaviorImpacts.Conditions = kvp.Value.Conditions;
                foreach (var emotionKvp in kvp.Value.Influences)
                {
                    if (System.Enum.TryParse(emotionKvp.Key, out BehaviorInfluences emotion))
                    {
                        behaviorImpacts.Influences[emotion] = emotionKvp.Value;
                    }
                }
                behaviorInfluences.Add(behaviorImpacts);
            }
            LoadManager.Instance.SaveObject("behaviorInfluences", behaviorInfluences);        
            Debug.Log("Behavior influences loaded successfully.");

        }

        private List<float> BuildCumulativeProbabilities(float probability, int count)
        {
            List<float> cumulative = new List<float>(count);
            float sum = 0f;
            float remainingProb = 1f;

            for (int i = 0; i < count; i++)
            {
                float p = probability * remainingProb;
                sum += p;
                cumulative.Add(sum);
                remainingProb *= (1f - probability);
            }
            return cumulative;
        }
    }
}
using ClassRoomVR;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{

    [TaskDescription("Decorator node for Behavior Designer that determines the priority of a task based on the emotional and personality influences.")]
    [TaskIcon("{SkinColor}CooldownIcon.png")]
    /// <summary>
    /// Nodo decorador para Behavior Designer que determina la prioridad de una tarea
    /// en función de la influencia emocional y de personalidad de un componente Student.
    /// </summary>
    public class EmoInfluence : Decorator
    {
        [Range(0, 1)]
        public float _priority = 1;
        [Range(0, 1)]
        public float OpennessInfluence = 0;
        [Range(0, 1)]
        public float ConscientiousnessInfluence = 0;
        [Range(0, 1)]
        public float ExtraversionInfluence = 0;
        [Range(0, 1)]
        public float AgreeablenessInfluence = 0;
        [Range(0, 1)]
        public float NeuroticismInfluence = 0;
        [Range(0, 1)]
        public float BoredomFascinationInfluence = 0;
        [Range(0, 1)]
        public float DispiritedEncouragedInfluence = 0;
        [Range(0, 1)]
        public float TerrorEnchantmentInfluence = 0;
        [Range(0, 1)]
        public float FrustrationEuphoriaInfluence = 0;
        [Range(0, 1)]
        public float AnxietyConfidenceInfluence = 0;

        public bool onlyPriority;
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override float GetPriority()
        {
            if (onlyPriority) return _priority;

            GameObject targetGameObject = gameObject;

            Student student = targetGameObject.GetComponent<Student>();
            if (student == null)
            {
                Debug.LogWarning("Student component not found on " + targetGameObject.name);
                return _priority;
            }

            Emotion emotion = student.GetEmotion();
            Personality personality = student.getPersonality();
            if (emotion == null || personality == null)
            {
                Debug.LogWarning("Emotion or Personality not found on " + targetGameObject.name);
                return _priority;
            }

            float emotionInfluence =
                emotion.GetEmotionValue(EmotionType.BoredomFascination) * BoredomFascinationInfluence +
                emotion.GetEmotionValue(EmotionType.DispiritedEncouraged) * DispiritedEncouragedInfluence +
                emotion.GetEmotionValue(EmotionType.TerrorEnchantment) * TerrorEnchantmentInfluence +
                emotion.GetEmotionValue(EmotionType.FrustrationEuphoria) * FrustrationEuphoriaInfluence +
                emotion.GetEmotionValue(EmotionType.AnxietyConfidence) * AnxietyConfidenceInfluence;

            float personalityInfluence =
                personality.GetTraitValue(PersonalityType.Openness) * OpennessInfluence +
                personality.GetTraitValue(PersonalityType.Agreeableness) * AgreeablenessInfluence +
                personality.GetTraitValue(PersonalityType.Conscientiousness) * ConscientiousnessInfluence +
                personality.GetTraitValue(PersonalityType.Extraversion) * ExtraversionInfluence +
                personality.GetTraitValue(PersonalityType.Neuroticism) * NeuroticismInfluence;

            return (emotionInfluence + personalityInfluence + 1.0f) * _priority;
        }

        public override bool CanExecute()
        {
            return executionStatus == TaskStatus.Inactive;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            executionStatus = childStatus;
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
        }

        public override void OnEnd()
        {
            executionStatus = TaskStatus.Inactive;
        }
    }
}
using ClassRoomVR;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{

    [TaskDescription("Determine Influence.")]
    [TaskIcon("{SkinColor}CooldownIcon.png")]
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
        public float JoyInfluence = 0;
        [Range(0, 1)]
        public float SadnessInfluence = 0;
        [Range(0, 1)]
        public float FearInfluence = 0;
        [Range(0, 1)]
        public float AngerInfluence = 0;
        [Range(0, 1)]
        public float SurpriseInfluence = 0;
        [Range(0, 1)]
        public float DisgustInfluence = 0;

        public bool onlyPriority;
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override float GetPriority()
        {
            if (onlyPriority) return _priority;

            GameObject targetGameObject = gameObject;
            //para generalizar se puede crear un componente especifico TODO
            Emotion emotion = targetGameObject.GetComponent<Student>().GetEmotion();
            Personality personality = targetGameObject.GetComponent<Student>().getPersonality();
            float emotionInfluence = emotion.GetEmotionValue(EmotionType.Joy) * JoyInfluence + emotion.GetEmotionValue(EmotionType.Sadness) * SadnessInfluence +
                emotion.GetEmotionValue(EmotionType.Fear) * FearInfluence + emotion.GetEmotionValue(EmotionType.Anger) * AngerInfluence +
               emotion.GetEmotionValue(EmotionType.Surprise) * SurpriseInfluence + emotion.GetEmotionValue(EmotionType.Disgust) * DisgustInfluence;
            float cont1 = (JoyInfluence + SadnessInfluence + FearInfluence + AngerInfluence + SurpriseInfluence + DisgustInfluence);

            float personalityInfluence = personality.GetTraitValue(PersonalityType.Openness) * OpennessInfluence + personality.GetTraitValue(PersonalityType.Agreeableness) * AgreeablenessInfluence +
               personality.GetTraitValue(PersonalityType.Conscientiousness) * ConscientiousnessInfluence + personality.GetTraitValue(PersonalityType.Extraversion) * ExtraversionInfluence + personality.GetTraitValue(PersonalityType.Neuroticism) * NeuroticismInfluence;
            float cont2 = OpennessInfluence + AgreeablenessInfluence + ConscientiousnessInfluence + ExtraversionInfluence + NeuroticismInfluence;


            return (emotionInfluence + personalityInfluence) * _priority / (cont1 + cont2);
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
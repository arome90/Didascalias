using ClassRoomVR;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{

    [TaskDescription("Determine Influence.")]
    //[TaskIcon("{SkinColor}CooldownIcon.png")]

    public class EmoInfluence : Decorator
    {
        [Range(0, 1)]
        public float _priority = 1;
        [Range(0, 1)]
        public float OpennessInfluence=0;
        [Range(0, 1)]
        public float ConscientiousnessInfluence=0;
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


        public override float GetPriority()
        {

            GameObject targetGameObject = this.gameObject;
            //para generalizar se puede crear un componente especifico TODO
            Emotion emotion = targetGameObject.GetComponent<Student>().GetEmotion();
            Personality personality= targetGameObject.GetComponent<Student>().getPersonality();
            float emotionInfluence = emotion.Joy * JoyInfluence + emotion.Sadness * SadnessInfluence +
                emotion.Fear * FearInfluence + emotion.Anger * AngerInfluence +
                emotion.Surprise * SurpriseInfluence + emotion.Disgust * DisgustInfluence;
            float cont1 = (JoyInfluence + SadnessInfluence + FearInfluence + AngerInfluence + SurpriseInfluence + DisgustInfluence);

            float personalityInfluence = personality.Openness * OpennessInfluence + personality.Agreeableness * AgreeablenessInfluence +
                personality.Conscientiousness * ConscientiousnessInfluence + personality.Extraversion * ExtraversionInfluence + personality.Neuroticism * NeuroticismInfluence;
            float cont2 = OpennessInfluence + AgreeablenessInfluence + ConscientiousnessInfluence + ExtraversionInfluence + NeuroticismInfluence;


            return (emotionInfluence+personalityInfluence)*_priority/ (cont1+cont2);
        }
    }
}
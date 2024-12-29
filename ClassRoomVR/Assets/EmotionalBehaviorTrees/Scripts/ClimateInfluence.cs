using ClassRoomVR;
using Unity.VisualScripting;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Emo
{
    [TaskCategory("Emo")]
    [TaskDescription("Hace que el clima influencie las emociones")]
    public class ClimateInfluence : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;

        private Emotion emotion;

        [Range(0, 1)]
        public float JoyInfluence = 0f;
        [Range(0, 1)]
        public float SadnessInfluence = 0f;
        [Range(0, 1)]
        public float FearInfluence = 1f;
        [Range(0, 1)]
        public float AngerInfluence = 0f;
        [Range(0, 1)]
        public float SurpriseInfluence = 0f;
        [Range(0, 1)]
        public float DisgustInfluence = 0f;


        [Range(0, 1)]
        public float AnxietyConfidenceInfluence = 0f;
        [Range(0, 1)]
        public float BoredomFascinationInfluence = 1f;
        [Range(0, 1)]
        public float FrustrationEuphoriaInfluence = 0f;
        [Range(0, 1)]
        public float DispiritedEncouragedInfluence = 0f;
        [Range(0, 1)]
        public float TerrorEnchantmentInfluence = 0f;

        public override void OnStart()
        {
            emotion = gameObject.GetComponent<Emotion>();
        }

        public override TaskStatus OnUpdate()
        {
            if (emotion == null)
            {
                Debug.LogWarning("Emotion is null");
                return TaskStatus.Failure;
            }

            //float climate = ClimateManager.Instance.environmentalClimate;

            //emotion.Joy += climate * JoyInfluence;
            //emotion.Sadness += climate * SadnessInfluence;
            //emotion.Fear += climate * FearInfluence;
            //emotion.Anger += climate * AngerInfluence;
            //emotion.Sadness += climate * SurpriseInfluence;
            //emotion.Disgust += climate * DisgustInfluence;

            //emotion.AnxietyConfidence += climate * AnxietyConfidenceInfluence;
            //emotion.BoredomFascination += climate * BoredomFascinationInfluence;
            //emotion.FrustrationEuphoria += climate * FrustrationEuphoriaInfluence;
            //emotion.DispiritedEncouraged += climate * DispiritedEncouragedInfluence;
            //emotion.TerrorEnchantment += climate * TerrorEnchantmentInfluence;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
        }
    }
}
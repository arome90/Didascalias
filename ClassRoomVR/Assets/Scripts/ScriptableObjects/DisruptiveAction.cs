using BehaviorDesigner.Runtime;
using Meta.WitAi.Composer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "DisruptiveAction", menuName = "ScriptableObject/DisruptiveAction", order = 2)]
    public class DisruptiveAction : ScriptableObject
    {
        public AnimationClip problematicsAnimation;
        public AudioClip situationAudioMasculine;
        public AudioClip situationAudioFeminine;

        public AudioClip classLaughter;
        public AudioClip noise;
        public bool laughter;

        [Tooltip("Position to move to")]
        public Positions position;

        public float reactionTime;

        public GameObject behaviorHolder;

        public Actions action;
    }
}

using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClassRoomVR
{
    // Always update according to the list of positions where students can move
    public enum Positions
    {
        None = -1,
        FrontSide,
        BackCorner,
        Doors
    }

    [CreateAssetMenu(fileName = "DisruptiveAction", menuName = "ScriptableObject/DisruptiveAction", order = 2)]
    public class DisruptiveAction : ScriptableObject
    {
        public AnimationClip problematicsAnimation;
        public AudioClip situationAudioMasculine;
        public AudioClip situationAudioFeminine;

        public AudioClip classLaughter;
        public AudioClip noise;
        public bool laughter;

        [Tooltip("Number of people involved in the disruptive action")]
        public int numStudents = 1;

        [Tooltip("Position to move to")]
        public Positions position;

        public float reactionTime;

        public GameObject behaviorHolder;

        //[Tooltip("Special behavior of the scene at any time during the path selection")]
        //public UnityEngine.Events.UnityEvent specificBehavior;
    }
}

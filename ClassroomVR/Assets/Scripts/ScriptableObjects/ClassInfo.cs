using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/ClassInfo", order = 1)]
    public class ClassInfo : ScriptableObject
    {
        [Header("Class information used to generate scenes")]

        [Tooltip("Names of male students")]
        public string[] maleStudentNames;

        [Tooltip("Names of female students")]
        public string[] femaleStudentNames;

        [Tooltip("Prefabs for male students")]
        public GameObject[] maleStudentPrefabs;

        [Tooltip("Prefabs for female students")]
        public GameObject[] femaleStudentPrefabs;


        [Tooltip("Animator controller for students")]
        public RuntimeAnimatorController studentAnimatorController;

        [Tooltip("Idle animation (sitting) for students")]
        public AnimationClip idleAnimation;
    }
}

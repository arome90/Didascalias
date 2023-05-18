using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/ClassInfo", order = 1)]
    public class ClassInfo : ScriptableObject
    {
        [Header("Class information used to generate scenes")]

        [Tooltip("Male student names")]
        public string[] boysNames;

        [Tooltip("Female student names")]
        public string[] girlsNames;

        [Tooltip("Male student prefabs")]
        public GameObject[] boysPrefabs;

        [Tooltip("Female student prefabs")]
        public GameObject[] girlsPrefabs;

        [Tooltip("Animator controller for students")]
        public RuntimeAnimatorController studentAnimator;

        [Tooltip("Idle animation (sitting) for students")]
        public AnimationClip idleAnim;
    }
}

using UnityEngine;
using UnityEngine.Events;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/PathPackage", order = 1)]
    public class PathPackage : ScriptableObject
    {
        [Header("Required path information")]

        [Tooltip("Info on what to do to activate this path")]
        public string pathInfo;

        [Tooltip("Keywords")]
        public string[] keyWords;

        // There should only be one path of this type in each scene; otherwise, the first one found will be used
        [Tooltip("Get close to the problematic student")]
        public bool getClose;

        // There should only be one path of this type in each scene; otherwise, the first one found will be used
        [Tooltip("Boolean indicating if it is an ignore path")]
        public bool ignore;

        [Tooltip("Final class reaction audio")]
        public AudioClip audio;

        [Tooltip("Class reaction animation to the teacher's response")]
        public AnimationClip pathClassAnimation;
        [Tooltip("Problematic students' reaction animation to the teacher's response")]
        public AnimationClip pathProbAnimation;

        // A prefab must be created, located in "Resources/prefabs/ScenesBeheviours," to which a script with the desired methods is added
        [Tooltip("Special behavior of the scene after choosing the path")]
        public UnityEvent specificBehavior;

        [Tooltip("Final feedback")]
        public string feedbackPath;

        [Tooltip("Final feedback audio")]
        public AudioClip finalFeedback;

        [Tooltip("Boolean indicating if it is a correct path")]
        public bool correctPath;

        [Tooltip("Bell after class")]
        public AudioClip afterClassBell;
    }
}

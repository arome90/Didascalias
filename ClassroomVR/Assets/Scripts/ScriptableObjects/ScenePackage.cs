using UnityEngine;
using UnityEngine.Events;

//---------------------------------------------------------------------
// ScriptableObjects can be created in Resources
namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Scene", menuName = "ScriptableObject/ScenePackage", order = 4)]
    public class ScenePackage : ScriptableObject
    {
        //-----
        [Header("Class Generation")]
        [Tooltip("Total number of students, maximum 30")]
        public int studentCount;

        [Tooltip("Tutorial student")]
        public int tutorialStudent;

        [Tooltip("Problematic students among the students")]
        public int problematicStudents;

        [Tooltip("Problematic students together")]
        public bool problematicTogether;

        [Tooltip("Number of groups, 0 if groups are not desired")]
        public int groupCount = 0;

        //------
        [Header("Situation-Specific Resources")]
        [Tooltip("Message with initial information about the situation")]
        public string initialMessage;

        [Tooltip("Audio clip to play as scene context")]
        public AudioClip contextClip;

        [Tooltip("Time to teach before the situation is triggered")]
        public float timeToStart = 0;

        [Tooltip("Time to react to the situation, 0 for maxFloat")]
        public float timeToReact = 10.0f;

        [Tooltip("Animations for 'critical situation', list in execution order")]
        public AnimationClip problematicsAnimation;

        [Tooltip("Audios needed for 'critical situation' (masculine), list in execution order")]
        public AudioClip situationAudioMasculine;

        [Tooltip("Audios needed for 'critical situation' (feminine), list in execution order")]
        public AudioClip situationAudioFeminine;

        [Tooltip("Class reaction audio to the critical situation")]
        public AudioClip classReactionAudio;

        // Entry and exit bells
        [Tooltip("Bell before class")]
        public AudioClip beforeClassBell;

        [Tooltip("Mix bell before class")]
        public AudioClip mixBeforeClassBell;

        [Tooltip("Bell after class")]
        public AudioClip afterClassBell;

        // TODO
        // A prefab should be created, located in "Resources/prefabs/ScenesBehaviors", to which a script with desired methods is added
        [Tooltip("Special behavior of the scene at any execution moment during path selection")]
        public UnityEvent specificBehavior;

        //-----
        [Header("Possible Paths for the Teacher")]
        public PathPackage[] paths;

        public GameObject scene;
    }
}

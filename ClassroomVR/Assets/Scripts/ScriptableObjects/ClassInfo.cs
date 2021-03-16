using UnityEngine;

namespace ClassRoomVR {
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/ClassInfo", order = 1)]
    public class ClassInfo : ScriptableObject {
        [Header("Informacion de la clase que se utilizara para generar la escenas")]

        [Tooltip("Nombres de alumnos masculinos")]
        public string[] boysNames;
        [Tooltip("Nombres de alumnos femeninos")]
        public string[] girlsNames;

        [Tooltip("Prefabs de alumnos masculinos")]
        public GameObject[] boysPrefabs;
        [Tooltip("Prefabs de alumnos femeninos")]
        public GameObject[] girlsPrefabs;

        [Tooltip("Animator controller de los estudiantes")]
        public RuntimeAnimatorController studentAnimator;

        [Tooltip("Animacion 'idle' (sitting) de los estudiantes")]
        public AnimationClip idleAnim;

        [Tooltip("Prefab del profesor")]
        public GameObject teacher;
    }
}
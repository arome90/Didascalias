using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/ClassInfo", order = 0)]
    public class ClassInfo : ScriptableObject
    {
        [Header("Class information used to generate scenes")]

        [Tooltip("Names of male students")]
        public string[] maleStudentNames;

        [Tooltip("Names of female students")]
        public string[] femaleStudentNames;

    }
}

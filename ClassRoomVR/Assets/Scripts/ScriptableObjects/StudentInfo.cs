using UnityEditor;
using UnityEngine;

namespace ClassRoomVR
{

    [CreateAssetMenu(fileName = "Student", menuName = "ScriptableObject/StudentInfo", order = 6)]
    public class StudentInfo : ScriptableObject
    {
        [SerializeField] private string _nameStudent;
        [SerializeField] private Gender _gender;
        [SerializeField] private bool _hasDisability;
        [SerializeField] private OriginInfo _origin;

        public string Name => string.IsNullOrEmpty(_nameStudent) ? name : _nameStudent;
        public Gender Gender => _gender;
        public OriginInfo Origin => _origin;
        public bool Disability => _hasDisability;

#if UNITY_EDITOR
        [CustomEditor(typeof(StudentInfo))]
        public class StudentInfoEditor : Editor
        {
            private void OnEnable()
            {
                serializedObject.FindProperty("_nameStudent").stringValue = target.name;
                serializedObject.ApplyModifiedProperties();

            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                DrawDefaultInspector();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
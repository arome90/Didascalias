using UnityEditor;
using UnityEngine;

namespace ClassRoomVR
{

    [CreateAssetMenu(fileName = "Student", menuName = "ScriptableObject/StudentInfo", order = 6)]
    public class StudentInfo2 : ScriptableObject
    {
        [SerializeField] private string _nameStudent;
        [SerializeField] private Gender2 _gender;
        [SerializeField] private bool _hasDisability;
        [SerializeField] private OriginInfo2 _origin;

        public string Name => string.IsNullOrEmpty(_nameStudent) ? name : _nameStudent;
        public Gender2 Gender => _gender;
        public OriginInfo2 Origin => _origin;
        public bool Disability => _hasDisability;

#if UNITY_EDITOR
        [CustomEditor(typeof(StudentInfo2))]
        public class StudentInfoEditor2 : Editor
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
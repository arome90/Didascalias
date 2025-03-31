using UnityEngine;
using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "NewClassSettings", menuName = "ScriptableObject/ClassSettings", order = 4)]
    public class ClassSettings : ScriptableObject
    {
        [Header("General Settings")]
        [Range(1, 30)]
        [SerializeField]
        private int _numStudents; // Usamos prefijo "_" para variables privadas
        public int NumStudents { get => _numStudents; set => _numStudents = value; }

        [SerializeField]
        private LocalizedString _name;
        public LocalizedString Name { get => _name; }

        [SerializeField]
        private Age2 _age;
        public Age2 Age { get => _age; set => _age = value; }

        [Header("Class Structure")]
        [SerializeField]
        private StructureMode2 _structureMode;
        public StructureMode2 StructureMode { get => _structureMode; set => _structureMode = value; }

        [Header("Generation Mode")]
        [SerializeField]
        private GenerateMode2 _mode;
        public GenerateMode2 Mode { get => _mode; set => _mode = value; }

        [SerializeField]
        private StudentInfo2[] _students;
        public StudentInfo2[] Students { get => _students; set => _students = value; }

        [Header("Gender Distribution")]
        [SerializeField]
        private int _numMen;
        public int NumMen { get => _numMen; set => _numMen = value; }

        [SerializeField]
        private int _numWomen;
        public int NumWomen { get => _numWomen; set => _numWomen = value; }

        [Header("Classroom Layout")]
        [SerializeField]
        private int _numDesks;
        public int NumDesks { get => _numDesks; set => _numDesks = value; }

        [SerializeField]
        private bool _fillEmptyDesks = true;
        public bool FillEmptyDesks { get => _fillEmptyDesks; set => _fillEmptyDesks = value; }

        [Range(1.0f, 3.8f)]
        [SerializeField]
        private float _radius;
        public float Radius { get => _radius; set => _radius = value; }

        [Range(-180f, 360f)]
        [SerializeField]
        private float _degrees;
        public float Degrees { get => _degrees; set => _degrees = value; }

        [SerializeField]
        private int _columns;
        public int Columns { get => _columns; set => _columns = value; }

        [SerializeField]
        private int _rows;
        public int Rows { get => _rows; set => _rows = value; }

#if UNITY_EDITOR
        [CustomEditor(typeof(ClassSettings))]
        public class ClassSettingsEditor : Editor
        {
            private SerializedProperty[] _properties;

            private void OnEnable()
            {
                string[] propertyNames = {
                    "_numStudents", "_numMen", "_numWomen", "_age", "_structureMode",
                    "_mode", "_students", "_radius", "_degrees", "_columns", "_rows", "_numDesks",
                    "_name"
                };
                _properties = new SerializedProperty[propertyNames.Length];
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    _properties[i] = serializedObject.FindProperty(propertyNames[i]);
                }
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                DrawProperty(_properties[3], "Age");
                DrawProperty(_properties[11], "Number of Desks");
                DrawProperty(_properties[12], "Name");

                DrawEnumProperty(_properties[4], "Structure", typeof(StructureMode2));
                DrawStructureProperties((StructureMode2)_properties[4].enumValueIndex);

                DrawEnumProperty(_properties[5], "Mode", typeof(GenerateMode2));
                DrawModeProperties((GenerateMode2)_properties[5].enumValueIndex);

                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
            }

            private void DrawEnumProperty(SerializedProperty prop, string label, System.Type enumType)
            {
                if (!enumType.IsEnum)
                {
                    Debug.LogError($"{enumType} is not an Enum type.");
                    return;
                }
                Enum newEnumValue = EditorGUILayout.EnumPopup(label, (Enum)Enum.ToObject(enumType, prop.enumValueIndex));
                prop.enumValueIndex = Convert.ToInt32(newEnumValue);
            }

            private void DrawStructureProperties(StructureMode2 structureMode)
            {
                switch (structureMode)
                {
                    case StructureMode2.Fila:
                        DrawProperty(_properties[9], "Columns");
                        DrawProperty(_properties[10], "Rows");
                        break;
                    case StructureMode2.U:
                    case StructureMode2.Circular:
                        DrawProperty(_properties[7], "Radius");
                        if (structureMode == StructureMode2.Circular)
                        {
                            DrawProperty(_properties[8], "Degrees");
                        }
                        break;
                }
            }

            private void DrawModeProperties(GenerateMode2 mode)
            {
                switch (mode)
                {
                    case GenerateMode2.Random:
                        DrawProperty(_properties[0], "Number of Students");
                        break;
                    case GenerateMode2.Personalized:
                        DrawProperty(_properties[0], "Number of Students");
                        DrawProperty(_properties[6], "Personalized Students");
                        if (_properties[6].arraySize > _properties[0].intValue)
                        {
                            _properties[6].arraySize = _properties[0].intValue;
                        }
                        break;
                    case GenerateMode2.Gender:
                        DrawProperty(_properties[2], "Number of Women");
                        DrawProperty(_properties[1], "Number of Men");
                        break;
                }
            }

            private void DrawProperty(SerializedProperty prop, string label)
            {
                EditorGUILayout.PropertyField(prop, new GUIContent(label));
            }
        }
#endif
    }
}

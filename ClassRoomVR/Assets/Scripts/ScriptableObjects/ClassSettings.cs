using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClassRoomVR
{

    [CreateAssetMenu(fileName = "NewClassSettings", menuName = "ScriptableObject/ClassSettings", order = 5)]
    public class ClassSettings : ScriptableObject
    {

        [Range(1, 30)]
        [SerializeField]
        private int numStudents;
        public int NumStudents
        {
            get { return numStudents; }
            set { numStudents = value; }
        }
        [SerializeField]
        private Age age;
        public Age Age
        {
            get { return age; }
            set { age = value; }
        }
        [SerializeField]
        private StructureMode structureMode;
        public StructureMode StructureMode
        {
            get { return structureMode; }
            set { structureMode = value; }
        }

        [SerializeField]
        private GenerateMode mode;
        public GenerateMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        [SerializeField]
        private StudentInfo[] students;
        public StudentInfo[] Students
        {
            get { return students; }
            set { students = value; }
        }

        [SerializeField]
        private int numMen;
        public int NumMen
        {
            get { return numMen; }
            set { numMen = value; }
        }

        [SerializeField]
        private int numWomen;
        public int NumWomen
        {
            get { return numWomen; }
            set { numWomen = value; }
        }

        [SerializeField]
        private bool areDesksEmpty;
        public bool AreDesksEmpty
        {
            get { return areDesksEmpty; }
            set { areDesksEmpty = value; }
        }


        [SerializeField]
        private int numDesks;
        public int NumDesks
        {
            get { return numDesks; }
            set { numDesks = value; }
        }


        [Range(1.0f, 3.4f)]
        [SerializeField]
        private double radius;
        public double Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [Range(-180f, 360f)]
        [SerializeField]
        private double degrees;
        public double Degrees
        {
            get { return degrees; }
            set { degrees = value; }
        }

        [SerializeField]
        private int columns;
        public int Columns
        {
            get { return columns; }
            set { columns = value; }
        }
        [SerializeField]
        private int rows;
        public int Rows
        {
            get { return rows; }
            set { rows = value; }
        }
    



#if UNITY_EDITOR

        [CustomEditor(typeof(ClassSettings))]
        public class ClassSettingsEditor : Editor
        {
            private SerializedProperty numStudentsProp;
            private SerializedProperty numMenProp;
            private SerializedProperty numWomenProp;
            private SerializedProperty ageProp;
            private SerializedProperty structureClassProp;
            private SerializedProperty modeProp;
            private SerializedProperty studentsProp;
            private SerializedProperty areDesksEmptyProp;
            private SerializedProperty radiusProp;
            private SerializedProperty degreesProp;
            private SerializedProperty columnsProp;
            private SerializedProperty rowsProp;
            private SerializedProperty numDesksProp;

            private void OnEnable()
            {
                numStudentsProp = serializedObject.FindProperty("numStudents");
                numMenProp = serializedObject.FindProperty("numMen");
                numWomenProp = serializedObject.FindProperty("numWomen");
                ageProp = serializedObject.FindProperty("age");
                structureClassProp = serializedObject.FindProperty("structureMode");
                modeProp = serializedObject.FindProperty("mode");
                studentsProp = serializedObject.FindProperty("students");
                areDesksEmptyProp = serializedObject.FindProperty("areDesksEmpty");
                radiusProp = serializedObject.FindProperty("radius");
                degreesProp = serializedObject.FindProperty("degrees");
                columnsProp = serializedObject.FindProperty("columns");
                rowsProp = serializedObject.FindProperty("rows");
                numDesksProp = serializedObject.FindProperty("numDesks");
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();


                EditorGUILayout.PropertyField(ageProp);
                EditorGUILayout.PropertyField(numDesksProp);
                EditorGUILayout.PropertyField(areDesksEmptyProp);

                structureClassProp.enumValueIndex = (int)(StructureMode)EditorGUILayout.EnumPopup("Structure", (StructureMode)structureClassProp.enumValueIndex);

                switch ((StructureMode)structureClassProp.enumValueIndex)
                {
                    case StructureMode.Fila:
                        EditorGUILayout.PropertyField(columnsProp);
                        EditorGUILayout.PropertyField(rowsProp);
                        break;
                    case StructureMode.U:
                        EditorGUILayout.PropertyField(radiusProp);
                        break;
                    case StructureMode.Circular:
                        EditorGUILayout.PropertyField(radiusProp);
                        EditorGUILayout.PropertyField(degreesProp);
                        break;
                    case StructureMode.UnPasillo:
                    case StructureMode.DosPasillos:
                        EditorGUILayout.PropertyField(rowsProp);
                        break;
                }

                modeProp.enumValueIndex = (int)(GenerateMode)EditorGUILayout.EnumPopup("Mode", (GenerateMode)modeProp.enumValueIndex);

                switch ((GenerateMode)modeProp.enumValueIndex)
                {
                    case GenerateMode.Random:
                        EditorGUILayout.PropertyField(numStudentsProp);
                        break;
                    case GenerateMode.Personalizado:
                        EditorGUILayout.PropertyField(numStudentsProp);
                        EditorGUILayout.PropertyField(studentsProp, new GUIContent("Personalized Students"));
                        if (studentsProp.arraySize >= numStudentsProp.intValue)
                        {
                            studentsProp.arraySize = numStudentsProp.intValue;
                        }
                        break;
                    case GenerateMode.Gender:
                        EditorGUILayout.PropertyField(numWomenProp);
                        EditorGUILayout.PropertyField(numMenProp);
                        break;
                }

                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}


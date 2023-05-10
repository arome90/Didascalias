using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClassRoomVR
{
    public enum GenerateMode { Random, Personalizado, Gender };
    public enum Age { Primero, Segundo, Tercero };
    public enum StructureMode { Fila, U ,Circular,UnPasillo, DosPasillos};

    [CreateAssetMenu(fileName = "New ClassSettings", menuName = "ScriptableObject/ClassSettings", order = 5)]
    public class ClassSettings : ScriptableObject
    {

        [Range(1, 30)]
        [SerializeField]
        private int numStu;

        [SerializeField]
        private Age edad;

        [SerializeField]
        private StructureMode structureClass;

        [SerializeField]
        private GenerateMode mode;

        [SerializeField]
        private StudentInfo[] students;



        public int men;

        public int women;

        public bool desksEmpties;

        public int numDesks;

        //Circular Structure
        [Range(1.0f,3.5f)]public float radius=3.5f;
        [Range(-180f, 360f)] public float grades = 360.0f;
        //Mode normal
        public int columns = 5,rows;

        public int NumStu
        {
            get { return numStu; }
            set { numStu = value; }
        }

        public Age Edad
        {
            get { return edad; }
            set { edad = value; }

        }

        public StructureMode StructureClass
        {
            get { return structureClass; }
            set { structureClass = value; }

        }

        public GenerateMode Mode
        {
            get { return mode; }
            set { mode = value; }

        }

        public StudentInfo[] Students
        {
            get { return students; }
            set { students = value; }

        }

       

        #region Editor
#if UNITY_EDITOR

        [CustomEditor(typeof(ClassSettings))]
        public class StudentsSettingsEditor : Editor
        {
           
           // private ClassSettings set;
            private SerializedProperty s_num;
            private SerializedProperty s_edad;
            private SerializedProperty s_str;
            private SerializedProperty s_mode;
            private SerializedProperty s_stu;
            private SerializedProperty s_numMen;
            private SerializedProperty s_numWom;
            private SerializedProperty s_desksEmpties;
            private SerializedProperty s_radius;
            private SerializedProperty s_grades;
            private SerializedProperty s_columns;
            private SerializedProperty s_rows;
            private SerializedProperty s_numDesks;



            private void OnEnable()
            {
                s_num = serializedObject.FindProperty("numStu");
                s_numMen = serializedObject.FindProperty("men");
                s_numWom = serializedObject.FindProperty("women");
                s_edad = serializedObject.FindProperty("edad");
                s_str = serializedObject.FindProperty("structureClass");
                s_mode = serializedObject.FindProperty("mode");
                s_stu = serializedObject.FindProperty("students");
                s_desksEmpties = serializedObject.FindProperty("desksEmpties");
                s_radius = serializedObject.FindProperty("radius");
                s_grades = serializedObject.FindProperty("grades");
                s_columns = serializedObject.FindProperty("columns");
                s_rows = serializedObject.FindProperty("rows");
                s_numDesks = serializedObject.FindProperty("numDesks");
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
               
                EditorGUILayout.PropertyField(s_edad);
                EditorGUILayout.PropertyField(s_numDesks);
                EditorGUILayout.PropertyField(s_desksEmpties);
                

                s_str.enumValueIndex = (int)(StructureMode)EditorGUILayout.EnumPopup("Structure", (StructureMode)s_str.enumValueIndex);

                switch (s_str.enumValueIndex) 
                {
                    case (int)StructureMode.Fila:
                        EditorGUILayout.PropertyField(s_columns);
                        EditorGUILayout.PropertyField(s_rows);
                        break;
                    case (int)StructureMode.U:
                        EditorGUILayout.PropertyField(s_radius);
                        break;
                    case (int)StructureMode.Circular:
                        EditorGUILayout.PropertyField(s_radius);
                        EditorGUILayout.PropertyField(s_grades);
                        break;
                    case (int)StructureMode.UnPasillo:
                    case (int)StructureMode.DosPasillos:
                        EditorGUILayout.PropertyField(s_rows);
                        break;
                }
                

                s_mode.enumValueIndex = (int)(GenerateMode)EditorGUILayout.EnumPopup("Mode", (GenerateMode)s_mode.enumValueIndex);

                switch (s_mode.enumValueIndex) 
                {
                    case (int)GenerateMode.Random:
                        EditorGUILayout.PropertyField(s_num);
                        break;
                    case (int)GenerateMode.Personalizado:
                        EditorGUILayout.PropertyField(s_num);
                        EditorGUILayout.PropertyField(s_stu, new GUIContent("Personalized Students"));
                        if (s_stu.arraySize >= s_num.intValue)
                        {
                            s_stu.arraySize = s_num.intValue;
                        }
                        break;
                    case (int)GenerateMode.Gender:
                        EditorGUILayout.PropertyField(s_numWom);
                        EditorGUILayout.PropertyField(s_numMen); break;
                }


                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
            }

        }

       
    
        }
        #endif
        #endregion

}
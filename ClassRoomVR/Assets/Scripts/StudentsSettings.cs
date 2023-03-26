using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "New StudentsSettings", menuName = "ScriptableObject/StudentsSettings", order = 5)]
    public class StudentsSettings : ScriptableObject
    {

        public enum GenerateMode { Random, Personalizado, Gender };
        public enum Age { Primero, Segundo, Tercero };
        public enum Structure { Fila, U };

        [Range(1, 30)]
        [SerializeField]
        private int numStu;

        [SerializeField]
        private Age edad;

        [SerializeField]
        private Structure structureClass;

        [SerializeField]
        private GenerateMode mode;

        [SerializeField]
        private StudentInfo[] students;


        [SerializeField]
        public int men;

        [SerializeField]
        public int women;




        public int NumStu
        {
            get { return numStu; }
        }

        public Age Edad
        {
            get { return edad; }
        }

        public Structure StructureClass
        {
            get { return structureClass; }
        }

        public GenerateMode Mode
        {
            get { return mode; }
        }

        public StudentInfo[] Students
        {
            get { return students; }
        }

        public void ChangeMen(Slider s) 
        {
            men = (int)s.value;

        }
        public void ChangeWomen(Slider s)
        {
            women = (int)s.value;
        }

        #region Editor
#if UNITY_EDITOR

        [CustomEditor(typeof(StudentsSettings))]
        public class StudentsSettingsEditor : Editor
        {
           
           // private StudentsSettings set;
            private SerializedProperty s_num;
            private SerializedProperty s_edad;
            private SerializedProperty s_str;
            private SerializedProperty s_mode;
            private SerializedProperty s_stu;
            private SerializedProperty s_numMen;
            private SerializedProperty s_numWom;

            private void OnEnable()
            {
                s_num = serializedObject.FindProperty("numStu");
                s_numMen = serializedObject.FindProperty("men");
                s_numWom = serializedObject.FindProperty("women");
                s_edad = serializedObject.FindProperty("edad");
                s_str = serializedObject.FindProperty("structureClass");
                s_mode = serializedObject.FindProperty("mode");
                s_stu = serializedObject.FindProperty("students");
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
               
                EditorGUILayout.PropertyField(s_edad);
                EditorGUILayout.PropertyField(s_str);

                s_mode.enumValueIndex = (int)(GenerateMode)EditorGUILayout.EnumPopup("Mode", (GenerateMode)s_mode.enumValueIndex);

                if (s_mode.enumValueIndex is (int)GenerateMode.Random)
                {
                    EditorGUILayout.PropertyField(s_num);

                }


                if (s_mode.enumValueIndex is (int)GenerateMode.Personalizado)
                {
                    EditorGUILayout.PropertyField(s_num);
                    EditorGUILayout.PropertyField(s_stu, new GUIContent("Personalized Students"));
                   if(s_stu.arraySize >= s_num.intValue) 
                   {
                        s_stu.arraySize = s_num.intValue;
                   }
                  
                }

                if (s_mode.enumValueIndex is (int)GenerateMode.Gender)
                {
                    EditorGUILayout.PropertyField(s_numWom);
                    EditorGUILayout.PropertyField(s_numMen);


                }


                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
            }

        }

       
    
        #endif
        #endregion
        }

}
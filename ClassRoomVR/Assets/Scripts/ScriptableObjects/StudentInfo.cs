using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Student", menuName = "ScriptableObject/StudentInfo", order = 4)]
    public class StudentInfo : ScriptableObject
    {

        //0 for random ??
        public enum OriginInfo {Random ,HispanicLatino, AsianPacificIslander, BlackAfricanAmerican, WhiteCaucasian, Other };
        public enum BodyInfo{Random, Body1, Body2, Body3, Body4};
        public enum GenderInfo {Random, Women, Men  };

        public string nameStudent;
        [SerializeField] GenderInfo gender;
        [SerializeField] bool hasDisability;
        [SerializeField] OriginInfo origin;
        [SerializeField] BodyInfo body;



        public string Name => nameStudent;
        public GenderInfo Gender => gender;
        public bool Disability => hasDisability;
        public OriginInfo Origen => origin;
        public BodyInfo Body => body;

        //Crear flags para todas estas variables y tener 
        //EJ :  [Flags]
       // enum Colors { Red = 1, Green = 2, Blue = 4, Yellow = 8 };

        ////Personal characteristics of children:
        //bool disability; // indicates whether the child has a disability
        //bool gifted; // indicates whether the child is gifted or talented
        //bool developmentalDelay; // indicates whether the child has a developmental delay or disorder
        //bool differentLearningPace; // indicates whether the child learns at a different pace than their peers
        //bool differentLearningStyle; // indicates whether the child has a different learning style than their peers
        //bool hyperactivity; // indicates whether the child has hyperactivity or attention-deficit/hyperactivity disorder (ADHD)
        //bool behaviorProblems; // indicates whether the child has behavior problems or conduct disorder
        //bool languageProblems; // indicates whether the child has language or communication problems
        //bool affectiveDevelopmentProblems; // indicates whether the child has problems with emotional or social development
        //float motivationLevel; // a floating-point variable to store the child's level of motivation, from 0 (low) to 1 (high)
        ////Sociocultural factors:
        //bool geographicMobility; // indicates whether the family moves frequently due to work or other reasons
        //bool ethnicMinority; // indicates whether the child belongs to an ethnic or racial minority group
        //float socioeconomicLevel; // a floating-point variable to store the family's socioeconomic level, from 0 (low) to 1 (high)
        //bool immigration; // indicates whether the family has immigrated to the country or city where the game is set
        ////Family factors:
        //bool onlyChild; // indicates whether the child is an only child
        //bool largeFamily; // indicates whether the child comes from a large family
        //bool separatedParents; // indicates whether the child's parents are separated or divorced
        //bool excessiveRules; // indicates whether the family has too many rules or restrictions
        //bool inadequateRules; // indicates whether the family has inadequate or ineffective rules
        //bool prolongedAbsences; // indicates whether one or both parents are frequently absent for long periods of time
        ////Personal characteristics of parents:
        //bool inexperiencedInParenting; // indicates whether the parent(s) have little experience in raising children
        //bool earlyParenthood; // indicates whether the parent(s) had children at a young age
        //bool busyProfessionals; // indicates whether the parent(s) have demanding professional careers
        //bool unemployed; // indicates whether the parent(s) are unemployed or have little work activity
        //bool overprotective; // indicates whether the parent(s) are overly protective of their child/children
        //bool authoritarian; // indicates whether the parent(s) have a strict, controlling parenting style
        //bool permissive; // indicates whether the parent(s) have a lenient, indulgent parenting style
        [CustomEditor(typeof(StudentInfo))]
        public class StudentInfoEditor : Editor
        {
            private SerializedProperty s_gen;
            private SerializedProperty s_dis;
            private SerializedProperty s_ori;
            private SerializedProperty s_bod;
            private SerializedProperty s_nam;

          
            private void OnEnable()
            {
                s_nam = serializedObject.FindProperty("nameStudent");
                s_gen = serializedObject.FindProperty("gender");
                s_dis = serializedObject.FindProperty("hasDisability");
                s_ori = serializedObject.FindProperty("origin");
                s_bod = serializedObject.FindProperty("body");
            }

            public override void OnInspectorGUI()
            {
                
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(s_nam);
                if (s_nam.stringValue.Length == 0) 
                {
                    s_nam.stringValue =serializedObject.targetObject.name ;
                }
                EditorGUILayout.PropertyField(s_dis);
                EditorGUILayout.PropertyField(s_gen);
                EditorGUILayout.PropertyField(s_ori);
                EditorGUILayout.PropertyField(s_bod);
                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
                
            }

            //private void SetRandomWithButton() 
            //{
            //    if (GUILayout.Button("Generate Random"))
            //    {
                   
            //    }
            //}

        }
    }

     
}
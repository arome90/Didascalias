using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Student", menuName = "ScriptableObject/StudentInfo", order = 4)]
    public class StudentInfo : ScriptableObject
    {
        [SerializeField] private string nameStudent;
        [SerializeField] private GenderInfo gender;
        [SerializeField] private bool hasDisability;
        [SerializeField] private OriginInfo origin;
        [SerializeField] private BodyInfo body;

        public string Name => nameStudent;
        public GenderInfo Gender => gender;
        public bool Disability => hasDisability;
        public OriginInfo Origin => origin;
        public BodyInfo Body => body;

        [CustomEditor(typeof(StudentInfo))]
        public class StudentInfoEditor : Editor
        {
            private SerializedProperty genderProperty;
            private SerializedProperty disabilityProperty;
            private SerializedProperty originProperty;
            private SerializedProperty bodyProperty;
            private SerializedProperty nameProperty;

            private void OnEnable()
            {
                nameProperty = serializedObject.FindProperty("nameStudent");
                genderProperty = serializedObject.FindProperty("gender");
                disabilityProperty = serializedObject.FindProperty("hasDisability");
                originProperty = serializedObject.FindProperty("origin");
                bodyProperty = serializedObject.FindProperty("body");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(nameProperty);
                if (nameProperty.stringValue.Length == 0)
                {
                    nameProperty.stringValue = serializedObject.targetObject.name;
                }
                EditorGUILayout.PropertyField(disabilityProperty);
                EditorGUILayout.PropertyField(genderProperty);
                EditorGUILayout.PropertyField(originProperty);
                EditorGUILayout.PropertyField(bodyProperty);
                EditorGUI.EndChangeCheck();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}




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

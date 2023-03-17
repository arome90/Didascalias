////C# Example (LookAtPointEditor.cs)
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(LookAtPoint))]
//[CanEditMultipleObjects]
//public class LookAtPointEditor : Editor
//{
//    SerializedProperty lookAtPoint;
//    SerializedProperty H;
//    SerializedProperty ;

//    void OnEnable()
//    {
//        lookAtPoint = serializedObject.FindProperty("lookAtPoint");
//        h = serializedObject.FindProperty("H");
//    }

//    public override void OnInspectorGUI()
//    {
//        EditorGUILayout.IntField(h,)
//        serializedObject.Update();
//        EditorGUILayout.PropertyField(lookAtPoint);
//        serializedObject.ApplyModifiedProperties();
//        if (lookAtPoint.vector3Value.y > (target as LookAtPoint).transform.position.y)
//        {
//            EditorGUILayout.LabelField("(Above this object)");
//        }
//        if (lookAtPoint.vector3Value.y < (target as LookAtPoint).transform.position.y)
//        {
//            EditorGUILayout.LabelField("(Below this object)");
//        }
//        serializedObject.Update();

//        EditorGUILayout.PropertyField(H);
//        serializedObject.ApplyModifiedProperties();

//        if (H == 0) { EditorGUILayout.LabelField("(Below this object)"); }
//    }
//}
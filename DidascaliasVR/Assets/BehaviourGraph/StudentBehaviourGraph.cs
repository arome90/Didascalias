//using System;
//using Unity.GraphToolkit.Editor;
//using UnityEditor;

//namespace Didascalia.BehaviourGraph
//{
//    [Graph(AssetExtension)]
//    [Serializable]
//    internal class StudentBehaviourGraph : Graph
//    {
//        internal const string AssetExtension = "dsbg";
//        internal const string CreateMenuPath = "Assets/Create/Graph/Student Behaviour Graph";

//        [MenuItem(CreateMenuPath, false)]
//        internal static void CreateAssetFile()
//        {
//            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<StudentBehaviourGraph>();
//        }
//    }

//    // TODO: choose a matching representation for graph nodes and transitions. Talk with @DavidRainder
//}
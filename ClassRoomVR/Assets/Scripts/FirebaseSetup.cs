//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FirebaseSetup : MonoBehaviour
//{
//    private Firebase.FirebaseApp firebase;
   
//    void Start()
//    {
//        Debug.Log("hola");
//        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
//            var dependencyStatus = task.Result;
//            if (dependencyStatus == Firebase.DependencyStatus.Available)
//            {
//                // Create and hold a reference to your FirebaseApp,
//                // where app is a Firebase.FirebaseApp property of your application class.
//                firebase = Firebase.FirebaseApp.DefaultInstance;
                
//                // Set a flag here to indicate whether Firebase is ready to use by your app.

//            }
//            else
//            {
//                UnityEngine.Debug.LogError(System.String.Format(
//                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
//                // Firebase Unity SDK is not safe to use here.
//            }
//        });
//    }


//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.U)) 
//        {
//            // Log an event with no parameters.
//            Firebase.Analytics.FirebaseAnalytics
//              .LogEvent(Firebase.Analytics.FirebaseAnalytics.EventLogin);
           
//        }
//        else if(Input.GetKeyDown(KeyCode.I)) 
//        {
//            // Log an event with a float parameter
//            Firebase.Analytics.FirebaseAnalytics
//              .LogEvent("progress", "percent", 0.4f);
//        }
//    }


//}

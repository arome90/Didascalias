//using System;
//using UnityEngine;
//using Firebase.Analytics;
//using System.Collections.Generic;
////using Unity.Services.Analytics;
//using Unity.Services.Core;

//public delegate void CustomEventDelegate(string eventName); 
//public delegate void CustomEventDelegateWithParameters(string eventName, Dictionary<string, object> parameterDictionary);
//public static class AnalyticsManager
//{


//    static CustomEventDelegate custom;
//    static CustomEventDelegateWithParameters customWithParameter;
//    public static void Start(bool useFirebase, bool useUnity)
//    {
//        InitializeServices(useFirebase, useUnity);
//    }
//    private static void InitializeServices(bool useFirebase, bool useUnity)
//    {
//        if (useFirebase)
//        {
//            CustomEventFirebase.Initialization();
//            custom += CustomEventFirebase.RecordCustomEvent;
//            customWithParameter += CustomEventFirebase.RecordCustomEventWithParameters;
//        }
//        if (useUnity)
//        {
//            CustomEventUnity.Initialization();
//            custom += CustomEventUnity.RecordCustomEvent;
//            customWithParameter += CustomEventUnity.RecordCustomEventWithParameters;
//        }
//    }

//    //Delegate[] InvocationList = custom.GetInvocationList();
//    //foreach (var item in InvocationList)
//    //{
//    //    Debug.Log($"  {item}");
//    //}




//    public static void CustomEvent(string eventName)
//    {
//        custom?.Invoke(eventName);
//    }

//    public static void CustomEventWithParameter(string eventName, string parameterName, object parameterValue)
//    {
//        customWithParameter?.Invoke(eventName, new Dictionary<string, object>() { { parameterName, parameterValue } } );
//    }

//    public static void CustomEventWithParameters(string eventName, Dictionary<string, object>parameters)
//    {
//        customWithParameter?.Invoke(eventName, parameters);
//    }

//}

//public static class CustomEventUnity
//{


//   public static async void Initialization() 
//    {
//        await UnityServices.InitializeAsync();
//        await AnalyticsService.Instance.CheckForRequiredConsents();

//        Debug.Log($"Started UGS Analytics Sample with user ID: {AnalyticsService.Instance.GetAnalyticsUserID()}");

//    }


//    public static void RecordCustomEvent(string eventName)
//    {
//        Debug.Log("evento lanzado unity");
//        AnalyticsService.Instance.CustomData(eventName);
//    }

//    public static void RecordCustomEventWithParameters(string eventName, Dictionary<string, object> parameters)
//    {
//        AnalyticsService.Instance.CustomData(eventName, parameters);
//    }
//}
//public static class CustomEventFirebase
//{
//    static Firebase.FirebaseApp fireapp;

//    public static void Initialization()
//    {
//        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
//            var dependencyStatus = task.Result;
//            if (dependencyStatus == Firebase.DependencyStatus.Available)
//            {
//                // Create and hold a reference to your FirebaseApp,
//                // where app is a Firebase.FirebaseApp property of your application class.
//                fireapp = Firebase.FirebaseApp.DefaultInstance;

//                // Set a flag here to indicate whether Firebase is ready to use by your app.
//                Debug.Log("Enabling data collection.");
//                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

//                Debug.Log("Set user properties.");
//                // Set the user's sign up method.
//                FirebaseAnalytics.SetUserProperty(
//                  FirebaseAnalytics.UserPropertySignUpMethod,
//                  "Google");
//                // Set the user ID.
//                FirebaseAnalytics.SetUserId("uber_user_510");
//                // Set default session duration values.
//                FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
//            }
//            else
//            {
//                UnityEngine.Debug.LogError(System.String.Format(
//                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
//                // Firebase Unity SDK is not safe to use here.
//            }
//        });
//    }



//    public static void RecordCustomEvent(string eventName)
//    {
//        Debug.Log("evento lanzado firebase");

//        FirebaseAnalytics.LogEvent(eventName);
//    }

//    public static void RecordCustomEventWithParameters(string eventName, Dictionary<string, object> parameters)
//    {
//        Parameter[] par = new Parameter[parameters.Count];
//        int index = 0;
//        foreach (var kvp in parameters)
//        {
//            par[index] = ConvertValueToCorrectType(kvp.Key, kvp.Value);
//            index++;
//        }

//        FirebaseAnalytics.LogEvent(eventName, par);

//    }

//    private static Parameter ConvertValueToCorrectType(string name, object value)
//    {

//        if (value is string)
//        {
//            return new Parameter(name, (string)value);
//        }
//        else if (value is long)
//        {
//            return new Parameter(name, (long)value);
//        }
//        else if (value is double)
//        {
//            return new Parameter(name, (double)value);
//        }

//        throw new ArgumentException("Tipo de valor no válido para el parámetro: " + name);
//    }

//}

////var parameters = new Dictionary<string, object>
////    {
////        { "fabulousString", "hello there" },
////        { "sparklingInt", 1337 },
////        { "tremendousLong", Int64.MaxValue },
////        { "spectacularFloat", 0.451f },
////        { "incredibleDouble", 0.000000000000000031337 },
////        { "peculiarBool", true }
////    };

////Parameter[] AchievementParameters = {
////  new Parameter(FirebaseAnalytics.ParameterAchievementID,
////                "ultimate_wizard"),
////  new Parameter(FirebaseAnalytics.ParameterCharacter, "mysterion"),
////  new Parameter(FirebaseAnalytics.ParameterLevel, currentLevel),
////};

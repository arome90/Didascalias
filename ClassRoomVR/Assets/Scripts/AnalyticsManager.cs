using System;
using UnityEditor;
using UnityEngine;
//using Firebase.Analytics;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;

public delegate void CustomEventDelegate(string eventName); 
public delegate void CustomEventDelegateWithParameters(string eventName, Dictionary<string, object> parameterDictionary);
public static class AnalyticsManager 
{
    

    static CustomEventDelegate custom;
    static CustomEventDelegateWithParameters customWithParameter;

    public static async void Start(bool firebase,bool unity)
    {
       
        if (firebase)
        {
            custom += CustomEventFirebase.RecordCustomEvent;
            customWithParameter += CustomEventFirebase.RecordCustomEventWithParameters;
        }
        if (unity)
        {
            await UnityServices.InitializeAsync();
            await AnalyticsService.Instance.CheckForRequiredConsents();

            Debug.Log($"Started UGS Analytics Sample with user ID: {AnalyticsService.Instance.GetAnalyticsUserID()}");
            custom += CustomEventUnity.RecordCustomEvent;
            customWithParameter += CustomEventUnity.RecordCustomEventWithParameters;
        }
        
        Delegate[] InvocationList = custom.GetInvocationList();
        foreach (var item in InvocationList)
        {
            Debug.Log($"  {item}");
        }

    }


    public static void CustomEvent(string eventName)
    {
        custom?.Invoke(eventName);
    }

    public static void CustomEventWithParameter(string eventName, string parameterName, object parameterValue)
    {
        customWithParameter?.Invoke(eventName, new Dictionary<string, object>() { { parameterName, parameterValue } } );
    }

    public static void CustomEventWithParameters(string eventName, Dictionary<string, object>parameters)
    {
        customWithParameter?.Invoke(eventName, parameters);
    }

}

public static class CustomEventUnity
{
    public static void RecordCustomEvent(string eventName)
    {
        Debug.Log("evento lanzado unity");
        AnalyticsService.Instance.CustomData(eventName);
    }

    public static void RecordCustomEventWithParameters(string eventName, Dictionary<string, object> parameters)
    {
        AnalyticsService.Instance.CustomData(eventName, parameters);
    }
}
public static class CustomEventFirebase
{
    public static void RecordCustomEvent(string eventName)
    {
        Debug.Log("evento lanzado firebase");

        //FirebaseAnalytics.LogEvent(eventName);
    }

    public static void RecordCustomEventWithParameters(string eventName, Dictionary<string, object> parameters)
    {
        //Parameter[] par = new Parameter[parameters.Count];
        //int index = 0;
        //foreach (var kvp in parameters)
        //{
        //    par[index] = ConvertValueToCorrectType(kvp.Key,kvp.Value);
        //    index++;
        //}

        //FirebaseAnalytics.LogEvent(eventName,par);

    }

    //private static Parameter ConvertValueToCorrectType(string name,object value)
    //{

    //    if (value is string)
    //    {
    //        return new Parameter(name, (string)value);
    //    }
    //    else if (value is long)
    //    {
    //        return new Parameter(name, (long)value);
    //    }
    //    else if (value is double)
    //    {
    //        return new Parameter(name, (double)value);
    //    }
       
    //        throw new ArgumentException("Tipo de valor no válido para el parámetro: " + name);
    //}

}

//var parameters = new Dictionary<string, object>
//    {
//        { "fabulousString", "hello there" },
//        { "sparklingInt", 1337 },
//        { "tremendousLong", Int64.MaxValue },
//        { "spectacularFloat", 0.451f },
//        { "incredibleDouble", 0.000000000000000031337 },
//        { "peculiarBool", true }
//    };

//Parameter[] AchievementParameters = {
//  new Parameter(FirebaseAnalytics.ParameterAchievementID,
//                "ultimate_wizard"),
//  new Parameter(FirebaseAnalytics.ParameterCharacter, "mysterion"),
//  new Parameter(FirebaseAnalytics.ParameterLevel, currentLevel),
//};
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyLog : MonoBehaviour
{
    private string myLog;
    private Queue<string> myLogQueue = new Queue<string>();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        string newString = "\n[" + type + "]: " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        myLog = string.Empty;
        foreach (string mylog in myLogQueue)
        {
            myLog += mylog;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label(myLog);
    }
}

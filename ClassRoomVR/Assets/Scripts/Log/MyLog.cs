using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MyLog : MonoBehaviour
{
    private string myLog;
    private Queue<string> myLogQueue = new Queue<string>();
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] GameObject text2;
    [SerializeField] GameObject text3;
    [SerializeField] private InputActionProperty logAction;
    private bool _isActive = false;

    //private void OnEnable()
    //{
    //    Application.logMessageReceivedThreaded += HandleLog;
    //}

    //private void OnDisable()
    //{
    //    Application.logMessageReceivedThreaded -= HandleLog;
    //}

    private void OnEnable()
    {
        _isActive = false;
        if (logAction != null)
        {
            logAction.action.performed += LogButton;
        }
    }

    // Remove click delegate
    private void OnDisable()
    {
        if (logAction != null)
        {
            logAction.action.performed -= LogButton;
        }
    }

    private void LogButton(InputAction.CallbackContext context)
    {
        if (!_isActive)
        {
            _isActive = true;
            text2.SetActive(true);
            text3.SetActive(true);
            Application.logMessageReceivedThreaded += HandleLog;

        }
        else
        {
            _isActive = false;
            text2.SetActive(false);
            text3.SetActive(false);
            text.text = string.Empty;
            Application.logMessageReceivedThreaded -= HandleLog;
        }
    }


    public void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        string newString = "\n[" + type + "]: " + myLog;
        myLogQueue.Enqueue(newString);
        if(myLogQueue.Count > 5 ) { myLogQueue.Dequeue(); }
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
        if (_isActive)
        {
            text.text = myLog;
        }
        // GUILayout.Label(myLog);

    }

}

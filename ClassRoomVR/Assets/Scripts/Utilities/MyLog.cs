using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Manages logging and display of log messages in the UI.
/// </summary>
public class MyLog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _text2;
    [SerializeField] private GameObject _text3;
    [SerializeField] private InputActionProperty _logAction;

    private string _myLog;
    private Queue<string> _myLogQueue = new Queue<string>();
    private bool _isActive = false;

    private void OnEnable()
    {
        _isActive = false;
        if (_logAction != null)
        {
            _logAction.action.performed += LogButton;
        }
    }

    private void OnDisable()
    {
        if (_logAction != null)
        {
            _logAction.action.performed -= LogButton;
        }
    }

    /// <summary>
    /// Toggles the visibility of log display and registers or unregisters the log handler.
    /// </summary>
    /// <param name="context">The input action context.</param>
    private void LogButton(InputAction.CallbackContext context)
    {
        if (!_isActive)
        {
            _isActive = true;
            _text2.SetActive(true);
            _text3.SetActive(true);
            Application.logMessageReceivedThreaded += HandleLog;
        }
        else
        {
            _isActive = false;
            _text2.SetActive(false);
            _text3.SetActive(false);
            _text.text = string.Empty;
            Application.logMessageReceivedThreaded -= HandleLog;
        }
    }

    /// <summary>
    /// Handles log messages and updates the log queue.
    /// </summary>
    /// <param name="logString">The log message.</param>
    /// <param name="stackTrace">The stack trace of the log message.</param>
    /// <param name="type">The type of the log message.</param>
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string newString = $"\n[{type}]: {logString}";
        _myLogQueue.Enqueue(newString);

        if (_myLogQueue.Count > 5)
        {
            _myLogQueue.Dequeue();
        }

        if (type == LogType.Exception)
        {
            newString = $"\n{stackTrace}";
            _myLogQueue.Enqueue(newString);
        }

        UpdateLogText();
    }

    /// <summary>
    /// Updates the log text with the contents of the log queue.
    /// </summary>
    private void UpdateLogText()
    {
        _myLog = string.Empty;
        foreach (string log in _myLogQueue)
        {
            _myLog += log;
        }
    }

    private void OnGUI()
    {
        if (_isActive)
        {
            _text.text = _myLog;
        }
    }
}

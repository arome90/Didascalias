using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
namespace ClassRoomVR
{
    public class StringWrapper
    {
        public string Value;
    }

    public class TextPanel : MonoBehaviour
    {
        public float time=2;
        [System.Serializable]
        public class MethodInfoWrapper
        {
            public string methodName;
            public bool newLine;
            public UnityEvent<StringWrapper> callbackEvent;
        }
        public List<MethodInfoWrapper> methods = new List<MethodInfoWrapper>();

        public TextMeshProUGUI textMeshPro;

        private void Start()
        {
            InvokeRepeating(nameof(UpdateTextPanel), 1, time);
        }

      
        public void UpdateTextPanel()
        {
            textMeshPro.text = string.Empty;

            foreach (var methodInfo in methods)
            {
                StringWrapper result = new StringWrapper();
                try
                {
                    methodInfo.callbackEvent.Invoke(result);
                    if (!string.IsNullOrEmpty(methodInfo.methodName)) textMeshPro.text += methodInfo.methodName;
                    textMeshPro.text += result.Value;
                    if (methodInfo.newLine) textMeshPro.text += "\n";

                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking method {methodInfo.methodName}: {e.Message}");
                }
            }

        }
    }
}
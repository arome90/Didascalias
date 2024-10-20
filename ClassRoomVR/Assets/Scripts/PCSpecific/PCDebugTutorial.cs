#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PCDebugTutorial : MonoBehaviour
{
    [SerializeField] Button _nextButton = null;
    
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            _nextButton.onClick.Invoke();
        }
    }
}
#endif
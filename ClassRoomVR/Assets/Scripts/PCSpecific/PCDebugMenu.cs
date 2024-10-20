#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PCDebugMenu : MonoBehaviour
{
    // Script con el que podemos entrar al juego desde el menú usando el PC, sin necesidad de VR.
    [SerializeField] Button classButton;
    [SerializeField] Button tutorialButton;
    [SerializeField] Button exitButton;

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.cKey.wasPressedThisFrame)
        {
            classButton.onClick.Invoke();
        }
        else if(Keyboard.current.tKey.wasPressedThisFrame)
        {
            tutorialButton.onClick.Invoke();
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            exitButton.onClick.Invoke();
        }
    }
}
#endif
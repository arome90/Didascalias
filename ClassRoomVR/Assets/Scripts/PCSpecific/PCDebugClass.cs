using ClassRoomVR;
using UnityEngine;
using UnityEngine.InputSystem;

public class PCDebugClass : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.mKey.wasPressedThisFrame)
        {
            GameManager2.Instance.LoadMainMenu();
        }
    }
}

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
            GameManager.Instance.LoadMainMenu();
        }
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (GameManager.Instance.IsPause)
                GameManager.Instance.Continue(false);
            else 
                GameManager.Instance.Pause(false, false);
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            if (GameManager.Instance.IsPause)
                GameManager.Instance.Continue(true);
            else
                GameManager.Instance.Pause(false, true);
        }
    }
}

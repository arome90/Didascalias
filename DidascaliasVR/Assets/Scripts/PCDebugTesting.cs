#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.InputSystem;


public class PCDebugTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // FIXME: should have a field `InputActionReference testSitTogether` and subscribe to that instead of hardcoding the key
        if(Keyboard.current.leftCtrlKey.wasReleasedThisFrame)
        {
            TestSitTogether();
        }
    }

    private void TestSitTogether()
    {
        StudentManager.Instance.GenerateConflict(StudentManager.ConflictType.SitTogether, null);
    }
}

#endif
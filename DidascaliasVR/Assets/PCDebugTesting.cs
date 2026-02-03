using UnityEngine;

public class PCDebugTesting : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.LeftControl))
        {
            TestSitTogether();
        }
    }

    private void TestSitTogether()
    {
        StudentManager.Instance.GenerateConflict(StudentManager.ConflictType.SitTogether, null);
    }
}

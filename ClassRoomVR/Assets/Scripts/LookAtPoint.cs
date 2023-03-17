//C# Example (LookAtPoint.cs)
using UnityEngine;
[ExecuteInEditMode]
public class LookAtPoint : MonoBehaviour
{
    public Vector3 lookAtPoint = Vector3.zero;
    public int H;
    public int A;
    void Update()
    {
        transform.LookAt(lookAtPoint);
    }
}
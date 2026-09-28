using UnityEngine;

public class Desk : MonoBehaviour
{
    public Transform OutOfDeskTransform;
    public Transform OutOfDeskTransform_Other;
    public Transform StudentPosition;

    public Transform GetNearestOutOfDeskPosition(Transform origin)
    {
        float distance1 = (origin.position - OutOfDeskTransform.position).magnitude;
        float distance2 = (origin.position - OutOfDeskTransform_Other.position).magnitude;

        if (distance1 > distance2) return OutOfDeskTransform_Other;
        else return OutOfDeskTransform;
    }
}

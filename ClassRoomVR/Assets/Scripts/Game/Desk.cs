using UnityEngine;

public class Desk : MonoBehaviour
{
    private Vector2 position;
    private bool isOccupied;

    public bool IsOccupied { get { return isOccupied; } set { isOccupied = value; } }
    public Vector2 Position { get { return position; } set { position = value; } }

    public Vector3 GetPositionStudent() { return transform.GetChild(0).position; }

    [HideInInspector] public UnityEngine.Events.UnityEvent onCollisionChanged;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            onCollisionChanged.Invoke();
        }
    }
}

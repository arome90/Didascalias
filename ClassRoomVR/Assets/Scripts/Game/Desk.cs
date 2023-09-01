using UnityEngine;

public class Desk : MonoBehaviour
{
    private Vector2 position; // The position of the desk 
    private bool isOccupied; // Flag to indicate if the desk is occupied

    public bool IsOccupied { get => isOccupied; set => isOccupied = value; } // Property to access the occupancy status
    public Vector2 Position { get => position; set => position = value; } // Property to access the position

    // Get the position of the student sitting at the desk
    public Vector3 GetPositionStudent() => transform.GetChild(0).position;

    [HideInInspector] public UnityEngine.Events.UnityEvent onCollisionChanged; // Event invoked when collision with other desk occurs

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            onCollisionChanged.Invoke(); // Invoke the onCollisionChanged event when colliding with another desk
        }
    }
}

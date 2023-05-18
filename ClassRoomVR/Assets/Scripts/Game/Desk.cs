using UnityEngine;

public class Desk : MonoBehaviour
{
    private Vector2 position;
    private bool isOccupied;

    public bool IsOccupied { get { return isOccupied; } set { isOccupied = value; } }
    public Vector2 Position { get { return position; } set { position = value; } }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Desk"))
        {
            Debug.Log("Collision Detected");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            Debug.Log("Trigger Entered");
        }
    }
}

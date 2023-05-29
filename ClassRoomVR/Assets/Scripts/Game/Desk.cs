using UnityEngine;

public class Desk : MonoBehaviour
{
    private Vector2 position;
    
    private bool isOccupied;

    public bool IsOccupied { get { return isOccupied; } set { isOccupied = value; } }
    public Vector2 Position { get { return position; } set { position = value; } }

    public Vector3 GetPositionStudent() { return transform.GetChild(0).position; }


    //Collisiones arreglo



    [HideInInspector] public UnityEngine.Events.UnityEvent onCollisionChanged;

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.collider.gameObject.CompareTag("Desk"))
    //    {
    //        toca = true;
    //        Debug.Log("toca");
    //    }
    //}
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            
            onCollisionChanged.Invoke();
            
        }
    }
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("Desk"))
    //    {

    //        Debug.Log("sigo");
    //    }
    //}

}

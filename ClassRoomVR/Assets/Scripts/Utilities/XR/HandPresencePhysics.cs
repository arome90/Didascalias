using UnityEngine;

public class HandPresencePhysics : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        UpdatePhysics();
    }

    private void UpdatePhysics()
    {
        rb.velocity = CalculateVelocity();
        rb.angularVelocity = CalculateAngularVelocity(CalculateRotationDifference());
    }

    private Vector3 CalculateVelocity()
    {
        Vector3 positionDifference = target.position - transform.position;
        return positionDifference / Time.fixedDeltaTime;
    }

    private Quaternion CalculateRotationDifference()
    {
        return target.rotation * Quaternion.Inverse(transform.rotation);
    }

    private Vector3 CalculateAngularVelocity(Quaternion rotationDifference)
    {
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);
        return angleInDegree * rotationAxis * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}
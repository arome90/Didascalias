using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        UpdatePosition();
        UpdateRotation();
    }

    private void UpdatePosition()
    {
        Vector3 velocity = CalculateVelocity();
        rb.velocity = velocity;
    }

    private Vector3 CalculateVelocity()
    {
        Vector3 positionDifference = target.position - transform.position;
        return positionDifference / Time.fixedDeltaTime;
    }

    private void UpdateRotation()
    {
        Quaternion rotationDifference = CalculateRotationDifference();
        Vector3 angularVelocity = CalculateAngularVelocity(rotationDifference);
        rb.angularVelocity = angularVelocity;
    }

    private Quaternion CalculateRotationDifference()
    {
        return target.rotation * Quaternion.Inverse(transform.rotation);
    }

    private Vector3 CalculateAngularVelocity(Quaternion rotationDifference)
    {
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);
        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
        return rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime;
    }
}

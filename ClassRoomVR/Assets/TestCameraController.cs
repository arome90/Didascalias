using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestCameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotateSpeed = 100f;
    public float zoomSpeed = 10f;

    void Update()
    {
        // Movimiento de la cámara
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = 0f;
        if (Input.GetKey(KeyCode.E)) moveY = moveSpeed * Time.deltaTime; // Subir
        if (Input.GetKey(KeyCode.Q)) moveY = -moveSpeed * Time.deltaTime; // Bajar
        float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, moveY, moveZ));

        // Rotación de la cámara
        if (Input.GetMouseButton(1)) // Botón derecho del mouse
        {
            float rotateX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
            float rotateY = -Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, rotateX, Space.World);
            transform.Rotate(Vector3.right, rotateY, Space.Self);
        }

        // Movimiento de la cámara con el botón medio del ratón
        if (Input.GetMouseButton(2)) // Botón medio del ratón
        {
            float panX = -Input.GetAxis("Mouse X") * moveSpeed * Time.deltaTime;
            float panY = -Input.GetAxis("Mouse Y") * moveSpeed * Time.deltaTime;

            transform.Translate(new Vector3(panX, panY, 0));
        }

        // Zoom de la cámara
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        transform.Translate(Vector3.forward * scroll);

    }
}
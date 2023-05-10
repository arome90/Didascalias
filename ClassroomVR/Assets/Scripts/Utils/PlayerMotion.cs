using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class PlayerMotion : MonoBehaviour
    {
        [SerializeField]
        private Transform lookRoot;

        [SerializeField]
        private float sensivity = 5f;
        [SerializeField]
        private Vector2 default_Look_Limits = new Vector2(-70f, 80f);
        private float xRot;
        // Movimiento
        private CharacterController character_Controller;
        [SerializeField]
        private float speed = 5f;
        private Vector3 velocity;



        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            character_Controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            Pause();
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                MovePlayer();
                LookAround();
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            // GameManager.Instance._sceneManager.setCollision(other.GetComponentInParent<Transform>().gameObject.name);
        }

        // Camara
        public void unlockCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Time.timeScale = 0;


        }

        void Pause()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    unlockCursor();
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1;

                }
            }
        }

        void LookAround()
        {
            Vector2 current_Mouse_Look = new Vector2(
                Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            xRot -= current_Mouse_Look.y * sensivity;
            xRot = Mathf.Clamp(xRot, default_Look_Limits.x, default_Look_Limits.y);
            transform.Rotate(0f, current_Mouse_Look.x * sensivity, 0f);
            lookRoot.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        }


        // Movimiento
        private void MovePlayer()
        {
            Vector3 moveVector = transform.TransformDirection(new Vector3(Input.GetAxis("Horizontal"), 0f,
                                         Input.GetAxis("Vertical")));
            character_Controller.Move(moveVector * speed * Time.deltaTime);

            if (!character_Controller.isGrounded)
            {
                velocity.y -= -9.81f * -2f * Time.deltaTime;
            }
            else velocity.y = -1f;

            character_Controller.Move(velocity * Time.deltaTime);
        }

    }
}
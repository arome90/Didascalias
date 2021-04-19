using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class PlayerMotion : MonoBehaviour
    {
        // Camara
        public float sensitivity = 1;

        [SerializeField]
        private Transform playerRoot, lookRoot;

        [SerializeField]
        private bool invert;

        [SerializeField]
        private bool can_Unlock = true;

        [SerializeField]
        private float sensivity = 5f;

        [SerializeField]
        private int smooth_Steps = 10;

        [SerializeField]
        private float smooth_Weight = 0.4f;

        [SerializeField]
        private float roll_Angle = 10f;

        [SerializeField]
        private float roll_Speed = 3f;

        [SerializeField]
        private Vector2 default_Look_Limits = new Vector2(-70f, 80f);

        private Vector2 look_Angles;

        private Vector2 current_Mouse_Look;

        private bool start = true;

        // Movimiento
        private CharacterController character_Controller;

        private Vector3 move_Direction;

        public float speed = 5f;
        private float gravity = 25f;

        public float jump_Force = 10f;
        private float vertical_Velocity;

        void Awake()
        {
            character_Controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                LookAround();
            }
            MoveThePlayer();
        }

        // Camara
        public void unlockCursor()
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        void LockAndUnlockCursor()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {       
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Time.timeScale = 0;
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
            if (start)
            {
                start = false;
                current_Mouse_Look = new Vector2(0f, 180f);
            }
            else
            {
                current_Mouse_Look = new Vector2(
                Input.GetAxis(Constants.MOUSE_Y), Input.GetAxis(Constants.MOUSE_X));
            }

            look_Angles.x += current_Mouse_Look.x * sensivity * (invert ? 1f : -1f);
            look_Angles.y += current_Mouse_Look.y * sensivity;

            look_Angles.x = Mathf.Clamp(look_Angles.x, default_Look_Limits.x, default_Look_Limits.y);

            lookRoot.localRotation = Quaternion.Euler(look_Angles.x, 0f, 0f);
            playerRoot.localRotation = Quaternion.Euler(0f, look_Angles.y, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            GameManager.Instance._sceneManager.setCollision(other.GetComponentInParent<Transform>().gameObject.name);
        }

        // Movimiento
        private void MoveThePlayer()
        {
            move_Direction = new Vector3(Input.GetAxis(Constants.HORIZONTAL_AXIS), 0f,
                                         Input.GetAxis(Constants.VERTICAL_AXIS));

            move_Direction = transform.TransformDirection(move_Direction);
            move_Direction *= speed * Time.deltaTime;

            ApplyGravity();

            character_Controller.Move(move_Direction);
        }

        private void ApplyGravity()
        {

            vertical_Velocity -= gravity * Time.deltaTime;

            PlayerJump();

            move_Direction.y = vertical_Velocity * Time.deltaTime;
        }

        private void PlayerJump()
        {
            if (character_Controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                vertical_Velocity = jump_Force;
            }
        }
    }
}
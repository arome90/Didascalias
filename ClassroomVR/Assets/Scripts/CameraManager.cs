using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public float sensitivity = 1;

    public OVRHeadsetEmulator headset;
    public OVRManager manager;
    public OVRCameraRig cameraRig;

    /*
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
    private Vector2 smooth_Move;

    private float current_Roll_Angle;

    private int last_Look_Frame;

    private bool start = true;
    */

    private void Start () {
        if (!OVRManager.isHmdPresent) {
            headset.enabled = false;
            manager.enabled = false;
            cameraRig.enabled = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
	}

	void Update () {
        /*
        LockAndUnlockCursor();
        if(Cursor.lockState == CursorLockMode.Locked) {
            LookAround();
        }
        */
	}

    void FixedUpdate()
    {
        float rotateHorizontal = Input.GetAxis("Mouse X");
        float rotateVertical = Input.GetAxis("Mouse Y");
        //transform.RotateAround(transform.position, -Vector3.up, rotateHorizontal * sensitivity); //use 
        transform.Rotate(-transform.up * rotateHorizontal * sensitivity); //instead if you dont want the camera to rotate around the player
        //transform.RotateAround(Vector3.zero, transform.right, rotateVertical * sensitivity); // again, use 
        transform.Rotate(transform.right * rotateVertical * sensitivity); //if you don't want the camera to rotate around the player
    }

    void LockAndUnlockCursor() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(Cursor.lockState == CursorLockMode.Locked) {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
            }
        }
    }

    /*
    void LookAround() {
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

    */
}















































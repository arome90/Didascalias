using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerIK : MonoBehaviour {
	public TouchpadIK leftArmTouchpadIK;
	//public Plane hittingPlane;
	public Transform ikTargetLeftArm;
	private Plane plane;
	// Use this for initialization
	void Start () {
		plane = new Plane (new Vector3 (0, 0, -1), new Vector3 (0, 0, -0.2f));
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 currentPos = leftArmTouchpadIK.GetCurrentPosition ();
		if (currentPos != Vector2.zero) {
			Ray ray = Camera.main.ScreenPointToRay(currentPos);
			float rayDistance;
			if (plane.Raycast(ray, out rayDistance))
				ikTargetLeftArm.position = ray.GetPoint(rayDistance);
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/** This class extends the DragBodyPartHandler to properly transform the look at
 * position especially for having natural movements in lateral views. */
public class LookPosHandler : DragBodyPartHandler {
	// Reference to the camera pose to know the relevant dimension for 
	// rotating the look up/down
	public CameraPose cameraPose;
	// Angular speed (grades per world units)
	private float angularSpeed = 120;
	// Minimum and maximum angles in grades
	private float minAngle=-60;
	private float maxAngle=60;
	// The initial hitPoint set when the pointer is down for the first 
	// time.
	private Vector3 initialHitPoint;
	// The angle for turning from right to left 
	// (current of the offset and permanent) 
	private float angleTurning;
	private float permanentAngleTurning;
	// The angle for going from down (negative) to up (positive) 
	// (current of the offset and permanent)
	private float angleUp;
	private float permanentAngleUp;
	// Is first the time the drag is used
	//public bool isFirstTime;
	// The initial look at direction from the Start of the obhe object
	//private Vector3 initialLookDir;

	// It saves the initial look at direction
	void Start(){
		base.Start ();
		//Vector3 lookDir = ikTarget.position - targetObject.position;
		//isFirstTime = true;
		//Debug.Log ("lookDir" + lookDir);
		//permanentAngleUp = -Mathf.Rad2Deg *Mathf.Atan(-lookDir.y/(-lookDir.z));
		//Debug.Log ("permanentAngleUp:"+permanentAngleUp);
	}

	// It saves the initial hitPoint
	public override void OnPointerDown(PointerEventData data){
		base.OnPointerDown (data);
		// On the first time used, it initiates the variables
		if (isFirstTime) {
			permanentAngleTurning=0;
			permanentAngleUp = 0;
			isFirstTime = false;
		}
		initialHitPoint = CalculateHitPoint ();
		//Debug.Log ("initialHitPoint " + initialHitPoint);
	}

  
// It transforms the hitting point in the proper look at position
	public override Vector3 TransformHitPoint(Vector3 hitPoint){
		// Calculate the angle of turning
		Vector3 offsetHit = hitPoint - initialHitPoint;
		// Calculate the angle of up/down, selecting the relevant dimension 
		// according to the camera view
		float dimensionUp, dimensionTurning;
		if (cameraPose.IsFrontView ()) {
			dimensionTurning = offsetHit.x;
			dimensionUp = offsetHit.y;

		} else {
			dimensionTurning = 0;
			dimensionUp = offsetHit.z + offsetHit.y;

		}
		angleTurning = permanentAngleTurning + (dimensionTurning* angularSpeed);
		angleTurning =	Mathf.Clamp(angleTurning, minAngle, maxAngle) ;
		angleUp = permanentAngleUp + dimensionUp * angularSpeed;
		angleUp = Mathf.Clamp(angleUp, minAngle,maxAngle);
		Quaternion quat = Quaternion.Euler(new Vector3(
			-angleUp, angleTurning, 0));
		// calculate the IK look position (i.e. ikTarget)
		Vector3 offsetLook = quat * new Vector3(0,0,1);
		//Vector3 offsetLook = quat * new Vector3(angleUp,angleTurning,1);
		Vector3 ikTargetPos = this.targetObject.transform.position + offsetLook.normalized;
		//DebugLook ("TransformHitPoint ");
		return ikTargetPos;
	}
	// When releasing the drag, change the current angleTurning and
	// angle up
	public override void ReleaseDrag(){
		base.ReleaseDrag ();
		//Vector3 offset = CalculateHitPoint() - initialHitPoint;
		permanentAngleTurning= angleTurning;
		permanentAngleUp= angleUp;
		//DebugLook ("Release Drag ");
	}



	public void DebugLook(string msg){
		Debug.Log(msg+": permanentAngleUp " + permanentAngleUp +", "+
			"permanentAngleTurning "+ permanentAngleTurning +", "+
			"angleUp "+angleUp +", "+
			"angleTurning "+angleTurning+", "+
			"initialHitPoint "+initialHitPoint+", "+
			"hitPoint "+CalculateHitPoint());
	}

}

////////// NOT USED

/*
  
// It transforms the hitting point in the proper look at position
public override Vector3 TransformHitPoint(Vector3 hitPoint){
	// calculate the quaternion of look direction = offsets
	Vector3 offsetHit = hitPoint - initialHitPoint;
	angleTurning = offsetHit.x * angularSpeed;
	angleTurning =	Mathf.Clamp(angleTurning, minAngle, maxAngle) ;
	angleUp = Mathf.Clamp(permanentAngleUp+ (offsetHit.y + offsetHit.z)*
		angularSpeed, minAngle,maxAngle);
	Quaternion quat = Quaternion.Euler(new Vector3(
		-angleUp, angleTurning, 0));
	// calculate the IK look position (i.e. ikTarget)
	Vector3 offsetLook = quat * new Vector3(0,0,1);
	//Vector3 offsetLook = quat * new Vector3(angleUp,angleTurning,1);
	Vector3 ikTargetPos = this.targetObject.position + offsetLook;
	DebugLook ("TransformHitPoint ");
	return ikTargetPos;
}
// When releasing the drag, change the current angleTurning and
// angle up
public override void ReleaseDrag(){
	base.ReleaseDrag ();
	//Vector3 offset = CalculateHitPoint() - initialHitPoint;
	//permanentAngleTurning= angleTurning;
	//permanentAngleTurning= angleUp;
	//DebugLook ("Release Drag ");
}
*/

/*
// It transforms the hitting point in the proper look at position
public override Vector3 TransformHitPoint(Vector3 hitPoint){
	// calculate the quaternion of look direction = offsets
	Vector3 offsetHit = hitPoint - initialHitPoint;
	angleTurning = offsetHit.x * angularSpeed;
	angleTurning = Mathf.Clamp (angleTurning, -45, 45);
	angleUp = (offsetHit.y + offsetHit.z)* angularSpeed;
	angleUp = Mathf.Clamp (angleTurning, -45, 45);
	Quaternion quat = Quaternion.Euler(new Vector3(-
		permanentAngleUp+angleUp, 
		permanentAngleTurning+angleTurning, 0));
	// calculate the IK look position (i.e. ikTarget)
	Vector3 offsetLook = quat * new Vector3(0,0,1);
	//Vector3 offsetLook = quat * new Vector3(angleUp,angleTurning,1);
	Vector3 ikTargetPos = this.targetObject.position + offsetLook;
	DebugLook ("TransformHitPoint ");
	return ikTargetPos;
}
// When releasing the drag, change the current angleTurning and
// angle up
public override void ReleaseDrag(){
	base.ReleaseDrag ();
	//Vector3 offset = CalculateHitPoint() - initialHitPoint;
	permanentAngleTurning+= angleTurning;
	permanentAngleTurning = Mathf.Clamp (permanentAngleTurning, minAngle, maxAngle);
	permanentAngleUp+= angleUp;
	permanentAngleUp = Mathf.Clamp (permanentAngleUp, minAngle, maxAngle);
	//DebugLook ("Release Drag ");
}

*/
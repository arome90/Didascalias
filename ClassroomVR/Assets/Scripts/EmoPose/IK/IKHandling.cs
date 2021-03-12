using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* It applies Inverse Kinematics to an character. It should be placed
 * on a specific character (e.g. Ethan) */
public class IKHandling : MonoBehaviour {
	// Animator for the IK
	private Animator anim;
	float ikWeight=1.0f;
	// The hands and feet targets
	public Transform ikTargetLeftHand;
	public Transform ikTargetRightHand;
	public Transform ikTargetLeftFoot;
	public Transform ikTargetRightFoot;
	// The ik hints targegs (knees and elbows)
	public Transform ikTargetLeftKnee;
	public Transform ikTargetRightKnee;
	public Transform ikTargetLeftElbow;
	public Transform ikTargetRightElbow;
	// The look weight parameters
	public float lookIKweight;
	public float bodyWeight;
	public float headWeight;
	public float eyesWeight;
	public float clampWeight;
	// The look position target
	public Transform lookPos;
	// The draggable groups to enable or disable these
	public GameObject[] dragKnees;
	public GameObject[] dragElbows;
	// The Y dimension threshold below which the feet should be horizontal
	// to look like they are laying on the floor.
	private float yFloor= 0.25f;
	private Quaternion horizontalFootQuat = Quaternion.Euler(0,180,0);


	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		ActivateDraggableParts ();
	}

	// It is called right before updating the objects with Inverse Kinematics.
	// It calculates the position of the arms and legs by Inverse Kinematics
	void OnAnimatorIK(){
		// The look position parameters
		anim.SetLookAtWeight(lookIKweight,bodyWeight,headWeight,eyesWeight,
			clampWeight); 
		anim.SetLookAtPosition (lookPos.position);
		// The hands position
		anim.SetIKPositionWeight (AvatarIKGoal.LeftHand, ikWeight);
		anim.SetIKPositionWeight (AvatarIKGoal.RightHand, ikWeight);
		anim.SetIKPosition(AvatarIKGoal.LeftHand, ikTargetLeftHand.position);
		anim.SetIKPosition(AvatarIKGoal.RightHand, ikTargetRightHand.position);
		// The legs position
		anim.SetIKPositionWeight (AvatarIKGoal.LeftFoot, ikWeight);
		anim.SetIKPositionWeight (AvatarIKGoal.RightFoot, ikWeight);
		anim.SetIKPosition(AvatarIKGoal.LeftFoot, ikTargetLeftFoot.position);
		anim.SetIKPosition(AvatarIKGoal.RightFoot, ikTargetRightFoot.position);
		// Change the knees hint positions only if activated
		if(PosePrefs.IsDraggingKnees()){
			anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee,ikWeight);
			anim.SetIKHintPositionWeight(AvatarIKHint.RightKnee,ikWeight);
			anim.SetIKHintPosition(AvatarIKHint.LeftKnee,ikTargetLeftKnee.position);
			anim.SetIKHintPosition(AvatarIKHint.RightKnee,ikTargetRightKnee.position);
		}
		// Change the elbows hint positions only if activated
		if(PosePrefs.IsDraggingElbows()){
			anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow,ikWeight);
			anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow,ikWeight);
			anim.SetIKHintPosition(AvatarIKHint.LeftElbow,ikTargetLeftElbow.position);
			anim.SetIKHintPosition(AvatarIKHint.RightElbow,ikTargetRightElbow.position);
		}
		// Put the feet horizontal if near the floor
		if (ikTargetLeftFoot.position.y <= yFloor) {
			anim.SetIKRotationWeight (AvatarIKGoal.LeftFoot, ikWeight);
			anim.SetIKRotation (AvatarIKGoal.LeftFoot, horizontalFootQuat);
		}else
			anim.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 0);
		if (ikTargetRightFoot.position.y <= yFloor) {
			anim.SetIKRotationWeight (AvatarIKGoal.RightFoot, ikWeight);
			anim.SetIKRotation (AvatarIKGoal.RightFoot, horizontalFootQuat);
		}else
			anim.SetIKRotationWeight (AvatarIKGoal.RightFoot, 0);	
	}

	// Activate/deactivate the draggable parts regarding the user's preferences
	public void ActivateDraggableParts(){
		ActivateDraggableArray(dragKnees,PosePrefs.IsDraggingKnees());
		ActivateDraggableArray(dragElbows,PosePrefs.IsDraggingElbows());
	}
	// Activate/deactivate a list of draggable parts
	public void ActivateDraggableArray(GameObject[] array, bool active){
		foreach(GameObject gameObject in array){
			gameObject.SetActive (active);
		}
	}

}

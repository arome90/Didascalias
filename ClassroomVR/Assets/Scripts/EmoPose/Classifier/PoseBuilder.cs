using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ivan Magariño

/* This class creates poses from the user interface */
public class PoseBuilder : MonoBehaviour {
    // Ini positions
    private Vector3 iniLeftHand;
    private Vector3 iniRightHand;
    private Vector3 iniLeftFoot;
    private Vector3 iniRightFoot;
    private Vector3 iniHead;
    private Vector3 iniLookPos;

	// Transforms of the different relevant body parts
	public Transform leftHand;
	public Transform rightHand;
	public Transform leftFoot;
	public Transform rightFoot;
	public Transform head;
	// The position where the the character is looking at (an IK target)
	public Transform lookPos;
	// The reference ot the Opening hands handler
	public OpeningHandsHandler openingHandsHandler;

    void Awake() {
        iniLeftHand = leftHand.position;
        iniRightHand = rightHand.position;
        iniLeftFoot = leftFoot.position;
        iniRightFoot = rightFoot.position;
        iniHead = head.position;
        iniLookPos = lookPos.position;
    }

	// It creates a pose from the character
	public Pose CreatePoseFromCharacter(){
		Pose pose = new Pose ();
		pose.leftHandPos = leftHand.position - iniLeftHand;
		pose.rightHandPos = rightHand.position - iniRightHand;
		pose.leftFootPos = leftFoot.position - iniLeftFoot;
		pose.rightFootPos = rightFoot.position - iniRightFoot;
		pose.headPos = head.position - iniHead;
		Vector3 lookDirection = (lookPos.position - iniLookPos) - head.position;
		pose.headLookDirection = lookDirection.normalized;
		pose.openingHandsNormalized = openingHandsHandler.GetOpeningHandsNormalized ();
		return pose;
	}

    public Pose CreatePoseFromCharacterWithoutMove(Vector3 genPos)
    {
        Pose pose = new Pose();
        pose.leftHandPos = leftHand.position - genPos;
        pose.rightHandPos = rightHand.position - genPos;
        pose.leftFootPos = leftFoot.position - genPos;
        pose.rightFootPos = rightFoot.position - genPos;
        pose.headPos = head.position - genPos;
        Vector3 lookDirection = lookPos.position - head.position - genPos;
        pose.headLookDirection = lookDirection.normalized;
        pose.openingHandsNormalized = openingHandsHandler.GetOpeningHandsNormalized();
        return pose;
    }
}

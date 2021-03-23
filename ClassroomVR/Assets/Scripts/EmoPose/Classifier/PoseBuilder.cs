using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* This class creates poses from the user interface */
public class PoseBuilder : MonoBehaviour {
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

	// It creates a pose from the character
	public Pose CreatePoseFromCharacter(){
		Pose pose = new Pose ();
		pose.leftHandPos = leftHand.position;
		pose.rightHandPos = rightHand.position;
		pose.leftFootPos = leftFoot.position;
		pose.rightFootPos = rightFoot.position;
		pose.headPos = head.position;
		Vector3 lookDirection = lookPos.position - head.position;
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

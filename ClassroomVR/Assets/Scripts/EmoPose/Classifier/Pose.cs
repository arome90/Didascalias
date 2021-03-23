using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* It represents the basic information of a pose, so that it can be compared
 * to others and be classified. Basically it contains the positions of hands,
 * feet and head. It also has the rotation of the head. */
public class Pose{
	// Positions of the hands, feet and head
	public Vector3 leftHandPos;
	public Vector3 rightHandPos;
	public Vector3 leftFootPos;
	public Vector3 rightFootPos;
	public Vector3 headPos; 
	// The rotation of the head is stored as the normalized vector from the 
	// head position to point that the character is looking at.
	public Vector3 headLookDirection;
	// The opening of both hands in grades from the pose. 
	// It starts with the initial normalzed opening of hands
	// 1.0 represents max angle (i.e hands completely close) while 0.0 represents open
	public float[] openingHandsNormalized = {0.333f,0.333f}; 
	// The maximum distance for body parts for normalizing the global distance
	public float handsMaxDist=1.3f;
	public float feetMaxDist=1.5f;
	public float headPosMaxDist=0.4f;
	public float headLookDirMaxDist=2f;

	// The weights for the comparison
	public float handsWeight=2;
	public float feetWeight=1;
	public float headPosWeight=0.01f;
	public float headLookDirWeight=3f;
	public float openingHandsWeight=1.5f;

	// Default constructor. A pose can also be created from the PoseBuilder.
	public Pose(){
		leftHandPos = Vector3.zero;
		rightHandPos = Vector3.zero;
		leftFootPos = Vector3.zero;
		rightFootPos = Vector3.zero;
		headPos = Vector3.zero;
		headLookDirection = Vector3.zero;
	}
	// Returns a string that describes the pose
	public override string ToString(){
		return "leftHandPos " + leftHandPos + "; rightHandPos " + rightHandPos +
			"; leftFootPos " + leftFootPos + "; rightFootPos " + rightFootPos +
			"; headPos " + headPos+"; headLookDirection "+headLookDirection+
			"; openingHandsNormalized("+openingHandsNormalized[(int)Hand.Left]+","+
			+openingHandsNormalized[(int)Hand.Right]+"),";
	}
	// It returns the symmetric posture
	public Pose SymmetricPose(){
		Pose pose = new Pose ();
		pose.leftHandPos = SymmetricVector (rightHandPos);
		pose.rightHandPos = SymmetricVector (leftHandPos);
		pose.leftFootPos = SymmetricVector (rightFootPos);
		pose.rightFootPos = SymmetricVector (leftFootPos);
		pose.headPos = SymmetricVector (headPos);
		pose.headLookDirection = SymmetricVector (headLookDirection);
		pose.openingHandsNormalized[(int)Hand.Right]=openingHandsNormalized [(int)Hand.Left];
		pose.openingHandsNormalized [(int)Hand.Left]=openingHandsNormalized [(int)Hand.Right];
		return pose;
	}
	// Calculates the symmetric vector to the YZ plane. Basically, it changes 
	// the sign of the x value 
	private Vector3 SymmetricVector(Vector3 vector){
		return new Vector3 (-vector.x, vector.y, vector.z);
	}
	// It measures the difference (i.e. distance) from another pose. It measures
	// a weighted distance. It considers the current pose and the symmetric one.
	public float DistanceWithSymmetry(Pose pose){
		float dist = this.Distance (pose);
		float distSymmetric = this.Distance (pose.SymmetricPose ());
		return Mathf.Min (dist, distSymmetric);
	}

	// It considers the distance of the current pose to another one, but without
	// considering the symmetric pose. It returns a a normalized percentage
	public float Distance(Pose pose)
	{
        //Debug.Log("Cargando: " + pose.ToString());
        //Debug.Log("La cogida: " + ToString());
		float distHands = ((pose.leftHandPos - leftHandPos).magnitude +
		                  (pose.rightHandPos - rightHandPos).magnitude) / (2 * handsMaxDist);
		float distFeet = ((pose.leftFootPos - leftFootPos).magnitude +
			(pose.rightFootPos - rightFootPos).magnitude)/ (2 * feetMaxDist);
		float distHeadPos = (pose.headPos - headPos).magnitude/headPosMaxDist;
		float distHeadLookDir = (pose.headLookDirection - headLookDirection).magnitude
			/ headLookDirMaxDist;
		float distOpeningHands = 
			(Mathf.Abs(pose.openingHandsNormalized[(int)Hand.Left]-openingHandsNormalized [(int)Hand.Left])+
				Mathf.Abs(pose.openingHandsNormalized[(int)Hand.Right]-openingHandsNormalized [(int)Hand.Right]))/2;
		float dist = (handsWeight * distHands + feetWeight * distFeet +
			headPosWeight * distHeadPos + headLookDirWeight * distHeadLookDir+
						openingHandsWeight*distOpeningHands)
					/(handsWeight+feetWeight+headPosWeight+headLookDirWeight+openingHandsWeight);
		float distPercentage = dist * 100;
		return distPercentage;
	}
}



// ---------- NOT USED -------------

//public Vector3 headForward;
// The rotation of the head is stored as the forward vector of the head
// (i.e the direction where the character is looking at).
//public Vector3 headForward;

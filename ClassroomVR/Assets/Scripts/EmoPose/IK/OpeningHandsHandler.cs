using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* It handles the zoom gesture (two fingers enclosing or getting far from each other) 
 * for respectively closing and opening the fingers of a hand.
	It also handles the rotation of two fingers for representing the rotation of hands */
public class OpeningHandsHandler : MonoBehaviour {
	// Animator of the character (i.e. Ethan) for controlling the IK
	private Animator anim;
	// The group of bones of the right hand
	//public static HumanBodyBones[] bonesRightHand={HumanBodyBones.RightThumbDistal};
	// The right and left sides
	//public static int left=0;
	//public static int right=1;
	//public static int none = 2;
	// Init and end of the fingers of each hand. It is distinguishes between fingers and thumb as these have
	// different rotations. In has an array to consider differentl left and right sides
	public static HumanBodyBones[] bonestFingersInit= {HumanBodyBones.LeftIndexProximal, HumanBodyBones.RightIndexProximal};
	public static HumanBodyBones[] bonesFingersEnd= {HumanBodyBones.LeftLittleDistal,HumanBodyBones.RightLittleDistal};
	public static Vector3[] fingersRotation = { new Vector3(0f, 0f, 1f), new Vector3 (0f, 0f, 1f) };
	public static HumanBodyBones[] bonesThumbInit={HumanBodyBones.LeftThumbIntermediate,HumanBodyBones.RightThumbIntermediate};
	public static HumanBodyBones[] bonesThumbEnd= {HumanBodyBones.LeftThumbDistal, HumanBodyBones.RightThumbDistal};
	public static Vector3[] thumbRotation = { new Vector3(-1f, 0f, 0f), new Vector3 (1f, 0f, 0f) };
	// Opening of the each hand
	private float[] openingHand = {0f,0f};
	// The limit of the hands
	public static float maxOpeningHand = 60f;
	public static float minOpeningHand = -30f;
	public static float incrementOpeningHand=1;
	// Rotation of each hand, their limits and their speed
	private float[] rotationHand = {0, 0};
	public static float[] minRotationHand = {-0.3f, -0.3f}; //-180;
	public static float[] maxRotationHand = {1.0f, 1.0f}; //180;// //
	public static float incrementRotationHand = 0.01f; // 1;//
	// The from and to quaternions for hands
	private Quaternion[] fromQuaternion = {Quaternion.Euler(-88,75,-64),Quaternion.Euler(88,75,64)};
	private Quaternion[] toQuaternion = {Quaternion.Euler(-185,-92,-221),Quaternion.Euler(-12,-93,-35)}; // Quaternion.Euler(-46,-68,-51)


	// Determines the state about whether there are two touches
	private bool stateTwoTouches; 
	// The hand selected
	private Hand selectedHand;
	// The opening of the selected hand when two touces down
	private float[] initialOpeningHand= {0f,0f};
	// Initial distance between two touches
	private float initialDistance;
	// Speed for opening/closing hands (angular grades per width of screen)
	private float speedOpening = 180f;

	// The initial rotation of hands when two touces down
	private float[] initialRotationHand ={0,0};
	// Initial angle of the two touches
	private float initialAngle;
	// The speed of rotatin hands units interporlation/radians
	private float speedRotation = 1.0f;
	// Last difference of rotation
	private float lastIncTouchAngle;
	// Maximum increment in the difference of rotation, to avoid jumping of angles
	private float thresholdTouchAngle = Mathf.PI/2;

	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		selectedHand = Hand.None;
	}
	/* Initial distance between fingers */

	
	// Update is called once per frame. In touch devices, it manages the calls to
	// the emthods OnTwoTouchesDown, OnTwoTouchesUp, OnDragTwoTouches.
	// In non-touch devices, it uses the keys W,S, up arrow and down arrow for
	// opening/closing the hand
	void Update () {
		// For touch devices
		if (Input.touchSupported) {
			if (Input.touchCount >= 2) {
				if (stateTwoTouches) {
					OnDragTwoTouches ();
				} else {
					OnTwoTouchesDown ();
					stateTwoTouches = true;
				}
			} else if (stateTwoTouches) {
				OnTwoTouchesUp ();
				stateTwoTouches = false;
			} 			
		} else{ // For non-touch devices			
			if (Input.GetKey (KeyCode.UpArrow))
				ChangeOpeningHand (true);
			else if (Input.GetKey (KeyCode.DownArrow))
				ChangeOpeningHand (false);
			else if (Input.GetKey (KeyCode.RightArrow))
				ChangeRotationHand (true);
			else if (Input.GetKey (KeyCode.LeftArrow))
				ChangeRotationHand (false);
		}
	}

	// This method set the selected hand
	public void SetSelectedHand(Hand hand){
		selectedHand = hand;
	}

	// This method is calle the first time the two touches are down for the first time
	private void OnTwoTouchesDown(){
		// Check whether there is a hand selected
		if (selectedHand == Hand.None)
			return;
		// Set the initial hand values
		initialOpeningHand [(int)selectedHand] = openingHand [(int)selectedHand];
		initialDistance = GetDistanceBetweenTouches ();
		// Set the initial rotation hand values
		initialRotationHand [(int)selectedHand] = rotationHand [(int)selectedHand];
		initialAngle = GetRotationBetweenTouches ();
		lastIncTouchAngle = 0;
	}
	// This method is calle the first time the two touches are up after a while
	private void OnTwoTouchesUp(){
		
	}
	// This method is calle while the two touches are dragging (not the first time)
	private void OnDragTwoTouches(){
		UpdateOpeningSelectedHand ();
		UpdateRotationSelectedHand ();
	}

	// It updates the touches on opening/closign gestures
	private void UpdateOpeningSelectedHand(){
		// Check whether there is a hand selected
		if (selectedHand == Hand.None)
			return;
		// Calculate the increment of distance of touches
		float incTouchDist = GetDistanceBetweenTouches () - initialDistance;
		// // Notice that the hand is open when the angles are lower (so it is multiplied
		// // by menis one
		float diffOpening = - incTouchDist * speedOpening;
		float newOpening = initialOpeningHand[(int)selectedHand] + diffOpening;
		// It clamps with the limits
		openingHand [(int)selectedHand] = Mathf.Max(minOpeningHand, 
										Mathf.Min(maxOpeningHand,newOpening));
	}

	// It updates the rotation gestures of the hand
	private void UpdateRotationSelectedHand(){
		// Check whether there is a hand selected
		if (selectedHand == Hand.None)
			return;
		// Calculate the increment of angle selecting the shortest path of the circuference
		float currentAngle = GetRotationBetweenTouches ();
		float auxInitialAngle = initialAngle;
		float incTouchAngle = currentAngle - initialAngle;
		if (currentAngle < 0)
			currentAngle += 2 * Mathf.PI;
		if (auxInitialAngle < 0)
			auxInitialAngle += 2 * Mathf.PI;
		float alternativeInc = currentAngle - auxInitialAngle;
		if(Mathf.Abs(alternativeInc) < Mathf.Abs(incTouchAngle))
			incTouchAngle=alternativeInc;
		// Ignore the jumpings (i.e. over 90º for one frame)
		if (Mathf.Abs (lastIncTouchAngle - incTouchAngle) < thresholdTouchAngle) {
			// Treat the increment	
			float diffRotation = incTouchAngle * speedRotation;
			// It changes for the left hand
			if (selectedHand == Hand.Left)
				diffRotation = -diffRotation;
			float newRotation = initialRotationHand [(int)selectedHand] + diffRotation;
			// It clamps with the limits
			rotationHand [(int)selectedHand] = Mathf.Clamp (newRotation,
				minRotationHand [(int)selectedHand], maxRotationHand [(int)selectedHand]);
			lastIncTouchAngle = incTouchAngle;
		}
	}

	// It gets the distance between the two touces as a ratio of the pixeles of
	// the screen divided by the width of the screen.
	private float GetDistanceBetweenTouches(){
		Vector2 posTouch0 = Input.GetTouch (0).position;
		Vector2 posTouch1 = Input.GetTouch (1).position;
		return Vector2.Distance (posTouch0, posTouch1) / Screen.width;
	}
	// It gets the angle of the line between two fingers 
	private float GetRotationBetweenTouches(){
		Vector2 posTouch0 = Input.GetTouch (0).position;
		Vector2 posTouch1 = Input.GetTouch (1).position;
		Vector2 diff = posTouch1 - posTouch0;
		float angle = Mathf.Atan2 (diff.y, diff.x); 
		return angle;
	}

	// It returns a clone of the opening hands but normalized to the [0,1] interval
	public float[] GetOpeningHandsNormalized(){
		float[] openingHandsNormalized = new float[2];
		for (Hand hand = Hand.Left; hand <= Hand.Right; hand++) {
			openingHandsNormalized [(int)hand] = (openingHand [(int)hand] - minOpeningHand) 
					/ (maxOpeningHand - minOpeningHand);
		}
		return openingHandsNormalized;
	}



	// It is called right before updating the objects with Inverse Kinematics.
	// It calculates the position of the arms and legs by Inverse Kinematics
	void OnAnimatorIK(){
		
		// The hands grab and rotation
		UpdateIKHands();
		UpdateRotationIK ();
		// Tests for now
		//anim.SetIKRotationWeight(AvatarIKGoal.RightHand,ikWeight);
		//anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.Euler(0f,90f,0f));
		//for (HumanBodyBones b = HumanBodyBones.RightThumbProximal; b<=HumanBodyBones.RightLittleDistal;b++)
		//	anim.SetBoneLocalRotation(b, Quaternion.Euler(0f,0f,0f));

	}

	// It updates the IK of both hands
	private void UpdateIKHands(){
		for(Hand side = Hand.Left; side<=Hand.Right;side++)
			UpdateIKHand(side);
	}
	// It updates the IK of a hand of the side determined by the constants defined in this class (i.e. left or right)
	private void UpdateIKHand(Hand hand){
		int side = (int)hand;
		Quaternion rotation = Quaternion.Euler(openingHand[side]*fingersRotation[side].x, openingHand[side]*fingersRotation[side].y,
			openingHand[side]*fingersRotation[side].z);
		SetRotationGroupBones (bonestFingersInit[side], bonesFingersEnd[side], rotation);
		rotation = Quaternion.Euler(openingHand[side]*thumbRotation[side].x, openingHand[side]*thumbRotation[side].y,
			openingHand[side]*thumbRotation[side].z);
		//rotation = Quaternion.Euler(openingHand[side],0f,0f); 
		SetRotationGroupBones (bonesThumbInit[side], bonesThumbEnd[side], rotation);
	}
	// It opens the selected hand or close regarding the pararmeter isOpening.
	public void ChangeOpeningHand (bool isOpening){	
		// Calculate the rotation	
		float inc;
		if (selectedHand == Hand.None)
			return;
		if (isOpening)
			inc = incrementOpeningHand;
		else
			inc = -incrementOpeningHand;	
		openingHand[(int)selectedHand] += inc;
		openingHand[(int)selectedHand] = Mathf.Min (Mathf.Max (minOpeningHand, openingHand[(int)selectedHand]), maxOpeningHand);
	}
	// It rotates the selected hand towards right or left regarding the pararmeter isClockwise. 
	public void ChangeRotationHand (bool isClockwise){		
		float inc;
		if (selectedHand == Hand.None)
			return;
		if (isClockwise)
			inc = incrementRotationHand;
		else
			inc = -incrementRotationHand;
		rotationHand[(int)selectedHand] += inc;
		int index = (int)selectedHand;
		rotationHand[(int)selectedHand] = Mathf.Clamp(rotationHand[index], minRotationHand[index], maxRotationHand[index]); 
	}
	// Update the rotation of hands in the avatar
	public void UpdateRotationIK(){
		anim.SetBoneLocalRotation(HumanBodyBones.RightHand, Quaternion.SlerpUnclamped(fromQuaternion[(int)Hand.Right],
			toQuaternion[(int)Hand.Right],rotationHand[(int)Hand.Right]));
		anim.SetBoneLocalRotation(HumanBodyBones.LeftHand,Quaternion.SlerpUnclamped(fromQuaternion[(int)Hand.Left],
			toQuaternion[(int)Hand.Left],rotationHand[(int)Hand.Left]));
	}
	// It sets the local rotation of a group of bones
	private void SetRotationGroupBones(HumanBodyBones boneInit, HumanBodyBones boneEnd, Quaternion rotation){
		for (HumanBodyBones b = boneInit; b<=boneEnd;b++)
			anim.SetBoneLocalRotation(b, rotation);
	}
}

/////// NOT USED

//Debug.Log ("rotationHand " + rotationHand [0] + "," + rotationHand [1]);
//anim.SetIKRotationWeight (AvatarIKGoal.RightHand,1);
//anim.SetIKRotationWeight (AvatarIKGoal.LeftHand,1);
//anim.SetIKRotation(AvatarIKGoal.LeftHand,Quaternion.Euler(80,180,
//	rotationHand[(int)Hand.Left]));
//	Quaternion.Euler(
//	89-rotationHand[(int)Hand.Right],-0.8f*rotationHand[(int)Hand.Right],-10-rotationHand[(int)Hand.Right]));
//anim.SetIKRotation(AvatarIKGoal.RightHand,Quaternion.Euler(0,
//	rotationHand[(int)Hand.Right],0));

//anim.SetIKRotation(AvatarIKGoal.LeftHand,Quaternion.Euler(90,180,
//	rotationHand[(int)Hand.Left]));

//Vector3 eulerLeft = anim.GetIKRotation(AvatarIKGoal.LeftHand).eulerAngles;
// anim.SetIKRotation(AvatarIKGoal.LeftHand,Quaternion.Euler(eulerLeft.x,eulerLeft.y,
//	rotationHand[(int)Hand.Left]));
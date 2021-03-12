using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This class controll the camera to see the avatar from different views 
 * (e.g. front view, side view). It makes a smooth transition following 
 * circuference trajectory
 * */
public class CameraPose : MonoBehaviour {
	// Camerta that is being controlled
	public new Transform camera;
	// Initial point towards the camera is looking at
	public Vector3 initialTargetPos;
	// Radius of the circuference
	public float radius;
	// The angular speed, as the angle that is summed in each frame
	public float angularSpeed;
	// The speed towards the aimed target 
	public float targetSpeed;
	// The right and left hands of the avatar
	public Transform rightHand;
	public Transform leftHand;
	// A reference to the OpeningHandsHander to select/deselect a hand
	public OpeningHandsHandler openingHandsHandler;
	// A reference to the DragBodyParts to enable/disable these
	public GameObject dragBodyParts;
	// Reference to all the left drag body parts
	public GameObject[] leftBodyParts;
	// Reference to all the right drag body parts
	public GameObject[] rightBodyParts;
	// Reference to the body parts
	public GameObject[] centerBodyParts;
	// A reference to the tutorial of hands
	public TutorialHandler tutorialHands;
	// The point towards the camera is looking at
	private Vector3 target;
	// Indicates the target position towards the camera is moving
	private Vector3 aimedTarget;
	// Angle in which the camera is positioned
	private float angle;
	// It determines the angle to which the camera is rotating. Once this angle
	// is achived the angular direction should be zero again
	private float aimedAngle;
	// The angles of the left and right views regarding whether using natural rotation
	private float rightAngle;
	private float leftAngle;
	// Thre right and left hand whether using the natural rotation
	private Hand rightHandNatural;
	private Hand leftHandNatural;
	// The camera sizes for respectively close and far
	private float closeZoom = 0.2f;
	private float farZoom = 1.25f;
	// The spped of zooming in and out
	private float speedZoom = 0.1f;
	// Aimed size of the camera for performing the zooming
	private float aimedZoom;
	// Current size of the camera (for zooming)
	private float zoom;
	// Threshold for values near zero
	private float epsilon = 0.1f;

	// Use this for initialization
	void Start () {
		angle = 0f;
		aimedAngle = 0f;
		target.Set(initialTargetPos.x, initialTargetPos.y, initialTargetPos.z);
		aimedTarget.Set(initialTargetPos.x, initialTargetPos.y, initialTargetPos.z);
		zoom = farZoom;
		aimedZoom = farZoom;
		SetNaturalRotation (PosePrefs.IsNaturalRotation());

	}
	// It changes the angles, target position, and zoom if appropriate. It refreshes 
	// the camera if any change
	void Update () {
		if (TowardsAimedAngle () | TowardsAimedTarget () | TowardsAimedZoom ()) {
			RefreshCamera ();
			MoveDraggingImages ();
		}
	}
	// It determines whether the rotation is natural or the opposite
	public void SetNaturalRotation(bool isNaturalRotation){
		// Sets the angles
		float factor;
		if (isNaturalRotation)
			factor = 1;
		else
			factor = -1;
		rightAngle = factor * Mathf.PI/2;
		leftAngle = - factor * Mathf.PI/2;
		// Sets the hands
		if (isNaturalRotation) {
			rightHandNatural = Hand.Left;
			leftHandNatural = Hand.Right;
		} else {
			rightHandNatural = Hand.Right;
			leftHandNatural = Hand.Left;
		}
	}
	// It sets the hand target receiving the natural hand (which can also be none  
	public void SetTargetHand(Hand handNatural){
		if (handNatural == Hand.Right) {
			aimedTarget.Set (rightHand.position.x, rightHand.position.y, rightHand.position.z);
		}else if (handNatural == Hand.Left) {
			aimedTarget.Set (leftHand.position.x, leftHand.position.y, leftHand.position.z);
		}else{
			aimedTarget.Set(initialTargetPos.x, initialTargetPos.y, initialTargetPos.z);
		}
	}
	/* It moves the comera to the see the right side of the avatar with a smooth transition*/
	public void RightView(){
		aimedAngle = rightAngle;
		//aimedTarget.Set(initialTargetPos.x, initialTargetPos.y, initialTargetPos.z);
		aimedZoom = farZoom;
		SetTargetHand (Hand.None);
		openingHandsHandler.SetSelectedHand (Hand.None);
		EnableDragBodyParts (true);
		EnableDragList (leftBodyParts,PosePrefs.IsNaturalRotation());
		EnableDragList (rightBodyParts, !PosePrefs.IsNaturalRotation());
	}
	/* It moves the comera to the see the front side of the avatar with a smooth transition*/
	public void FrontView(){
		aimedAngle= 0f;
		SetTargetHand (Hand.None);
		aimedZoom = farZoom;
		openingHandsHandler.SetSelectedHand (Hand.None);
		EnableDragBodyParts (true);
		EnableDragList (leftBodyParts,true);
		EnableDragList (rightBodyParts, true);
	}

	/* It moves the comera to the see the left side of the avatar with a smooth transition*/
	public void LeftView(){
		aimedAngle = leftAngle;
		SetTargetHand (Hand.None);
		aimedZoom = farZoom;
		openingHandsHandler.SetSelectedHand (Hand.None);
		EnableDragBodyParts (true);
		EnableDragList (leftBodyParts, !PosePrefs.IsNaturalRotation());
		EnableDragList (rightBodyParts, PosePrefs.IsNaturalRotation());
	}
	/* It moves the comera to the see the right hand of the avatar with a smooth transition*/
	public void RightHandView(){
		aimedAngle = rightAngle;
		aimedZoom = closeZoom;
		SetTargetHand (rightHandNatural);
		openingHandsHandler.SetSelectedHand (rightHandNatural);
		EnableDragBodyParts (false);
		// It shows the tutorial when the first time
		tutorialHands.ShowIfFirstTime();
	}

	/* It moves the comera to the see the right hand of the avatar with a smooth transition*/
	public void LefttHandView(){
		aimedAngle = leftAngle;
		//aimedTarget.Set (leftHand.position.x, leftHand.position.y, leftHand.position.z);
		aimedZoom = closeZoom;
		SetTargetHand (leftHandNatural);
		openingHandsHandler.SetSelectedHand (leftHandNatural);
		EnableDragBodyParts (false);
		// It shows the tutorial when the first time
		tutorialHands.ShowIfFirstTime();
	}
	// Enable/disable the dragging body parts regarding the parameter
	public void EnableDragBodyParts(bool active){
		dragBodyParts.SetActive (active);
	}

	// Enable a list of drag body parts considering the specific cases of knees and elbow
	public void EnableDragList(GameObject[] dragList, bool active){
		foreach (GameObject drag in dragList) {
			if (drag.name.Contains ("Knee"))
				drag.SetActive (active && PosePrefs.IsDraggingKnees());
			else if(drag.name.Contains("Elbow"))
				drag.SetActive (active && PosePrefs.IsDraggingKnees());
			else
				drag.SetActive (active);
			// Move also the dragging images
			if (active) {
				DragBodyPartHandler dragHandler = drag.GetComponent<DragBodyPartHandler>();
				dragHandler.MoveDraggingImage ();
			}
		}
	}
	// Move all the dragging images of a list
	private void MoveDraggingImages(GameObject[] dragList){
		foreach (GameObject drag in dragList) {
			DragBodyPartHandler dragHandler = drag.GetComponent<DragBodyPartHandler>();
			dragHandler.MoveDraggingImage ();
		}
	}
	// Move all the dragging images of all the corresponding internal lists
	private void MoveDraggingImages(){
		MoveDraggingImages (leftBodyParts);
		MoveDraggingImages (rightBodyParts);
		MoveDraggingImages (centerBodyParts);
	}


	/** Rotate the angle towards the desired angle, and refresh the camera 
	only when necessary. It returns whether there has been any change. */
	private bool TowardsAimedAngle(){
		if (Mathf.Abs (angle - aimedAngle) >= angularSpeed) {
			if (angle < aimedAngle)
				angle += angularSpeed;
			else
				angle -= angularSpeed;
			return true;
		} else
			return false;	
	}
	/** It moves the target towards the aimed target. It returns whether 
	 * there has been any change. */
	private bool TowardsAimedTarget(){
		Vector3 direction = aimedTarget - target;
		if (direction.magnitude >= targetSpeed) {
			Vector3 movement = Vector3.ClampMagnitude(direction,targetSpeed);
			target = target + movement;
			return true;
		} else
			return false;
	}
	/** It moves the size fo the camera towards the aimed size. It returns whether 
	 * there has been any change. */
	private bool TowardsAimedZoom(){
		if (Mathf.Abs(aimedZoom-zoom)>speedZoom) {
			float sign = Mathf.Sign (aimedZoom - zoom);
			zoom += sign * speedZoom;
			return true;
		} else
			return false;
	}
	/* It refresh the position and rotation of the camera */
	private void RefreshCamera(){
		PlaceCamera ();
		OrientateCamera ();
		Camera.main.orthographicSize = zoom;
	}
	/* It sets the position of the camera according to the angle attribute */
	private void PlaceCamera(){
		float y = target.y;
		float z = -Mathf.Cos(angle) * radius;
		float x = Mathf.Sin(angle) * radius;
		camera.position = new Vector3 (x, y, z);
	}
	/* It orientates the camera towards the target position from the current position */
	private void OrientateCamera(){
		Vector3 forward = target - camera.position;
		camera.rotation = Quaternion.LookRotation (forward);
	}
	/* It determines whether it is in front view. If not, it is assumed to be front view */
	public bool IsFrontView(){
		return Mathf.Abs (aimedAngle) < epsilon; // == 0 but assuring it for floats
	}

}

//////// NOT USED //////////////////
// The angular direction in which it is rotating, (1 for positive direction,
// 0 for non rotations, -1 for negative rotations
//private int angularDirection;
//angularDirection=0;
//if (angularDirection != 0) {
//	angle += ((float)angularDirection) * angularSpeed;
//	RefreshCamera ();
//}

/* It simply goes to the right view instantly without any smooth transition 
	public void InstantRightView(){
		camera.position = new Vector3 (10f, 0.6f, 0f);
		camera.rotation = Quaternion.Euler (0f, -90f, 0f);
	}*/

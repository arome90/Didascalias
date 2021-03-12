using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/** This script can be associated with a inverted kinematics handler and the corresponding  
 * body part (e.g. a hand). It is attached to an image in the UI. 
 * The associated image moves arround with the body part. It needs that the associated 
 * image anchor mins are 0 and 0 to work properly. The anchor maxs determines the width and 
 * height of the displayerd area.
 * It also highlights the handler while dragging for increasing its usability as the
 * use has feedback. */
public class DragBodyPartHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler  {
	// Inverted Kinematics (IK) target, used to move the body part
	public Transform ikTarget;
	// The physical body part, used to move the current dragging image */
	public Transform targetObject;
	// To distinguish different hitting planes, it determines whether the drag image is 
	// represented with a foot
	public bool isFoot;
	// To distinguish different hitting planes, it determines whether the drag image is 
	// associated with a hand
	//public bool isHand;
	public bool isRightHand;
	public bool isLeftHand;
	// Reference to the camera to determine whether it is in front view
	// For now it is only necessary for the hands, which need to alter
	// their position
	public CameraPose cameraPoseMain;
	// Plane for the raycastting
	private Plane hittingPlane;
	// Current position 
	private Vector2 pointerPosition;
	// Rect transform of the dragging image
	private RectTransform rectTransform;
	// Offset established at the beginning 
	private Vector2 offsetScreen;
	// Reference ot the image that the script is attached
	private Image image;
	// The colors when the handle is respectively normal and highlighed
	private Color normalColor;
	private Color highlightedColor;
	private static float increaseAlpha=0.5f;
	// The proper times for managing the change of order when keeping pressed for a while
	// The time that the user has kept pressed still
	private float pressedTime;
	// The threshold of pressed time for taking the action of changing the order of dragging parts
	private float thresholdPressedTime=2.0f;
	// Track whether it is pressed by the user
	private bool isPressed;
	// Track whether it is ignored by the fact that it sent to background
	private bool isIgnored;
	// Path of the prefab within the Resources folder
	private string pathPrefabEdge = "Prefabs/DragEdge";
	// Reference to the prefab DragEdge
	private static GameObject prefabEdge=null;
	// Reference to the instantation of dragEdge
	private GameObject dragEdge;
	// The first time used?
	protected bool isFirstTime;
	// The maximun Z for hands when front view
	private float maxZHands = -0.15f; // 0.2f
	// The limits for the right and left hand
	private float maxXRightHand = 0.06f;
	private float minXLeftHand = -0.06f;
	// In case of being a hand it references to the corresponding elbow. In case of being
	// a foot, it points to the corresponing knee
	public DragBodyPartHandler joint;




	// Use this for initialization
	public void Start () {
		// Initialize the pointer and raycast issues
		pointerPosition = Vector2.zero;
		// Initialize the dragging issues
		rectTransform = GetComponent < RectTransform> ();
		offsetScreen = new Vector2(Screen.width*rectTransform.anchorMax.x/2,
			Screen.height*rectTransform.anchorMax.y/2);
		// Get the image and initialize the associated colors
		image = GetComponent<Image>();
		normalColor = new Color (image.color.r, image.color.g, image.color.b, image.color.a);
		float newAlpha = Mathf.Min (image.color.a + increaseAlpha, 1); 
		highlightedColor = new Color (image.color.r, image.color.g, image.color.b, newAlpha);
		// Create the edge of the drag part
		CreateEdge();
		// Other
		isPressed=false;
		isFirstTime = true;

	}

	// Move the dragging to the body part when updated. It realeses the draga part and sent it
	// to the back
	void Update(){
		if (isFirstTime) {
			MoveDraggingImage ();
			isFirstTime = false;
		}
		if (isPressed) {
			pressedTime += Time.deltaTime;
			// If pressed for a while, drop it and send it to the backgroun
			// to leave the oportunity for other drags
			if (pressedTime>=thresholdPressedTime){
				ReleaseDrag ();
				isIgnored = true;

			}
		}
	}
	// For considering physics, avoiding shaking from colliders
	void LateUpdate(){
		if(isPressed)
			MoveDraggingImage ();
	}

	/** When selecting a handler, highlight it */
	public virtual void OnPointerDown(PointerEventData data){
		isPressed = true;
		isIgnored = false;
		pointerPosition = data.position;
		CalculateHittingPlane();
		HighlightHandler ();
		pressedTime = 0;
	}
	/** While dragging, move the IK target */
	public void OnDrag(PointerEventData data){
		// check whether it is ignored
		if (isIgnored)
			return;
		// Manage the movement
		pointerPosition = data.position;
		MoveIKTarget ();
		//MoveDraggingImage ();
		// When it moves reset the still pressed time 
		pressedTime = 0;
	}
	// When stopping the dragging, return the hander to its normal state
	public void OnPointerUp(PointerEventData data){
		// check whether it is ignored
		if (isIgnored)
			return;
		MoveDraggingImage ();
		// Manage the realease
		ReleaseDrag ();
	}
	/** It moves the IK target to the pointer position. The hitting plane
	is calculated as the perpendicula plane to the camerat that goes throught
	the IK target position */
	private void MoveIKTarget (){
		// Calculate the hitting plane perpendicular to the camera
		CalculateHittingPlane();
		// Move the IK target
		Vector3 hitPoint = CalculateHitPoint();
		ikTarget.position = TransformHitPoint (hitPoint);
	}
	/** Calculate the hit point */
	public Vector3 CalculateHitPoint(){
		Vector2 currentPos = GetPointerPosition ();
		if (currentPos != Vector2.zero) {
			Ray ray = Camera.main.ScreenPointToRay(currentPos);
			float rayDistance;
			if (hittingPlane.Raycast (ray, out rayDistance)) {
				Vector3 hitPoint = ray.GetPoint (rayDistance);
				hitPoint = ConsiderHitPointExceptions (hitPoint);
				return hitPoint;
			}
		}
		return Vector3.zero;
	}
	/** It considers the hit point exceptions such as hands when front view.
	It modifies the hitpoint in some cases */
	public Vector3 ConsiderHitPointExceptions(Vector3 hitPointIn){
		if ((isRightHand || isLeftHand) && cameraPoseMain.IsFrontView()) {
			Vector3 hitPoint = new Vector3 (hitPointIn.x, hitPointIn.y, hitPointIn.z);
			// Make sure the hands are away from the body
			hitPoint.z = Mathf.Min (hitPoint.z, maxZHands);
			// Guarantee some limits when going to the opposite side of the hand
			if (isRightHand) {
				hitPoint.x = Mathf.Min (hitPoint.x, maxXRightHand);
			} else if (isLeftHand) {
				hitPoint.x = Mathf.Max (hitPoint.x, minXLeftHand);
			}
			return hitPoint;
		}else
			return hitPointIn;
	}
	/** Transform the hitting point into another point if necessary. It will be 
	 * overwritten su subclasses when necessary */
	public virtual Vector3 TransformHitPoint(Vector3 hitPoint){
		return hitPoint;
	}
	/* It releases the drag by turning to normal hander, and turning isPressed off.
	It sends back to the background*/
	public virtual void ReleaseDrag(){
		rectTransform.SetAsFirstSibling ();
		NormalHandler ();
		isPressed = false;
	}
	/** It calculates the hitting plane as perpendicular to the camera and going 
	 * throught the target ik position. In case of hands, it makes sure
	that the hand is in front (not behind the body) */
	private void CalculateHittingPlane(){
		// Calculate the normal of the plane
		Vector3 inNormal = Camera.main.transform.position - targetObject.position;
		inNormal.Normalize();
		// Alter the position for hands in front view
		//Vector3 targetPos = new Vector3 (targetObject.position.x, targetObject.position.y,
		//	                    targetObject.position.z);
		//if (isHand && cameraPoseMain.IsFrontView()) {
		//	targetPos.z = Mathf.Min (targetPos.z, maxZHands);
		//}
		// Creates a plane goint through the IK target position 
		hittingPlane= new Plane(inNormal,targetObject.position);
		//hittingPlane= new Plane(inNormal,targetPos);
	}
	/** It moves the dragging image to the body part location considering the ray cast */
	public void MoveDraggingImage(){
		Vector2 targetScreenPosition = Camera.main.WorldToScreenPoint(targetObject.position);
		//Debug.Log ("TargetScreenPosition =" + targetScreenPosition);
		rectTransform.anchoredPosition = targetScreenPosition-offsetScreen;
		// It call to the corresponding joint
		if (joint != null && joint.gameObject.activeInHierarchy)
			joint.MoveDraggingImage ();
	}
	/* It highlights the handler so the user knows when they are dragging */
	public void HighlightHandler(){
		//image.color = highlightedColor;
		dragEdge.SetActive(true);
	}
	/* It determines when the handler returns to normal after being highlighted */
	public void NormalHandler(){
		//image.color = normalColor;
		dragEdge.SetActive(false);
	}

	/** Getter of the current pointer position */
	public Vector2 GetPointerPosition(){
		return pointerPosition;
	}

	/** It creates the edge of the drag area by instantiating the prefab */
	public void CreateEdge(){
		// Get the prefab only for the first object
		if (prefabEdge == null)
			prefabEdge = (GameObject) Resources.Load(pathPrefabEdge);
		// Instantiate the prefab
		if (prefabEdge != null) {
			dragEdge = (GameObject) Instantiate (prefabEdge);
			// Adjust the location
			dragEdge.transform.parent = this.transform;
			RectTransform edgeRectTransform = dragEdge.GetComponent<RectTransform> ();
			edgeRectTransform.offsetMin= new Vector2(0,0);
			edgeRectTransform.offsetMax= new Vector2(1,1);
			// Put the color to all the inside images
			Image[] edgeImages = dragEdge.GetComponentsInChildren<Image>();
			foreach (Image image in edgeImages) {
				image.color = highlightedColor;
			}
			// Deactivate the dragEdge
			dragEdge.SetActive(false);
		} else {
			Debug.Log ("Error instantiating prefab");
		}
	}
}

/* NOT USED 

//if(isFoot)
		//	hittingPlane = new Plane (new Vector3 (0, -1, -1), new Vector3 (0, 0, 0.1f));
		//else // is a hand
		//	hittingPlane = new Plane (new Vector3 (0, 0, -1), new Vector3 (0, 0, -0.2f));


/** It calculates the hitting plane as perpendicular to the camera and going 
* throught the target ik position 
private void CalculateHittingPlane(){
	// Calculate the normal of the plane
	Vector3 inNormal = Camera.main.transform.position - ikTarget.position;
	inNormal.Normalize();
	// Creates a plane goint through the IK target position 
	hittingPlane= new Plane(inNormal,ikTarget.position);
}
//Vector2 targetScreen = Camera.main.WorldToScreenPoint (targetObject.position);
		//offsetScreen = new Vector2(50,50);

// Threshold for obtaining vertical and horizontal, equivalent a cosine or a sine.
//private static float CosSinThreshold = 0.5f;
//public Canvas canvas;
//offsetScreen = targetScreen - rectTransform.anchoredPosition;
//offsetScreen = Vector2.zero;

var centreX = (rectTransform.anchorMin.x+rectTransform.anchoredPosition.x) * canvas.scaleFactor;
		var centreY = rectTransform.anchoredPosition.y * canvas.scaleFactor;
		offsetScreen = new Vector2(centreX,centreY)-rectTransform.anchoredPosition;
*/


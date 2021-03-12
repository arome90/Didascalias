using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** This class allows tha handle a Drag Image associated with an object (Transform). it 
 * will move with the same offset from the object. It can be used to contro body parts
 * such as hands and feet */
public class DragImageHandler : MonoBehaviour {
	// The object used to move the current drag image */
	public Transform targetObject;
	// Offset established at the beginning 
	private Vector2 offsetScreen;
	// Rect transform of the dragging image
	private RectTransform rectTransform;
	// Distance between anchors
	private Vector2 offsetAnchors;
	// Use this for initialization
	void Start () {
		rectTransform = GetComponent < RectTransform> ();
		Vector2 originScreen = Camera.main.WorldToScreenPoint (targetObject.position);
		offsetScreen = originScreen - rectTransform.anchoredPosition;
		//offsetScreen = originScreen - rectTransform.anchorMin;
		//offsetAnchors = rectTransform.anchorMax - rectTransform.anchorMin;
	}
	
	// Move the dragging imagge
	void Update () {
		Vector2 targetScreenPosition = Camera.main.WorldToScreenPoint(targetObject.position);
		rectTransform.anchoredPosition = targetScreenPosition - offsetScreen;
		//rectTransform.anchorMax = rectTransform.anchorMin + offsetAnchors;
		//Debug.Log ("Target Screen Postion = "+targetScreenPosition.ToString ()+
		//	"; Screen Position = "+rectTransform.anchorMin);
		
	}
}

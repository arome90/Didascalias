using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TouchpadIK : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
	private Vector2 currentPosition;
	// Threshold for obtaining vertical and horizontal, equivalent a cosine or a sine.
	//private static float CosSinThreshold = 0.5f;

	void Start(){
		currentPosition = Vector2.zero;
	}

	public void OnPointerDown(PointerEventData data){
		currentPosition = data.position;
	}
	public void OnDrag(PointerEventData data){
		currentPosition = data.position;
	}
	public void OnPointerUp(PointerEventData data){
		currentPosition = data.position;
	}
	public Vector2 GetCurrentPosition(){
		return currentPosition;
	}
}

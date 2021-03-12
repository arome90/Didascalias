using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


/** This script implements the overlapped tutorials that appears
 * above the app, and disappears when the user closes it.
 * It only appears only for the first time. In some cases the
 * user might request see the tutorial. */
public class TutorialHandler : MonoBehaviour, IPointerDownHandler {
	// Determines whether to start being visible. This is only for
	// the first time. If false, the program should explicitly call
	// the method "ShowIfFirstTime".
	public bool startVisible;
	// Is the first time the screen is shown. If so, the the
	// tutorial will be shown when requested
	private bool isFirstTime;

	// It initializes the attributes, and hides it until requested
	void Start () {
		 // This will change when distributed
				// for each user. For the experiments, it 
				// is useful to start fresh when running the app
				// as each user will be different.
		Reset();
		//isFirstTime = true;
		//if (!isFirstTime || !startVisible) 
		//	gameObject.SetActive (false);
	}
	// A method for closing the tutorial. When closing, it is registered
	// that the next time will not be the first time.
	public void Close(){
		gameObject.SetActive (false);
		isFirstTime = false;
	}

	// It shows the tutorial if first time. This should be explicitly
	// called by another script when appropriate
	public void ShowIfFirstTime(){
		if (isFirstTime) {
			gameObject.SetActive (true);
		}		
	}

	// It resets the tutorial for the first time
	public void Reset(){
		isFirstTime = true;
		if (!isFirstTime || !startVisible) {
			gameObject.SetActive (false);
		}
	}


	// When tapping it closes the tutorial
	public void OnPointerDown(PointerEventData data){
		Close ();
	}


	// Update is called once per frame
	void Update () {
		
	}
}

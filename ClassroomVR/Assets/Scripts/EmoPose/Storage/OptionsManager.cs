using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* This clase manages the options menu, and updates the pose prefs 
 * when appropriate */
public class OptionsManager : MonoBehaviour {
	// References to the toggle buttons
	public Toggle draggingKneesToggle;	
	public Toggle draggingElbowsToggle;
	public Toggle naturalRotationToggle;	
	// References to the groups of dragggers 
	//public GameObject dragKnees;
	//public GameObject dragElbows;
	// Reference to the scrip of the camera for poses
	public CameraPose cameraPose;
	// Reference to the IK Handling for updating the draggable parts
	public IKHandling ikHandling;
	// Use this for initialization
	void Start () {
		draggingKneesToggle.isOn = PosePrefs.IsDraggingKnees ();
		draggingElbowsToggle.isOn = PosePrefs.IsDraggingElbows ();
		naturalRotationToggle.isOn = PosePrefs.IsNaturalRotation ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	// Update whether to drag knees
	public void UpdateDraggingKnees(){
		PosePrefs.SetDraggingKnees (draggingKneesToggle.isOn);
		//dragKnees.SetActive (draggingKneesToggle.isOn);
		ikHandling.ActivateDraggableParts ();
	}
	// Update whether to drag elbows
	public void UpdateDraggingElbows(){
		PosePrefs.SetDraggingElbows (draggingElbowsToggle.isOn);
		//dragElbows.SetActive (draggingElbowsToggle.isOn);
		ikHandling.ActivateDraggableParts ();
	}
	// Update wheter to use natural rotation
	public void UpdateNaturalRotation(){
		PosePrefs.SetNaturalRotation (naturalRotationToggle.isOn);
		cameraPose.SetNaturalRotation(naturalRotationToggle.isOn);
	}
}

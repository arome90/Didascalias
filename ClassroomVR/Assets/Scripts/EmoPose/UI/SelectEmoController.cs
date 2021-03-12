using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SelectEmoController : MonoBehaviour {
	// Reference to the panel with the toggle buttons
	public GameObject panelToggles;
	// Reference to the game controller classifer
	public GameControllerClassifier gameControllerClassifier;
	// A reference to the text distance in the selected emo UI
	public Text distSelectedEmoText;
	// In the code any toggle button of the SelectedEmoCanvas (AngryToggle Group)
	public ToggleGroup selectedEmoToggleGroup; 


	// Use this for initialization
	void Start () {
		// Add listeners to all the toggle buttons
		Component[] toggles = panelToggles.GetComponentsInChildren<Toggle>();
		foreach(Toggle toggle in toggles)
			toggle.onValueChanged.AddListener (ShowDistanceLastPoseToEmo);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	// Shows the distance of the selected emo whenever the user checks an emotion
	// The parameter is the value actually changed, which is necessary for 
	// being used as a listener
	public void ShowDistanceLastPoseToEmo (bool value){
		// Get the selected emo
		Emotion emotion = GetSelectedEmo();
		// Show the selected emo
		float dist = gameControllerClassifier.GetDistanceLastPoseToEmo(emotion);
		distSelectedEmoText.text = "Distance " + dist.ToString ("0.0") + "%";
	}
	// It returns the selected emotion in the toggles buttons
	public Emotion GetSelectedEmo(){
		IEnumerator<Toggle> enumerator = selectedEmoToggleGroup.ActiveToggles ().GetEnumerator ();
		enumerator.MoveNext ();
		Toggle selectedToggle = enumerator.Current; 
		Emotion selectedEmo;
		if (selectedToggle == null)
			selectedEmo = Emotion.Neutral;
		else {
			string strEmo = selectedToggle.name;
			selectedEmo = (Emotion)Enum.Parse (typeof(Emotion), strEmo);
		}
		return selectedEmo;
	}


}

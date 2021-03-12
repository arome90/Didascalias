using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/* It controls the game flow of the classifier */
public class GameControllerClassifier : MonoBehaviour {
	// For builidng poses from the UI
	public PoseBuilder poseBuilder;
	// A reference to the text classifier result in the UI
	public Text textClassifierResult;
	// A reference to the text distance in the result UI
	public Text distResultText;
	// A pose base for classifying the pose of the UI
	private PoseBase poseBase; 
	// Determines whether the application is being used for manually training
	private bool manuallyTraining = true;

	// Initialize the element
	void Start(){
		poseBase = new PoseBase ();
		poseBase.AddDefaultCases ();
		// Testing
		//Test();
	}
		
	/* This method allows the developer to define the poses of the initial
	 * pose base */
	public void ShowPoseFromCharacter(){
		Pose pose = poseBuilder.CreatePoseFromCharacter ();
	}
	/** It classify the pose from the UI. It shows the emotion and the distance **/
	public void ClassifyPoseFromCharacter(){
		Pose pose = poseBuilder.CreatePoseFromCharacter ();
		Emotion emo = poseBase.Classify (pose);
		textClassifierResult.text = emo.ToString ();
		distResultText.text = "Distance "+poseBase.lastDistance.ToString("0.0")+"%";
		// Only for configuring the default pose cases:
		if(manuallyTraining)
			SavePoseCaseFromCharacter();
	
	}
	/** Method for conforming the default pose cases */
	public void SavePoseCaseFromCharacter(){
		Pose pose = poseBuilder.CreatePoseFromCharacter ();
		PoseWriter writer = new PoseWriter ();
		PoseCase poseCase = new PoseCase (pose, Emotion.None);
		writer.WritePoseCase (poseCase);
	}

	// This method is only used to perform tests (development and builidng process)
	public void Test(){
		string[] array = "aaa\n".Split ('\n');
		Debug.Log("Test: array.length ="+array.Length+"array last ='"+array[1]+"'");
	}
	// Get the distance of the last classification
	public float GetLastDistance(){
		return poseBase.lastDistance;
	}
	// Gets the last user pose
	public Pose GetLastUserPose(){
		return poseBase.lastUserPose;
	}
	// Gets the last emotion cla
	public Emotion GetLastOutputtedEmotion(){
		return poseBase.lastEmotion;
	}
	// It gets the distance of the last pose to a given emotion
	public float GetDistanceLastPoseToEmo(Emotion emotion){
		return poseBase.GetDistanceLastPoseToEmo(emotion);
	}

}


/* ------------- BAK ----------------
		// Only for debugging
		PoseWriter writer = new PoseWriter ();
		writer.WritePose (pose);
		string content = writer.ReadFromFile ("prueba.txt");
		Debug.Log ("Information readed from file: " + content);
//
		PoseWriter writer = new PoseWriter ();
		Pose pose = new Pose ();
		writer.WritePose (pose);
//
//Debug.Log ("Emotion classified: " + emo.ToString ());
		

*/

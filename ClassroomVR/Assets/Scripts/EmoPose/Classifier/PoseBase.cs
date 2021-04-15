using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/* This class represents a base of pose cases. It allows storing different
 * pose cases. Its main purpuse is to classify a new pose, by finding
 * the most similar one in the base case. Then, it returns its associated
 * emotion.
 */
public class PoseBase{
	// The list of the pose cases 
	private List<PoseCase> listCases;
	// A writer for persiting the data of pose cases
	private PoseWriter writer;
	// References to the distance of the last classification
	public float lastDistance;
	// Last User Pose
	public Pose lastUserPose;
	// Last emotion outputted
	public Emotion lastEmotion;

	// Constructor 
	public PoseBase(){
		listCases = new List<PoseCase>();
		writer = new PoseWriter ();
	}
	// Classify the given pose into an emotion by obtaining the nearest
	// neighbor (NN).
	public Emotion Classify (Pose pose){
		// Check whether there are any base case
		if (listCases.Count==0)
			return Emotion.None;
		// Obtains the most similar
		float minDist = Mathf.Infinity;
		PoseCase mostSimilarCase = new PoseCase(new Pose(), Emotion.None);
		foreach (PoseCase poseCase in listCases) {
			float dist = pose.DistanceWithSymmetry (poseCase.pose);
			if (dist < minDist) {
				mostSimilarCase = poseCase;
				minDist = dist;
			}
		}
		lastDistance = minDist;
		lastUserPose = pose;
		lastEmotion = mostSimilarCase.emotion;
		return mostSimilarCase.emotion;
	}
	// It gets the distance of the last pose to a given emotion
	public float GetDistanceLastPoseToEmo(Emotion emotion){
		float minDist = Mathf.Infinity;
		foreach (PoseCase poseCase in listCases) {
			if(poseCase.emotion.Equals(emotion)){
				float dist = lastUserPose.DistanceWithSymmetry (poseCase.pose);
				if (dist < minDist) {
					minDist = dist;
				}
			}
		}
		return minDist;
				
	}
	// It adds the default poses for all the emotions except "None",
	// which is not actally an emotion.
	public void AddDefaultCases(){
		foreach(Emotion emotion in Enum.GetValues(typeof(Emotion))){
			if (emotion != Emotion.None) {
				List<PoseCase> poseCases = writer.ReadPoseCases(emotion);
                //Debug.Log(emotion + "--------------------------------");
				foreach (PoseCase poseCase in poseCases) {
                    //Debug.Log(poseCase.pose.ToString());
					listCases.Add (poseCase);
				}
			}
		}
	}
}

/*
-------- BAK -------
//poseCase = writer.ReadPoseCase (Emotion.Sad);
		//listCases.Add (poseCase);

// Add very basic pose cases for testing 
	public void AddDefaultCases(){
		PoseCase poseCase = new PoseCase(new Pose(), Emotion.Happy);
		listCases.Add (poseCase);
		poseCase = new PoseCase(new Pose(), Emotion.Sad);
		listCases.Add (poseCase);
	}

*/

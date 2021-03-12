using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class contains a pose and the associated emotion */
public class PoseCase {
	public Pose pose;
	public Emotion emotion;
	// Constructor of the class
	public PoseCase(Pose pose, Emotion emotion){
		this.pose = pose;
		this.emotion = emotion;
	}
}

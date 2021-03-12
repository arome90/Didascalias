using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System;

/* It measures the time elapsed for setting a 3D posture */
public class PoseStopwatch : MonoBehaviour {
	// Generic stopwatch of C#
	private Stopwatch stopwatch;
	// Reference to the text for showing the time elapsed setting the posture
	// in the classifier output screen
	public Text timeText;
	// The last time elapsed in seconds
	public int lastTimeElapsedSeconds;
	// Use this for initialization
	void Start () {
		stopwatch = new Stopwatch();
	}
	// It starts the stopwatch for the posture manager
	public void StartPostureManager(){
		stopwatch.Reset ();
		stopwatch.Start ();
	}

	// It stops the stopwatch for the posture manager and
	// shows the result in the classifier result screen
	public void StopPostureManager(){
		stopwatch.Stop ();
		TimeSpan timeSpan = stopwatch.Elapsed;
		// Show time
		string result = "Time : ";
		if(timeSpan.Hours>0)
			result+=timeSpan.Hours+"h ";
		if(timeSpan.Minutes>0)
			result+=timeSpan.Minutes+"m ";
		result+=timeSpan.Seconds+"s ";
		timeText.text = result;
		// Calculate the time in seconds
		lastTimeElapsedSeconds = timeSpan.Hours * 3600 + timeSpan.Minutes * 60 +
				timeSpan.Seconds;
	}
}

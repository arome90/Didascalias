using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/** It writes and reads Pose and PoseBase persistently in files */
public class PoseWriter{
	public static string pathPosesInUnityEditor = "LearnedPosesTxt";
	public static string defaultPathPoseAssets = "DefaultPosesTxt/";

	// Constructor of the class
	public PoseWriter(){
	}

	// It writes a Pose Case appending it to the ones about the same emotion.
	// It appends it to the existing file. 
	public void WritePoseCase(PoseCase poseCase){
		Pose pose = poseCase.pose;
		string content = PoseToString (pose);
		string filename = poseCase.emotion+".txt";
		WriteIntoFile (filename, content);
	}
	// It reads the existing list of Pose Cases for a given emotion. If the parameter
	// 'isDefault' is true, then load from the text assets. Otherwise,
	// it reads from the learned file.
	public List<PoseCase> ReadPoseCases(Emotion emotion, bool isDefault = true){
		string filename; 
		string content;
		if (isDefault) {
			filename = "" + emotion; // Notice that it does not have extension
			string path = defaultPathPoseAssets + filename;
			TextAsset poseAsset = Resources.Load (path) as TextAsset;
			if (poseAsset == null)
				content = "";
			else
				content = poseAsset.text;
		} else {
			filename = emotion + ".txt";
			content = ReadFromFile (filename);
		}
		// Create a pose case for each non-empty line of the content
		List<PoseCase> poseCases = new List<PoseCase>();
		string[] strArray = content.Split ('\n');
		for (int i = 0; i < strArray.Length; i++) {
			if(!strArray[i].Equals("")){
				Pose pose = StringToPose (strArray[i]);
				PoseCase poseCase = new PoseCase (pose, emotion);
				poseCases.Add(poseCase);
			}
		}
		return poseCases;
	}

	// Methods to change from string to classes of the current application
	// and viceversa.
	public string PoseToString(Pose pose){
		return (Vector3ToString (pose.leftHandPos) + ";" +
			Vector3ToString (pose.rightHandPos) + ";" +
			Vector3ToString (pose.leftFootPos) + ";" +
			Vector3ToString (pose.rightFootPos) + ";" +
			Vector3ToString (pose.headPos) + ";" +
			Vector3ToString (pose.headLookDirection)+ ";" +
			pose.openingHandsNormalized [(int)Hand.Left]+ ";" +
			pose.openingHandsNormalized [(int)Hand.Right]);
	}

	public Pose StringToPose(string str){
		string[] strArray = str.Split (';');
		Pose pose = new Pose ();
		pose.leftHandPos = StringToVector3 (strArray [0]);
		pose.rightHandPos = StringToVector3 (strArray [1]);
		pose.leftFootPos = StringToVector3 (strArray [2]);
		pose.rightFootPos = StringToVector3 (strArray [3]);
		pose.headPos = StringToVector3 (strArray [4]);
		pose.headLookDirection = StringToVector3 (strArray [5]);
		if (strArray.Length >= 8) {
			for(Hand hand = Hand.Left; hand<Hand.Right; hand++)
				pose.openingHandsNormalized [(int)hand] = float.Parse (strArray [6+((int) hand)]);
		}
		return pose;
	}

	private string Vector3ToString(Vector3 vector){
		return ("(" + vector.x + "," + vector.y + "," + vector.z + ")");
	}
	private Vector3 StringToVector3(string str){
		string strAux = str.Substring (1, str.Length - 2);
				string[] strArray = strAux.Split(',');
		float x = float.Parse (strArray [0]);
		float y = float.Parse (strArray [1]);
		float z = float.Parse (strArray [2]);
		return new Vector3 (x, y, z);
	}
	/* It writes a file in a relative path with some content. It
	considers whether it is running in unity path or another platform.
	It indicates whether to append or overwrite the content 
	in case it already exists. In case of appending it adds a line break */
	private void WriteIntoFile(string filename, string content, bool append=true){
		string path = GetPersistentPath ();
		try{
			if(append)
				File.AppendAllText(path+filename,content+'\n');
			else
				File.WriteAllText (path+filename, content);
		}catch(IOException e){
			Debug.Log ("Exception when writting in file " + path + "; " +e.Source);
		}
	}
	// It reads a file and returns its content 
	public string ReadFromFile (string filename){
		string path = GetPersistentPath ();
		try{
			if(File.Exists(path + filename)){
				return File.ReadAllText (path + filename);
			}else{
				return "";
			}
		}catch(IOException e){
			Debug.Log ("Exception when reading from file " + path + "; " +e.Source);
			return "error";
		}
	}
	// Gets the appropriate path
	public string GetPersistentPath(){
		string path;
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.OSXEditor ||
			platform == RuntimePlatform.WindowsEditor ||
			platform == RuntimePlatform.LinuxEditor) {
			path = Application.dataPath+"/"+pathPosesInUnityEditor;
		} else {
			path = Application.persistentDataPath;
		}
		return path+"/";
	}

}

/* -------- BAK ----------

// It writes a pose in a text file. Deprecated 
	public void WritePose (Pose pose){
		WriteIntoFile ("prueba.txt", pose.ToString ());
	}
	// It loads a default cases from the Resources Text Asset
	public PoseCase LoadDefaultPoseCase(Emotion emotion){
		TextAsset poseAsset = (TextAsset) Resources.Load (defaultPathPoseAssets);

	}
	Debug.Log ("path asset = " + path);
			
*/
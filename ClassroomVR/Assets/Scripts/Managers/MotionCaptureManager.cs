using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class MotionCaptureManager : MonoBehaviour
	{
        public Transform playerTransform;

		// For builidng poses from the UI
		public PoseBuilder poseBuilder;
		// A pose base for classifying the pose of the UI
		private PoseBase poseBase;
		// A reference to the text classifier result in the UI
		//public Text textClassifierResult;
		// A reference to the text distance in the result UI
		//public Text distResultText;

		// Url of the "InsertGenericData.php" file for this EmoPose application
		private string url = "http://webdiis.unizar.es/~ivangmg/emopose/InsertGenericData.php";
		// Reference to the generic DB manager
		private GenericDBManager dbManager;
		// A pose writer to perform the writing
		private PoseWriter poseWriter;
		// The last result datetime. It will be associated with the following related actions.
		// It uses the URL string format
		private string lastResultDatetime;
		///// References for obtaing data from different places
		//public GameControllerClassifier gameControllerClassifier;
		//public PoseStopwatch poseStopWatch;
		//public ToggleGroup selectedEmoToggleGroup; // In the code any toggle button of the SelectedEmoCanvas (AngryToggle Group)
		//public SelectEmoController selectedEmoController;

		public float delay = 2.0f;
		private float delta = 0.0f;

		// Use this for initialization
		public void init()
		{ 
			//poseWriter = new PoseWriter();

			poseBase = new PoseBase();
			poseBase.AddDefaultCases();

			//dbManager.SetUrl(url);
			//dbManager.SaveDeviceInfo("EmoPose");
		}

		private void ClassifyPoseFromCharacter()
		{
			Pose pose = poseBuilder.CreatePoseFromCharacterWithoutMove(playerTransform.position);
            Debug.Log(pose.ToString());
			Emotion emo = poseBase.Classify(pose);
			string textClassifierResult = emo.ToString();
			string distResultText = textClassifierResult + " distance " + poseBase.lastDistance.ToString("0.0") + "%";
			Debug.Log(distResultText);
		}


		public void update(float deltaTime)
        {
			delta += deltaTime;
			// Habria que hacer que mirara cual es y la añadiera a la lista de leidas.
			if (delta > delay)
			{
				delta = 0.0f;
				ClassifyPoseFromCharacter();
			}
        }

        public void onDestroy()
        {

        }

    }
}
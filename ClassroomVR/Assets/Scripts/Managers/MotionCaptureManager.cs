using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class MotionCaptureManager : MonoBehaviour
	{
		// Para los calculos de la emocion mas utilizada
		struct motionAverage
        {
			public int nTimes;
			public float sumatory;
        }

        public Transform playerTransform;

		// For builidng poses from the UI
		public PoseBuilder poseBuilder;
		// A pose base for classifying the pose of the UI
		private PoseBase poseBase;

		// Url of the "InsertGenericData.php" file for this EmoPose application
		private string url = "http://webdiis.unizar.es/~ivangmg/emopose/InsertGenericData.php";

		// Delay entre calculo de emocion asociada a la pose
		public float delay = 1.0f;
		private float delta = 0.0f;

		// Diccionario para el calculo de las emociones detectadas
		private Dictionary<string, motionAverage> dicEmotions;

		// Use this for initialization
		public void init() {
			poseBase = new PoseBase();
			poseBase.AddDefaultCases();

			dicEmotions = new Dictionary<string, motionAverage>();
		}

		private void ClassifyPoseFromCharacter()
		{
			Pose pose = poseBuilder.CreatePoseFromCharacterWithoutMove(playerTransform.position);
			Emotion emo = poseBase.Classify(pose);
			string key = emo.ToString();

            if (dicEmotions.ContainsKey(key))
            {
				motionAverage val = dicEmotions[key];
				dicEmotions.Remove(key);
				val.nTimes += 1;
				val.sumatory += poseBase.lastDistance;
				dicEmotions.Add(key, val);
				Debug.Log("Update existente");
            }
            else
            {
				motionAverage newEmo = new motionAverage();
				newEmo.nTimes = 1;
				newEmo.sumatory = poseBase.lastDistance;
				dicEmotions.Add(emo.ToString(), newEmo);
				Debug.Log("Añadida nueva");
            }
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

		public void finalResult()
        {
			// Mas cercana a 0
			string closestEmo = "";
			float averageDistanceClosest = float.MaxValue;
			float resAverage = 0;

			// Mas veces detectada
			string moreRepeatedEmo = "";
			float averageDistanceRepeated = float.MaxValue;
			int resRepeated = 0;


			foreach(KeyValuePair<string, motionAverage> emo in dicEmotions)
            {
				resAverage = emo.Value.sumatory / emo.Value.nTimes;

				// Si la distancia media de la emocion es la mas pequeña
				if(resAverage < averageDistanceClosest)
                {
					closestEmo = emo.Key;
					averageDistanceClosest = resAverage;
                }
				// Si la emocion se ha repetido mas veces
				if(emo.Value.nTimes > resRepeated)
                {
					resRepeated = emo.Value.nTimes;
					moreRepeatedEmo = emo.Key;
					averageDistanceRepeated = resAverage;
                }
            }

			Debug.Log("La emocion mas repetida ha sido " + moreRepeatedEmo + " detectada un total de " + resRepeated + " con una distancia media de " + averageDistanceRepeated);
			Debug.Log("La emocion mejor detectada ha sido " + closestEmo + " con una distancia media de " + averageDistanceClosest);
        }

        public void onDestroy()
        {

        }
    }
}
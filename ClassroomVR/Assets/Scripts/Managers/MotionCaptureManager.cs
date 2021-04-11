using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class MotionCaptureManager : MonoBehaviour {
		// Struct para informar del estado final
		public struct finalInfo
        {
			public string closestEmo;
			public float averageDistanceClosest;
			public string moreRepeatedEmo;
			public int resRepeated;
			public float averageDistanceRepeated;

			public void toString()
            {
				Debug.Log("La emocion mas repetida ha sido " + moreRepeatedEmo + " detectada un total de " + resRepeated + " con una distancia media de " + averageDistanceRepeated);
				Debug.Log("La emocion mejor detectada ha sido " + closestEmo + " con una distancia media de " + averageDistanceClosest);
			}
		}

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

		public finalInfo finalResult()
        {
			finalInfo res;

			// Mas cercana a 0
			res.closestEmo = "";
			res.averageDistanceClosest = float.MaxValue;
			float resAverage = 0;

			// Mas veces detectada
			res.moreRepeatedEmo = "";
			res.averageDistanceRepeated = float.MaxValue;
			res.resRepeated = 0;


			foreach(KeyValuePair<string, motionAverage> emo in dicEmotions)
            {
				resAverage = emo.Value.sumatory / emo.Value.nTimes;

				// Si la distancia media de la emocion es la mas pequeña
				if(resAverage < res.averageDistanceClosest)
                {
					res.closestEmo = emo.Key;
					res.averageDistanceClosest = resAverage;
                }
				// Si la emocion se ha repetido mas veces
				if(emo.Value.nTimes > res.resRepeated)
                {
					res.resRepeated = emo.Value.nTimes;
					res.moreRepeatedEmo = emo.Key;
					res.averageDistanceRepeated = resAverage;
                }
            }

			//res.toString();

			return res;
        }

		// Metodo que se encarga de puntuar la emocion detectada mas caracteristica de la escena
		public static int emotionValue(finalInfo res)
        {

			return 0;
        }

        public void onDestroy()
        {

        }
    }
}
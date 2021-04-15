using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class MotionCaptureManager : MonoBehaviour {
		// Struct que guarda la informacion de cada intervalo
		public struct IntervalResult
        {
			/// <summary>
			/// La lista resultEmotion guarda las emociones repetidas consecutivamente.
			/// El vector2 indica:
			/// 1. Nº de repeticiones
			/// 2. Fiabilidad de esa lectura consecutiva
			/// </summary>
			public List<Vector2> resultEmotion;
			// Guarda la emocion respectiva a la lista anterior.
			public List<Emotion> emoRelated;

			/// <summary>
			/// El Diccionario totalEmoRepeated guarda cuantas veces se ha repetido en total una emocion durante el intervalo.
			/// El vector2 indica:
			/// 1.El total de veces que se ha detectado la emocion
			/// 2.Nº de veces que se ha detectado la emocion por mas tiempo seguido
			/// Se utiliza para calcular el numero de emociones totales durante el intervalo y, por tanto, el tiempo del mismo.
			/// </summary>
			public Dictionary<Emotion, Vector2> totalEmoRepeated;
        }

		public Transform playerTransform;

		// For builidng poses from the UI
		public PoseBuilder poseBuilder;
		// A pose base for classifying the pose of the UI
		private PoseBase poseBase;

		// Url of the "InsertGenericData.php" file for this EmoPose application
		private string url = "http://webdiis.unizar.es/~ivangmg/emopose/InsertGenericData.php";

		public bool debug = false;

		// Delay entre calculo de emocion asociada a la pose
		public float delay = 0.5f;
		private float delta = 0.0f;

		// Lista con la info de los intervalos
		static private List<IntervalResult> intervals;

		// Para cada intervalo
		private IntervalResult actualInterval;
		private List<Vector2> actResultEmo;
		private List<Emotion> actEmo;
		private Dictionary<Emotion, Vector2> actEmoRepeated;
		//Para cada fragmento de cada intervalo
		private Emotion lastEmotion;
		private Vector2 recurrentEmo;


		// Use this for initialization
		public void init() {
			poseBase = new PoseBase();
			poseBase.AddDefaultCases();

			Debug.Log(delay);

			//---
			intervals = new List<IntervalResult>();
			//---
			actResultEmo = new List<Vector2>();
			actEmo = new List<Emotion>();
			actEmoRepeated = new Dictionary<Emotion, Vector2>();
			//---
			lastEmotion = Emotion.None;
			recurrentEmo = new Vector2(0, 0);
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

		static public string getIntervalsInfo()
		{
			string intervalsInfo = "";

			int interval = 1;
			foreach (IntervalResult iRes in intervals)
			{
				//---
				intervalsInfo += "Por orden de deteccion durante el intervalo " + interval + ":\n";
				string emoDuringIntervalInfo = "";
				int i = 0;
				foreach (Emotion em in iRes.emoRelated)
				{
					if (iRes.resultEmotion[i].y > 33.3)
					{
						emoDuringIntervalInfo += "Se detecto la emocion " + em.ToString() +
							" con una fiabilidad media del " + iRes.resultEmotion[i].y +
							" un total de " + iRes.resultEmotion[i].x + "\n";
					}
					i++;
				}
				intervalsInfo += emoDuringIntervalInfo;

				//----
				string emotionsDuringIntervalInfo = "";
				foreach (KeyValuePair<Emotion, Vector2> emInfo in iRes.totalEmoRepeated)
				{
					emotionsDuringIntervalInfo += "Se detecto la emocion " + emInfo.Key.ToString() + 
						" un total de " + emInfo.Value.x +
						" veces, con una repeticion maxima de " + emInfo.Value.y + "\n";
				}
				intervalsInfo += emotionsDuringIntervalInfo + "\n";
				interval++;
			}
			return intervalsInfo;
		}

		// Actualiza la informacion de los intervalos con la calculada hasta ahora
		public void nextInterval()
        {
			// Actualizamos la info del intervalo actual
			actualInterval = new IntervalResult();
			actualInterval.resultEmotion = new List<Vector2>();
			actualInterval.resultEmotion = actResultEmo;
			actualInterval.emoRelated = new List<Emotion>();
			actualInterval.emoRelated = actEmo;
			actualInterval.totalEmoRepeated = new Dictionary<Emotion, Vector2>();
			actualInterval.totalEmoRepeated = actEmoRepeated;

			// Guardamos el intervalo en la lista de intervalos.
			intervals.Add(actualInterval);

			// Limpiamos los objetos para el siguiente intervalo
			actResultEmo = new List<Vector2>();
			actEmo = new List<Emotion>();
			actEmoRepeated = new Dictionary<Emotion, Vector2>();
			lastEmotion = Emotion.None;
			recurrentEmo = new Vector2(0, 0);
        }

		// Obtiene la info de la emocion a partir del cuerpo del character
		private void ClassifyPoseFromCharacter()
		{
			Pose pose = poseBuilder.CreatePoseFromCharacterWithoutMove(playerTransform.position);
			Emotion emo = poseBase.Classify(pose);

			storeInfo(emo);
		}

		// Guarda la info cada vez que se detecta una emocion
		private void storeInfo(Emotion emo)
		{
			if (lastEmotion == emo || lastEmotion == Emotion.None)
			{
				if(debug) Debug.Log("Misma emocion " + emo.ToString());
				//repeticiones, fiabilidad
				recurrentEmo.x += 1;
				recurrentEmo.y += poseBase.lastDistance;
			}
			else
			{
				if (debug) Debug.Log("Cambio en la emocion detectada el ultimo intervalo");
				// Preparamos el valor de y para que muestre lo que nos interesa
				recurrentEmo.y = recurrentEmo.y / recurrentEmo.x;

				// Añadimos la emocion detectada repetidamente a la lista
				actResultEmo.Add(recurrentEmo);
				// Emocion asociada a actResultEmo
				actEmo.Add(lastEmotion);

				Vector2 val = new Vector2();
				// Actualizamos las emociones detectadas durante el intervalo actual en el diccionario
				if (actEmoRepeated.ContainsKey(lastEmotion))
				{
					val = actEmoRepeated[lastEmotion];
					actEmoRepeated.Remove(lastEmotion);
					// Numero total de veces que se repite la emo
					val.x += recurrentEmo.x;
					// Duracion mas larga de deteccion de la emo
					if (recurrentEmo.x > val.y) val.y = recurrentEmo.x;
					actEmoRepeated.Add(lastEmotion, val);
					if (debug) Debug.Log("Update de emo existente");
				}
				else
				{
					val.x = recurrentEmo.x;
					val.y = recurrentEmo.x;
					actEmoRepeated.Add(lastEmotion, val);
					if (debug) Debug.Log("Añadida nueva emo");
				}

				// Reiniciamos recurrentEmo
				recurrentEmo = new Vector2(0, 0);
			}
			lastEmotion = emo;
		}
	}
}
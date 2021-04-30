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

		// CSVSerializer
		//private CSVSerializer serializer;

		// Para debug
		public int debugLevel = 0;

		// Delay entre calculo de emocion asociada a la pose
		public float delay = 0.5f;
		private float delta = 0.0f;


		//-----------INFO------------
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

        // Info de cada intervalo
        private List<string> intervalsInfo;
		//----------------------------

		// Use this for initialization
		public void init() {
			poseBase = new PoseBase();
			poseBase.AddDefaultCases();

			//serializer = new CSVSerializer();

			//---
			intervals = new List<IntervalResult>();
			//---
			actResultEmo = new List<Vector2>();
			actEmo = new List<Emotion>();
			actEmoRepeated = new Dictionary<Emotion, Vector2>();
			//---
			lastEmotion = Emotion.None;
			recurrentEmo = new Vector2(0, 0);

            //---InitList---
            intervalsInfo = new List<string>();
		}

		public void update(float deltaTime)
        {
			delta += deltaTime;
			if (delta > delay)
			{
				delta = 0.0f;
				ClassifyPoseFromCharacter();
			}
        }

		//-----------------PUBLICS----------------
		public void saveIntervalsInfo()
		{
			int interval = 1;
			foreach (IntervalResult iRes in intervals)
			{
				string tempString = "";
				// Intervalo 2 (toma de decision)
				if (interval == 2)
				{
					tempString += "Al tomar la decisión de como actuar frente a la situación, nuestro software ha detectado:\n";
					// Buscar la emo mas repetida y la menos
					int moreRepeated = 0;
					string moreRepEmo = "";
					foreach (KeyValuePair<Emotion, Vector2> emInfo in iRes.totalEmoRepeated)
					{
						// Mas repetida
                        if (moreRepeated < emInfo.Value.x)
                        {
							moreRepeated = (int)emInfo.Value.x;
							moreRepEmo = emInfo.Key.ToString();
						}
					}

					tempString += "Que " + moreRepEmo + 
						" ha sido la emoción mas repetida, con una repetición total de " + 
						moreRepeated + " veces.\n";
					tempString += "Se detecto:\n";

					// Buscar cuando se reprodujo la emocion
					int i = 0;
					string emoInfo = "";
					float time = 0.0f;
					foreach (Emotion em in iRes.emoRelated)
					{
						if (em.ToString() == moreRepEmo)
						{
							int a = (int)(iRes.resultEmotion[i].y * 100);
							float fiability = (float)a / 100;
							emoInfo = "En el segundo " + time + " tras el inicio del intervalo, " +
								"con una fiabilidad media del " + fiability +
								"%, un total de " + iRes.resultEmotion[i].x + ".\n";

						}
						time += (iRes.resultEmotion[i].x * 0.5f);
						i++;
					}
					tempString += emoInfo;

				}
				// Intervalos 1 y 3, pre situ y post decision
				else
				{
					//---
					if (interval == 1) tempString += "Ántes de que se produjera la situación critica:\n";
					if (interval == 3) tempString += "Después de tomar la decisión del camino a seguir:\n";

					string emoDuringIntervalInfo = "";
					int i = 0;
					foreach (Emotion em in iRes.emoRelated)
					{
						if (iRes.resultEmotion[i].y > 33.3)
						{
							int a = (int)(iRes.resultEmotion[i].y * 100);
							float fiability = (float)a / 100;
							emoDuringIntervalInfo += "Se detecto la emoción " + em.ToString() +
								" con una fiabilidad media del " + fiability +
								"%, un total de " + iRes.resultEmotion[i].x + ".\n";
						}
						i++;
					}
					tempString += emoDuringIntervalInfo;

					//----
					string emotionsDuringIntervalInfo = "\nEn total durante el intervalo:\n";
					foreach (KeyValuePair<Emotion, Vector2> emInfo in iRes.totalEmoRepeated)
					{
						emotionsDuringIntervalInfo += "Se detecto la emoción " + emInfo.Key.ToString() +
							" un total de " + emInfo.Value.x +
							" veces, con una repetición maxima de " + emInfo.Value.y + "\n";
					}
					tempString += emotionsDuringIntervalInfo;
				} //end else

                // Guardamos
                intervalsInfo.Add(tempString);
                interval++;
			} // end foreach
		} // end saveIntervalsInfo

        public string getIntInfo(int i)
        {
            return intervalsInfo[i];
        }

		// Actualiza la informacion de los intervalos con la calculada hasta ahora
		public void nextInterval()
        {
			storeInfo(Emotion.None);

			// Actualizamos la info del intervalo actual
			actualInterval = new IntervalResult();
			actualInterval.resultEmotion = new List<Vector2>();
			actualInterval.resultEmotion = actResultEmo;
			actualInterval.emoRelated = new List<Emotion>();
			actualInterval.emoRelated = actEmo;
			actualInterval.totalEmoRepeated = new Dictionary<Emotion, Vector2>();
			actualInterval.totalEmoRepeated = actEmoRepeated;

			//-------
			if (debugLevel == 3)
			{
				Debug.Log("Por orden de deteccion durante el intervalo " + 0 + ":");

				int i = 0;
				foreach (Emotion em in actEmo)
				{
					Debug.Log("Se detecto la emocion " + em.ToString() +
						" con una fiabilidad media del " + actResultEmo[i].y +
						" un total de " + actResultEmo[i].x);
				}

				//----
				Debug.Log("En total durante la escena:");
				foreach (KeyValuePair<Emotion, Vector2> emInfo in actEmoRepeated)
				{
					Debug.Log("Se detecto la emocion " + emInfo.Key.ToString() +
						" un total de " + emInfo.Value.x +
						" veces, con una repeticion maxima de " + emInfo.Value.y);
				}
				Debug.Log("-------------------------");
			}
			//-------

			// Guardamos el intervalo en la lista de intervalos.
			intervals.Add(actualInterval);
			CSVSerializer.saveData("\n\n");

			// Limpiamos los objetos para el siguiente intervalo
			actResultEmo = new List<Vector2>();
			actEmo = new List<Emotion>();
			actEmoRepeated = new Dictionary<Emotion, Vector2>();
			lastEmotion = Emotion.None;
			recurrentEmo = new Vector2(0, 0);
        }

		//-----------------PRIVATES----------------
		// Obtiene la info de la emocion a partir del cuerpo del character
		private void ClassifyPoseFromCharacter()
		{
			Pose pose = poseBuilder.CreatePoseFromCharacterWithoutMove(playerTransform.position);
			Emotion emo = poseBase.Classify(pose);

            if(debugLevel == 1) Debug.Log(emo.ToString());

			// LOGS
			string CSVData = emo.ToString() + ";" + pose.ToStringNoNames() + ";" + poseBase.lastDistance;
			CSVData = CSVData.Replace(",", "/");
			CSVData = CSVData.Replace(";", ",");
			CSVData += "\n";

			CSVSerializer.saveData(CSVData);

			storeInfo(emo);
		}

		// Guarda la info cada vez que se detecta una emocion
		private void storeInfo(Emotion emo)
		{
			if (lastEmotion == emo || lastEmotion == Emotion.None)
			{
				if(debugLevel == 2) Debug.Log("Misma emocion " + emo.ToString());
				//repeticiones, fiabilidad
				recurrentEmo.x += 1;
				recurrentEmo.y += poseBase.lastDistance;
			}
			else
			{
				if (debugLevel == 2) Debug.Log("Cambio en la emocion detectada el ultimo intervalo " + emo.ToString());
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
				}
				else
				{
					val.x = recurrentEmo.x;
					val.y = recurrentEmo.x;
					actEmoRepeated.Add(lastEmotion, val);
				}

				// Reiniciamos recurrentEmo
				recurrentEmo = new Vector2(0, 0);
			}
			lastEmotion = emo;
		}
	}
}
using ClassRoomVR;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
namespace ClassRoomVR
{
    /// <summary>
    /// Clima de los estudiantes
    /// Hereda de <see cref="GenericSingleton{ClimateManager}"/>.
    /// </summary>
    public class ClimateManager : GenericSingleton<ClimateManager>
    {
        //influido por el comportamiento del estudiante
        private float _environmentalClimate; // [-1,1]

        private Dictionary<string, float> studentBehaviorWeights;

        private Dictionary<EventSittingAnimations, float> behaviorValues;

        // Start is called before the first frame update
        [SerializeField] private string sittingBehaviorsJsonPath;
        void Start()
        {
            _environmentalClimate = 0.0f;
            LoadBehaviorValues();

        }
        /// <summary>
        /// M�todo para cargar los valores desde el archivo JSON
        /// </summary>
        private void LoadBehaviorValues()
        {
            string filePath = Path.Combine(Application.dataPath, sittingBehaviorsJsonPath);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                // Deserializar el JSON en un diccionario
                Dictionary<string, float> tempValues = JsonUtility.FromJson<Wrapper>(json).ToDictionary();

                // Convertir las claves a enumeradores
                behaviorValues = new Dictionary<EventSittingAnimations, float>();

                foreach (var kvp in tempValues)
                {
                    if (System.Enum.TryParse(kvp.Key, out EventSittingAnimations behavior))
                    {
                        behaviorValues[behavior] = kvp.Value;
                    }
                }

                Debug.Log("Behavior values loaded successfully.");
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }
        /// <summary>
        /// M�todo para obtener el valor de un comportamiento
        /// </summary>
        public float GetBehaviorValue(EventSittingAnimations behavior)
        {
            if (behaviorValues != null && behaviorValues.TryGetValue(behavior, out float value))
            {
                return value;
            }

            Debug.LogWarning($"Behavior {behavior} not found in dictionary.");
            return 0.0f; // Valor por defecto
        }


        /// <summary>
        /// Establecer el tamaño de estudiantes para contener el peso del comportamiento actual del estudiante
        /// </summary>
        public void SetStudents(Dictionary<string, Student> students)
        {
            studentBehaviorWeights = students.Keys.ToDictionary(key => key, value => 0f);
        }

        public void SetWeight(string studentName, EventSittingAnimations behavior)
        {
            // Si el estudiante existe en el diccionario
            if (studentBehaviorWeights.ContainsKey(studentName))
            {
                float weight = GetBehaviorValue(behavior);
                // Ajustar el peso del estudiante
                studentBehaviorWeights[studentName] = weight;
                Debug.Log($"Weight set to {weight} for student {studentName}");
                RecalculateClimate();
            }
            else
            {
                // En caso de que el estudiante no exista
                Debug.LogWarning($"Student {studentName} not found in the dictionary.");
            }
        }

        /// <summary>
        /// Recalcula el clima basado en las influencias actuales
        /// </summary>
        private void RecalculateClimate()
        {
            float newEnvironmentalClimate = 0;
            // Recorriendo el diccionario con foreach
            foreach (var s in studentBehaviorWeights)
            {
                newEnvironmentalClimate += s.Value;
            }

            _environmentalClimate = newEnvironmentalClimate / studentBehaviorWeights.Count;
            Debug.Log("ACTUAL CLIMATE: "+ _environmentalClimate);
        }
    }
}
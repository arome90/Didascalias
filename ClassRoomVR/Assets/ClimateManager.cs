using ClassRoomVR;
using MathNet.Numerics.Distributions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

        // Start is called before the first frame update
        void Start()
        {
            _environmentalClimate = 0.0f;
        }

        /// <summary>
        /// Establecer el tamaño de estudiantes para contener el peso del comportamiento actual del estudiante
        /// </summary>
        public void SetStudents(Dictionary<string, Student> students)
        {
            studentBehaviorWeights = students.Keys.ToDictionary(key => key, value => 0f);
        }

        public void SetWeight(string studentName, float weight)
        {
            // Si el estudiante existe en el diccionario
            if (studentBehaviorWeights.ContainsKey(studentName))
            {
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

        }
    }
}
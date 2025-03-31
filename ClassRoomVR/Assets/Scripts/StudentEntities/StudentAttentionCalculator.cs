using UnityEngine;
using MathNet.Numerics.Statistics;

namespace ClassRoomVR
{
    /// <summary>
    /// Calcula el promedio de atención de los estudiantes en la clase.
    /// </summary>
    public class StudentAttentionCalculator : MonoBehaviour
    {
        /// <summary>
        /// Promedio de atención calculado.
        /// </summary>
        [SerializeField] private float _attentionAverage;

        private Student2[] _students;
        private RunningStatistics _attentionStatistics;

        private void Start()
        {
            // Obtiene los estudiantes del gestor de la clase y inicializa las estadísticas.
            _students = GetComponent<ClassManager>().GetStudents();
            _attentionStatistics = new RunningStatistics();

            // Inicia la llamada repetida para calcular el promedio de atención.
            InvokeRepeating(nameof(CalculateAttentionAverage), 2.5f, 2.5f);
        }

        /// <summary>
        /// Calcula el promedio de atención de todos los estudiantes y actualiza la variable Media.
        /// </summary>
        private void CalculateAttentionAverage()
        {
            foreach (var student in _students)
            {
                float studentAttention = student.GetBehavior().AttentionLevel;
                student.GetBehavior().CalculateAttentionAverage();
                _attentionStatistics.Push(studentAttention);
            }

            _attentionAverage = (float)_attentionStatistics.Mean;
        }
        /// <summary>
        /// Muestra el promedio final de atención cuando la aplicación se cierra.
        /// </summary>
        private void OnApplicationQuit()
        {
            Debug.Log("Promedio final de atención: " + _attentionAverage);
        }
    }
}

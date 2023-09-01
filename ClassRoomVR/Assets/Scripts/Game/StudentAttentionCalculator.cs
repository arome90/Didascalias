using UnityEngine;
using MathNet.Numerics.Statistics; // Asegúrate de que MathNet.Numerics.Statistics esté instalado

//namespace ClassRoomVR
//{
//    public class StudentAttentionCalculator : MonoBehaviour
//    {
//        [SerializeField] private float attentionAverage;
//        private float currentAttentionAverage;
//        private Student[] students;
//        private int count;

//        private void Start()
//        {
//            count = 0;
//            students = GetComponent<ClassManager>().GetStudents();
//            InvokeRepeating(nameof(CalculateAttentionAverage), 2.5f, 2.5f);
//        }

//        private void CalculateAttentionAverage()
//        {
//            currentAttentionAverage = CalculateCurrentAttentionAverage();

//            float sum = attentionAverage * count + currentAttentionAverage;
//            count++;
//            attentionAverage = sum / count;

//            //Debug.Log("Current attention average: " + currentAttentionAverage);
//        }

//        private float CalculateCurrentAttentionAverage()
//        {
//            float totalAttention = 0;

//            foreach (var student in students)
//            {
//                float studentAttention = student.GetBehavior().AttentionLevel;
//                student.GetBehavior().CalculateAttentionAverage();

//                totalAttention += studentAttention;
//            }

//            return totalAttention / students.Length;
//        }

//        private void OnApplicationQuit()
//        {
//            Debug.Log("Final attention average: " + attentionAverage);
//        }
//    }
//}



namespace ClassRoomVR
{
    public class StudentAttentionCalculator : MonoBehaviour
    {
        [SerializeField] private float attentionAverage;
        private Student[] students;
        private RunningStatistics attentionStatistics = new RunningStatistics();

        private void Start()
        {
            students = GetComponent<ClassManager>().GetStudents();
            InvokeRepeating(nameof(CalculateAttentionAverage), 2.5f, 2.5f);
        }

        private void CalculateAttentionAverage()
        {
            foreach (var student in students)
            {
                float studentAttention = student.GetBehavior().AttentionLevel;
                student.GetBehavior().CalculateAttentionAverage();

                attentionStatistics.Push(studentAttention);
            }

            attentionAverage = (float)attentionStatistics.Mean;

            //Debug.Log("Current attention average: " + attentionAverage);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Final attention average: " + attentionAverage);
        }
    }
}

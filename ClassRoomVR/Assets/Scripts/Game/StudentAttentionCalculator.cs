using UnityEngine;

namespace ClassRoomVR
{
    public class StudentAttentionCalculator : MonoBehaviour
    {
        [SerializeField] private float attentionAverage;
        private float currentAttentionAverage;
        private Student[] students;
        private int count;
        private void Start()
        {
            count = 0;
            students = GetComponent<ClassManager>().GetStudents();
            InvokeRepeating("CalculateAttentionAverage", 2.5f, 2.5f);
        }

        private void CalculateAttentionAverage()
        {
            currentAttentionAverage = 0;

            for (int i = 0; i < students.Length; i++)
            {
                float studentAttention = students[i].GetBehavior().AttentionLevel;
                students[i].GetBehavior().CalculateAttentionAverage();

                currentAttentionAverage += studentAttention;
            }

            currentAttentionAverage /= students.Length;

            float sum = attentionAverage * count + currentAttentionAverage;
            count++;
            attentionAverage = sum / count;
            //Debug.Log("Current attention average: " + currentAttentionAverage);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("Final attention average: " + attentionAverage);
        }
    }
}
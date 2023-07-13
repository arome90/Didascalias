using UnityEngine;

namespace ClassRoomVR
{
    public class StudentBehavior : MonoBehaviour
    {
        private float attentionLevel = 50.0f; // a>50 attentive a<50 distracted
        public float AttentionLevel => attentionLevel;

        [SerializeField] private bool disruptiveBehavior;
        [SerializeField] private float decisionTime = 2.5f;
        public float DecisionTime => decisionTime;

        [SerializeField] private float attentionAddition = 30;
        [SerializeField] private float attentionSubtraction = 20;

        [SerializeField] private float additionMultiplier = 0.2f;
        [SerializeField] private float subtractionMultiplier = 0.1f;

        [SerializeField] private float distanceFactorAddition = 2.0f;
        [SerializeField] private float distanceFactorSubtraction = 2.0f;

        private Student student;
        private float averageAttentionLevel;
        private int count;
        private Transform player;
        private float additionMultiplierAux;
        private float subtractionMultiplierAux;

        private void Start()
        {
            student = GetComponent<Student>();
            player = Camera.main.transform;
            additionMultiplierAux = additionMultiplier;
            subtractionMultiplierAux = subtractionMultiplier;
            InvokeRepeating("MakeDecision", decisionTime, decisionTime);
        }

        public void AddAttention()
        {
            float distance = distanceFactorAddition * (1 - Vector3.Distance(transform.position, player.transform.position) / 10);
            attentionLevel += attentionAddition * (1 + additionMultiplier) * (1 + distance);
            if (attentionLevel > 100) { attentionLevel = 100; }
            additionMultiplier += additionMultiplierAux;
            subtractionMultiplier = subtractionMultiplierAux;
        }

        public void SubtractAttention()
        {
            float distance = distanceFactorSubtraction * (1 + Vector3.Distance(transform.position, player.transform.position) / 10);
            attentionLevel -= attentionSubtraction * (1 + subtractionMultiplier) * (1 + distance);
            if (attentionLevel <= 0) { attentionLevel = 0; }
            subtractionMultiplier += subtractionMultiplierAux;
            additionMultiplier = additionMultiplierAux;
        }

        public void SetDisruptive(bool value)
        {
            disruptiveBehavior = value;
        }

        private void MakeDecision()
        {
            if (GameManager.Instance.isPause) return;


            if (student.IsStudentInFieldOfVision())
                AddAttention();
            else
                SubtractAttention();
        }

        public float CalculateAttentionAverage()
        {
            float sum = averageAttentionLevel * count + attentionLevel;
            count++;
            averageAttentionLevel = sum / count;
            return averageAttentionLevel;
        }

        public float GetAttentionAverage()
        {
            return averageAttentionLevel;
        }
    }
}
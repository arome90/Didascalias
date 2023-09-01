
using UnityEngine;


namespace ClassRoomVR
{
    public class StudentBehavior : MonoBehaviour
    {
        //TO DO . VOLVER A PONER MULTIPLICADORES
        private float attentionLevel = 50.0f; // a>50 attentive a<50 distracted
        private float attentionLevelAux = 50.0f; // a>50 attentive a<50 distracted
        public float AttentionLevel => attentionLevel;
        public float resta;
        [SerializeField] private bool disruptiveBehavior;
        [SerializeField] private float decisionTime = 2.5f;
        public float DecisionTime => decisionTime;

        [SerializeField] private float attentionAddition = 30;
        [SerializeField] private float attentionSubtraction = 20;


        [SerializeField] private float distanceFactorAddition = 2.0f;
        [SerializeField] private float distanceFactorSubtraction = 2.0f;

        private Student student;
        private Transform player;

        MathNet.Numerics.Statistics.RunningStatistics statistics;

        private void Start()
        {
            statistics = new MathNet.Numerics.Statistics.RunningStatistics();
            student = GetComponent<Student>();
            player = Camera.main.transform;
            InvokeRepeating("MakeDecision", decisionTime, decisionTime);
        }
        public void ModifyAttention()
        {
            float normalizedDistance = CalculateDistanceToPlayerMap();
            UpdateAttentionLevel(normalizedDistance);
        }

        private void UpdateAttentionLevel(float factor)
        {   //aux para test
            attentionLevelAux = attentionLevel;

            if (student.IsStudentInFieldOfVision()) attentionLevel += attentionAddition * distanceFactorAddition * (1 - factor);
            else attentionLevel -= attentionSubtraction * distanceFactorSubtraction * (1 + factor);
            attentionLevel = Mathf.Clamp(attentionLevel, 0f, 100f);

            //aux para test
            resta = attentionLevel - attentionLevelAux;
        }


        private float CalculateDistanceToPlayerMap()
        {
            float d= Vector3.Distance(transform.position, player.transform.position);
            //1.5 dis minima y 12 distancia maxima en este aula
            return  Unity.Mathematics.math.remap(1.5f, 12f, 0f, 1f, d);
        }

        public void SetDisruptive(bool value)
        {
            disruptiveBehavior = value;
        }

        private void MakeDecision()
        {
            if (GameManager.Instance.IsPause) return;
            ModifyAttention();
        }


        public double CalculateAttentionAverage()
        {
            statistics.Push(attentionLevel);
            return statistics.Mean;
        }

        public double GetAttentionAverage()
        {
            return statistics.Mean;
        }
    }
}





//namespace ClassRoomVR
//{
//    public class StudentBehavior : MonoBehaviour
//    {
//        private float attentionLevel = 50.0f;
//        public float AttentionLevel => attentionLevel;

//        [SerializeField] private bool disruptiveBehavior;
//        [SerializeField] private float decisionTime = 2.5f;

//        [SerializeField] private float attentionAddition = 30;
//        [SerializeField] private float attentionSubtraction = 20;

//        [SerializeField] private float distanceFactor = 2.0f;

//        private Student student;
//        private Transform player;
//        MathNet.Numerics.Statistics.RunningStatistics statistics;


//        private void Start()
//        {
//            statistics = new MathNet.Numerics.Statistics.RunningStatistics();

//            student = GetComponent<Student>();
//            player = Camera.main.transform;
//            InvokeRepeating(nameof(MakeDecision), decisionTime, decisionTime);
//        }

//        private void MakeDecision()
//        {
//            if (GameManager.Instance.IsPause) return;

//            float distance = distanceFactor * (1 - Mathf.Clamp01(Vector3.Distance(transform.position, player.transform.position) / 10));

//            if (student.IsStudentInFieldOfVision())
//                UpdateAttention(attentionAddition, distance);
//            else
//                UpdateAttention(-attentionSubtraction, distance);
//        }

//        private void UpdateAttention(float attentionChange, float distance)
//        {
//            attentionLevel += attentionChange * (1 + distance);
//            attentionLevel = Mathf.Clamp(attentionLevel, 0, 100);
//        }

//        public void SetDisruptive(bool value)
//        {
//            disruptiveBehavior = value;
//        }
//        public double CalculateAttentionAverage()
//        {
//            statistics.Push(attentionLevel);
//            return statistics.Mean;
//        }

//        public double GetAttentionAverage()
//        {
//            return statistics.Mean;
//        }
//    }
//}


        //public void AddAttention()
        //{
        //    attentionLevelAux = attentionLevel;
        //    float distance = distanceFactorAddition * (1 -  Vector3.Distance(transform.position, player.transform.position) /10);
        //    attentionLevel += attentionAddition * (1 + distance);
        //    if (attentionLevel > 100) { attentionLevel = 100; }

        //    resta = attentionLevel - attentionLevelAux;
        //   // Debug.Log(Vector3.Distance(transform.position, player.transform.position));
        //}

        //public void SubtractAttention()
        //{
        //    attentionLevelAux = attentionLevel;
        //    float distance = distanceFactorSubtraction * (1 + Vector3.Distance(transform.position, player.transform.position) / 10);
        //    attentionLevel -= attentionSubtraction * (1 + distance);
        //    if (attentionLevel <= 0) { attentionLevel = 0; }

        //    resta = attentionLevel - attentionLevelAux;

        //}
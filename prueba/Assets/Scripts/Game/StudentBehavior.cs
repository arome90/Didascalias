
//using Meta.WitAi.Lib;
//using System.Collections;
//using UnityEngine;


//namespace ClassRoomVR
//{
//    public class StudentBehavior : MonoBehaviour
//    {
//        //TO DO . VOLVER A PONER MULTIPLICADORES
//        private float attentionLevel = 50.0f; // a>50 attentive a<50 distracted
//        private float attentionLevelAux = 50.0f; // a>50 attentive a<50 distracted
//        public float AttentionLevel => attentionLevel;
//        public float resta;
//        [SerializeField] private float decisionTime = 2.5f;
//        public float DecisionTime => decisionTime;

//        [SerializeField] private float attentionAddition = 30;
//        [SerializeField] private float attentionSubtraction = 20;


//        [SerializeField] private float distanceFactorAddition = 2.0f;
//        [SerializeField] private float distanceFactorSubtraction = 2.0f;

//        private Student student;
//        private Transform player;

//        MathNet.Numerics.Statistics.RunningStatistics statistics;

//        private void Start()
//        {
//            statistics = new MathNet.Numerics.Statistics.RunningStatistics();
//            student = GetComponent<Student>();
//            player = Camera.main.transform;
//            InvokeRepeating("MakeDecision", decisionTime, decisionTime);


//            Invoke(nameof(InitRenderer), 1);
//            Invoke(nameof(RandomThing), 2);
//        }

//        public void InitRenderer() 
//        {
//            meshRenderer = transform.GetChild(5).GetComponent<SkinnedMeshRenderer>();
//            list = new float[6];
//        }
//        public void ModifyAttention()
//        {
//            float normalizedDistance = CalculateDistanceToPlayerMap();
//            UpdateAttentionLevel(normalizedDistance);
//        }

//        private void UpdateAttentionLevel(float factor)
//        {   //aux para test
//            attentionLevelAux = attentionLevel;

//            if (student.IsStudentInFieldOfVision()) attentionLevel += attentionAddition * distanceFactorAddition * (1 - factor);
//            else attentionLevel -= attentionSubtraction * distanceFactorSubtraction * (1 + factor);
//            attentionLevel = Mathf.Clamp(attentionLevel, 0f, 100f);
            
//            //aux para test
//            resta = attentionLevel - attentionLevelAux;
//        }


//        private float CalculateDistanceToPlayerMap()
//        {
//            float d= Vector3.Distance(transform.position, player.transform.position);
//            //1.5 dis minima y 12 distancia maxima en este aula
//            return  Unity.Mathematics.math.remap(1.5f, 12f, 0f, 1f, d);
//        }

       

//        private void MakeDecision()
//        {
//            if (GameManager.Instance.IsPause) return;
//            ModifyAttention();
//        }

//        public void SetAttention() 
//        {
//            attentionLevel = Mathf.Max(attentionLevel, 65f);
//            StartCoroutine(SetExpression(StudentProperties.Expressions.Smile));
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




//        SkinnedMeshRenderer meshRenderer;
      
//        private float[] list;


//        public void SetPestañeo()
//        {
//            SetBlendShape(StudentProperties.Expressions.CloseEyes, 100);
//            if (attentionLevel < 40) { StartCoroutine(SetExpression(StudentProperties.Expressions.Bored)); }
//            Invoke(nameof(SetAbrirOjos), 0.2f);

//        }

//        void RandomThing()
//        {
//            float randomTime = Random.Range(2f, 3f);
//            SetPestañeo();
//            Invoke("RandomThing", randomTime);

//        }
//        public void SetAbrirOjos()
//        {
//            SetBlendShape(StudentProperties.Expressions.CloseEyes, 0);

//        }
//        public void SetBlendShape(StudentProperties.Expressions expresion, float value)
//        {
//            meshRenderer.SetBlendShapeWeight((int)expresion, value);
//        }

//        public void SetBlendShape(int expresion, float value)
//        {
//            meshRenderer.SetBlendShapeWeight(expresion, value);
//        }

//        //private void Update()
//        //{

//        //    if (Input.GetKeyDown(KeyCode.C))
//        //    {
//        //        StopAllCoroutines();
//        //        StartCoroutine(SetExpression(Expresiones.Enfadado));
//        //    }
//        //    if (Input.GetKeyDown(KeyCode.V))
//        //    {
//        //        StopAllCoroutines();
//        //        StartCoroutine(SetExpression(Expresiones.Quejarse));
//        //    }
//        //    if (Input.GetKeyDown(KeyCode.B))
//        //    {
//        //        StopAllCoroutines();
//        //        StartCoroutine(SetExpression(Expresiones.Sonreir));
//        //    }
//        //    if (Input.GetKeyDown(KeyCode.N))
//        //    {
//        //        StopAllCoroutines();
//        //        StartCoroutine(SetExpression(Expresiones.Dormido));
//        //    }
//        //    if (Input.GetKeyDown(KeyCode.M))
//        //    {
//        //        StopAllCoroutines();
//        //        StartCoroutine(SetExpression(Expresiones.LLorar));
//        //    }

//        //}


//        public IEnumerator SetExpression(StudentProperties.Expressions exp)
//        {
//            while (meshRenderer.GetBlendShapeWeight((int)exp) != 100)
//            {
//                for (int i = 0; i < list.Length; i++)
//                {
//                    if ((int)exp == i)
//                    {
//                        list[i] = Mathf.Min(100, list[i] + 15);
//                        SetBlendShape(exp, list[i]);
//                    }
//                    else if (meshRenderer.GetBlendShapeWeight(i) > 0)
//                    {
//                        list[i] = Mathf.Max(0, list[i] - 20);
//                        SetBlendShape(i, list[i]);
//                    }
//                }
//                yield return new WaitForSeconds(0.5f);
//            }

//        }


//    }
//}





////namespace ClassRoomVR
////{
////    public class StudentBehavior : MonoBehaviour
////    {
////        private float attentionLevel = 50.0f;
////        public float AttentionLevel => attentionLevel;

////        [SerializeField] private bool disruptiveBehavior;
////        [SerializeField] private float decisionTime = 2.5f;

////        [SerializeField] private float attentionAddition = 30;
////        [SerializeField] private float attentionSubtraction = 20;

////        [SerializeField] private float distanceFactor = 2.0f;

////        private Student student;
////        private Transform player;
////        MathNet.Numerics.Statistics.RunningStatistics statistics;


////        private void Start()
////        {
////            statistics = new MathNet.Numerics.Statistics.RunningStatistics();

////            student = GetComponent<Student>();
////            player = Camera.main.transform;
////            InvokeRepeating(nameof(MakeDecision), decisionTime, decisionTime);
////        }

////        private void MakeDecision()
////        {
////            if (GameManager.Instance.IsPause) return;

////            float distance = distanceFactor * (1 - Mathf.Clamp01(Vector3.Distance(transform.position, player.transform.position) / 10));

////            if (student.IsStudentInFieldOfVision())
////                UpdateAttention(attentionAddition, distance);
////            else
////                UpdateAttention(-attentionSubtraction, distance);
////        }

////        private void UpdateAttention(float attentionChange, float distance)
////        {
////            attentionLevel += attentionChange * (1 + distance);
////            attentionLevel = Mathf.Clamp(attentionLevel, 0, 100);
////        }

////        public void SetDisruptive(bool value)
////        {
////            disruptiveBehavior = value;
////        }
////        public double CalculateAttentionAverage()
////        {
////            statistics.Push(attentionLevel);
////            return statistics.Mean;
////        }

////        public double GetAttentionAverage()
////        {
////            return statistics.Mean;
////        }
////    }
////}


//        //public void AddAttention()
//        //{
//        //    attentionLevelAux = attentionLevel;
//        //    float distance = distanceFactorAddition * (1 -  Vector3.Distance(transform.position, player.transform.position) /10);
//        //    attentionLevel += attentionAddition * (1 + distance);
//        //    if (attentionLevel > 100) { attentionLevel = 100; }

//        //    resta = attentionLevel - attentionLevelAux;
//        //   // Debug.Log(Vector3.Distance(transform.position, player.transform.position));
//        //}

//        //public void SubtractAttention()
//        //{
//        //    attentionLevelAux = attentionLevel;
//        //    float distance = distanceFactorSubtraction * (1 + Vector3.Distance(transform.position, player.transform.position) / 10);
//        //    attentionLevel -= attentionSubtraction * (1 + distance);
//        //    if (attentionLevel <= 0) { attentionLevel = 0; }

//        //    resta = attentionLevel - attentionLevelAux;

//        //}
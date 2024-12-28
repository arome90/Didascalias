using MathNet.Numerics.Statistics;
using System.Collections;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Controla el comportamiento del estudiante en el entorno VR.
    /// </summary>
    public class StudentBehavior : MonoBehaviour
    {
        private const float AttentionMin = 0f;
        private const float AttentionMax = 100f;


        private float _attentionLevel = 50.0f; // >50 atento, <50 distraído
        public float AttentionLevel => _attentionLevel;

        [SerializeField] private float _decisionTime = 4f;
        public float DecisionTime => _decisionTime;

        [SerializeField] private float _attentionAddition = 0f;
        [SerializeField] private float _attentionSubtraction = 2f;
        [SerializeField] private float _distanceFactorAddition = 2.1f;
        [SerializeField] private float _distanceFactorSubtraction = 2.0f;
        
        [SerializeField] private float _conscientiousnessInfluence = 1f;
        [SerializeField] private float _agreeablenessInfluence = 0.4f;
        [SerializeField] private float _neuroticismInfluence = 0.2f;
        [SerializeField] private float range = 20f;

        private Student _student;
        private Transform _player;
        private RunningStatistics _statistics;


        private void Start()
        {
            _statistics = new RunningStatistics();
            _student = GetComponent<Student>();
            _player = Camera.main.transform;
       
            InvokeRepeating(nameof(MakeDecision), _decisionTime, _decisionTime);
        }

        public void InitializeAttention(Personality personality)
        {
            _attentionLevel += (personality.Conscientiousness < 0.5 ? -personality.Conscientiousness : personality.Conscientiousness) * _attentionAddition * _conscientiousnessInfluence;
            _attentionLevel += (personality.Agreeableness < 0.5 ? -personality.Agreeableness : personality.Agreeableness) * _attentionAddition * _agreeablenessInfluence;
            _attentionLevel += (personality.Neuroticism < 0.5 ? -personality.Neuroticism : personality.Neuroticism) * _attentionAddition * _neuroticismInfluence;

            _attentionLevel += Random.Range(-1.0f, 1.0f) * range;
            _attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
        }

        private void MakeDecision()
        {
            if (GameManager.Instance.IsPause) return;

            float distanceFactor = CalculateDistanceToPlayerMap();
            UpdateAttentionLevel(distanceFactor);
        }

        /// <summary>
        /// Calcula un factor basado en la distancia al jugador.
        /// </summary>
        /// <returns>Factor de distancia normalizado entre 0 y 1.</returns>
        private float CalculateDistanceToPlayerMap()
        {
            return Mathf.InverseLerp(1.5f, 12f, Vector3.Distance(transform.position, _player.position));
        }

        /// <summary>
        /// Actualiza el nivel de atención del estudiante basado en el factor.
        /// </summary>
        /// <param name="factor">Factor de distancia normalizado.</param>
        private void UpdateAttentionLevel(float factor)
        {
            if (_student.IsStudentInFieldOfVision())
            {
                _attentionLevel += _attentionAddition * _distanceFactorAddition * (1 - factor);
                //Debug.Log(_attentionAddition);
            }
            else
            {
                _attentionLevel -= _attentionSubtraction * _distanceFactorSubtraction * (1 + factor);
            }

            //_attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
        }

        /// <summary>
        /// Establece un nivel mínimo de atención y cambia la expresión.
        /// </summary>
        public void SetAttention()
        {
            _attentionLevel = Mathf.Max(_attentionLevel, 65f);
          //  StartCoroutine(ChangeExpression(Expressions.Smile));
        }

        /// <summary>
        /// Calcula el promedio de atención usando estadísticas acumulativas.
        /// </summary>
        /// <returns>Promedio de atención.</returns>
        public double CalculateAttentionAverage()
        {
            _statistics.Push(_attentionLevel);
            return _statistics.Mean;
        }

    }
}

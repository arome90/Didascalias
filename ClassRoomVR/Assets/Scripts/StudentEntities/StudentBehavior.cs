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
        private const int SkinnedMeshIndex = 5;
        private const float BlinkIntervalMin = 2f;
        private const float BlinkIntervalMax = 3f;

        private float _attentionLevel = 50.0f; // >50 atento, <50 distraído
        public float AttentionLevel => _attentionLevel;

        [SerializeField] private float _decisionTime = 2.5f;
        public float DecisionTime => _decisionTime;

        [SerializeField] private float _attentionAddition = 30f;
        [SerializeField] private float _attentionSubtraction = 20f;
        [SerializeField] private float _distanceFactorAddition = 2.1f;
        [SerializeField] private float _distanceFactorSubtraction = 2.0f;

        private Student _student;
        private Transform _player;
        private RunningStatistics _statistics;
        private SkinnedMeshRenderer _meshRenderer;
        private float[] _blendShapeWeights;

        private void Start()
        {
            _statistics = new RunningStatistics();
            _student = GetComponent<Student>();
            _player = Camera.main.transform;
            _blendShapeWeights = new float[6];

            StartCoroutine(CallLineAfterDelay());

            InvokeRepeating(nameof(MakeDecision), _decisionTime, _decisionTime);
            StartCoroutine(RandomBlink());
        }

        /// <summary>
        /// Espera un segundo para obtener el componente SkinnedMeshRenderer.
        /// </summary>
        private IEnumerator CallLineAfterDelay()
        {
            yield return new WaitForSeconds(1f);
            _meshRenderer = transform.GetChild(SkinnedMeshIndex).GetComponent<SkinnedMeshRenderer>();
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
            }
            else
            {
                _attentionLevel -= _attentionSubtraction * _distanceFactorSubtraction * (1 + factor);
            }

            _attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
        }

        /// <summary>
        /// Establece un nivel mínimo de atención y cambia la expresión.
        /// </summary>
        public void SetAttention()
        {
            _attentionLevel = Mathf.Max(_attentionLevel, 65f);
            StartCoroutine(ChangeExpression(Expresiones.Sonreir));
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

        /// <summary>
        /// Maneja el parpadeo aleatorio del estudiante.
        /// </summary>
        private IEnumerator RandomBlink()
        {
            while (true)
            {
                yield return Blink(Expresiones.Pestañear);
                yield return new WaitForSeconds(Random.Range(BlinkIntervalMin, BlinkIntervalMax));
            }
        }

        /// <summary>
        /// Ejecuta un parpadeo.
        /// </summary>
        /// <param name="expresion">Expresión de parpadeo.</param>
        private IEnumerator Blink(Expresiones expresion)
        {
            SetBlendShape(expresion, 100f);
            yield return new WaitForSeconds(0.2f);
            SetBlendShape(expresion, 0f);
        }

        /// <summary>
        /// Establece el peso de una forma de mezcla en el renderizador.
        /// </summary>
        /// <param name="expresion">Expresión de la forma de mezcla.</param>
        /// <param name="value">Valor del peso.</param>
        public void SetBlendShape(Expresiones expresion, float value)
        {
            if (_meshRenderer != null)
            {
                _meshRenderer.SetBlendShapeWeight((int)expresion, value);
            }
        }

        /// <summary>
        /// Cambia la expresión del estudiante suavemente.
        /// </summary>
        /// <param name="exp">Expresión a cambiar.</param>
        private IEnumerator ChangeExpression(Expresiones exp)
        {
            int expressionIndex = (int)exp;
            while (_meshRenderer.GetBlendShapeWeight(expressionIndex) < 100f)
            {
                for (int i = 0; i < _blendShapeWeights.Length; i++)
                {
                    float changeValue = i == expressionIndex ? 15f : -20f;
                    _blendShapeWeights[i] = Mathf.Clamp(_blendShapeWeights[i] + changeValue, 0f, 100f);
                    _meshRenderer.SetBlendShapeWeight(i, _blendShapeWeights[i]);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}

using BehaviorDesigner.Runtime.Tasks.Emo;
using MathNet.Numerics.Statistics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        [SerializeField] private float _decisionTime = 10.0f;
        public float DecisionTime => _decisionTime;

        private Student _student;
        private Transform _player;
        private RunningStatistics _statistics;

        private Dictionary<studentBehaviorParams, float> studentBehavoirValues;
        [SerializeField] private string studentBehavoirJsonPath;

        private void Start()
        {
            _statistics = new RunningStatistics();
            _student = GetComponent<Student>();
            _player = Camera.main.transform;
            InvokeRepeating(nameof(MakeDecision), _decisionTime, _decisionTime);
        }

        private void LoadStudentBehavoirValues()
        {
            if (LoadManager.Instance.GetObject("studentBehavoir", ref studentBehavoirValues))
            {
                Debug.Log("Behavior values loaded successfully.");
                return;
            }
            string path=System.IO.Path.Combine(Application.persistentDataPath, studentBehavoirJsonPath);
            Dictionary<string, float> tempImpacts = LoadManager.Instance.LoadDataFromJson<string,float>(path);
            if (tempImpacts == null) return;
            // Convertir claves a enumeradores
            studentBehavoirValues = LoadManager.Instance.ConvertDictionary<studentBehaviorParams,float>(tempImpacts);
            LoadManager.Instance.SaveObject("studentBehavoir", studentBehavoirValues);
            Debug.Log("Behavior values loaded successfully.");
        }

        public void InitializeAttention(Personality personality)
        {
            LoadStudentBehavoirValues();

            float _attentionAddition = studentBehavoirValues[studentBehaviorParams.attentionAddition];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Extraversion) * _attentionAddition * studentBehavoirValues[studentBehaviorParams.extraversionInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Conscientiousness) * _attentionAddition * studentBehavoirValues[studentBehaviorParams.conscientiousnessInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Agreeableness) * _attentionAddition * studentBehavoirValues[studentBehaviorParams.agreeablenessInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Neuroticism) * _attentionAddition * studentBehavoirValues[studentBehaviorParams.neuroticismInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Openness) * _attentionAddition * studentBehavoirValues[studentBehaviorParams.opennessInfluence];

            _attentionLevel += Random.Range(-1.0f, 1.0f) * studentBehavoirValues[studentBehaviorParams.range];
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
                _attentionLevel += studentBehavoirValues[studentBehaviorParams.attentionAddition] * studentBehavoirValues[studentBehaviorParams.distanceFactorAddition] * (1 - factor);
            }
            else
            {
                _attentionLevel -= studentBehavoirValues[studentBehaviorParams.attentionSubtraction] * studentBehavoirValues[studentBehaviorParams.distanceFactorSubtraction] * (1 + factor);
            }

            _attentionLevel += ClimateManager.Instance.environmentalClimate * studentBehavoirValues[studentBehaviorParams.climateInfluence];

            _attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
        }

        public void ExternalForceInfluence(float ef)
        {
            //_attentionLevel += ClimateManager.Instance.environmentalClimate * studentBehavoirValues[studentBehaviorParams.climateInfluence];
            _attentionLevel += ef*(AttentionMax-AttentionMin);
            _attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
        }

        /// <summary>
        /// Establece un nivel mínimo de atención y cambia la expresión.
        /// </summary>
        public void SetAttention()
        {
          //  _attentionLevel = Mathf.Max(_attentionLevel, 65f);
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
        /// <summary>
        /// Cambia la expresión del estudiante suavemente.
        /// </summary>
        /// <param name="exp">Expresión a cambiar.</param>
        public IEnumerator ChangeExpression(Expresiones2 exp)
        {
            //int expressionIndex = (int)exp;
            //while (_meshRenderer.GetBlendShapeWeight(expressionIndex) < 100f)
            //{
            //    for (int i = 0; i < _blendShapeWeights.Length; i++)
            //    {
            //        float changeValue = i == expressionIndex ? 15f : -20f;
            //        _blendShapeWeights[i] = Mathf.Clamp(_blendShapeWeights[i] + changeValue, 0f, 100f);
            //        _meshRenderer.SetBlendShapeWeight(i, _blendShapeWeights[i]);
            //    }
            // El valor de esto antes era 0.5f. El cambio de expresiones se hace un tanto lento
            yield return new WaitForSeconds(0.125f);
            //}
            //_currentExpression = exp;
        }
    }
    
}

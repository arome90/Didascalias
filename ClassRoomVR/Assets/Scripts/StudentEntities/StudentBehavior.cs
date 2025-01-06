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

        [SerializeField] private float _decisionTime = 4f;
        public float DecisionTime => _decisionTime;

        //[SerializeField] private float _attentionAddition = 0f;
        //[SerializeField] private float _attentionSubtraction = 2f;
        //[SerializeField] private float _distanceFactorAddition = 2.1f;
        //[SerializeField] private float _distanceFactorSubtraction = 2.0f;
        
        //[SerializeField] private float _extraversionInfluence = 0f;
        //[SerializeField] private float _agreeablenessInfluence = 0.4f;
        //[SerializeField] private float _conscientiousnessInfluence = 1f;
        //[SerializeField] private float _neuroticismInfluence = 0.2f;
        //[SerializeField] private float _opennessInfluence = 0f;
        //[SerializeField] private float range = 20f;

        private Student _student;
        private Transform _player;
        private RunningStatistics _statistics;

        private Dictionary<studentBehaviorPrams, float> studentBehavoirValues;
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
            string filePath = Path.Combine(Application.dataPath, studentBehavoirJsonPath);

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                // Deserializar el JSON en un diccionario
                Dictionary<string, float> tempValues = JsonUtility.FromJson<KeyValueWrapper>(json).ToDictionary();

                // Convertir las claves a enumeradores
                studentBehavoirValues = new Dictionary<studentBehaviorPrams, float>();

                foreach (var kvp in tempValues)
                {
                    if (System.Enum.TryParse(kvp.Key, out studentBehaviorPrams param))
                    {
                        studentBehavoirValues[param] = kvp.Value;
                    }
                }

                Debug.Log("Behavior values loaded successfully.");
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }

        public void InitializeAttention(Personality personality)
        {
            LoadStudentBehavoirValues();

            float _attentionAddition = studentBehavoirValues[studentBehaviorPrams.attentionAddition];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Extraversion) * _attentionAddition * studentBehavoirValues[studentBehaviorPrams.extraversionInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Conscientiousness) * _attentionAddition * studentBehavoirValues[studentBehaviorPrams.conscientiousnessInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Agreeableness) * _attentionAddition * studentBehavoirValues[studentBehaviorPrams.agreeablenessInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Neuroticism) * _attentionAddition * studentBehavoirValues[studentBehaviorPrams.neuroticismInfluence];
            _attentionLevel += personality.GetTraitValue(PersonalityType.Openness) * _attentionAddition * studentBehavoirValues[studentBehaviorPrams.opennessInfluence];

            _attentionLevel += Random.Range(-1.0f, 1.0f) * studentBehavoirValues[studentBehaviorPrams.range];
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
                _attentionLevel += studentBehavoirValues[studentBehaviorPrams.attentionAddition] * studentBehavoirValues[studentBehaviorPrams.distanceFactorAddition] * (1 - factor);
                //Debug.Log(_attentionAddition);
            }
            else
            {
                _attentionLevel -= studentBehavoirValues[studentBehaviorPrams.attentionSubtraction] * studentBehavoirValues[studentBehaviorPrams.distanceFactorSubtraction] * (1 + factor);
            }

            _attentionLevel += ClimateManager.Instance.environmentalClimate * studentBehavoirValues[studentBehaviorPrams.climateInfluence];

            _attentionLevel = Mathf.Clamp(_attentionLevel, AttentionMin, AttentionMax);
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

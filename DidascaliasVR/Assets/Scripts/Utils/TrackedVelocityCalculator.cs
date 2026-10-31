using System;
using UnityEngine;

namespace Didascalia
{
    /// <summary>
    /// Calcula la velocidad lineal de la cabeza y las manos por Transform
    /// y genera/envía muestras de PlayerData al GameDataManager.
    /// </summary>
    public class TrackedVelocityCalculator : MonoBehaviour
    {
        [Header("Referencias XR Transforms")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform leftHandTransform;
        [SerializeField] private Transform rightHandTransform;

        [Header("Frecuencia de Registro")]
        [Tooltip("Intervalo en segundos para enviar una nueva muestra de PlayerData.")]
        [SerializeField] private float sampleInterval = 1.0f;

        // Posiciones registradas en el cuadro anterior
        private Vector3 _prevHeadPos;
        private Vector3 _prevLeftHandPos;
        private Vector3 _prevRightHandPos;

        // Propiedades con las velocidades calculadas en m/s
        public float CurrentHeadVelocity { get; private set; }
        public float CurrentLeftHandVelocity { get; private set; }
        public float CurrentRightHandVelocity { get; private set; }

        private float _sampleTimer;

        void Start()
        {
            if (headTransform != null) _prevHeadPos = headTransform.position;
            if (leftHandTransform != null) _prevLeftHandPos = leftHandTransform.position;
            if (rightHandTransform != null) _prevRightHandPos = rightHandTransform.position;
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;

            // Calcular velocidades en cada frame
            CalculateVelocities(deltaTime);

            // Temporizador para generar la muestra de PlayerData
            _sampleTimer += deltaTime;
            if (_sampleTimer >= sampleInterval)
            {
                _sampleTimer = 0f;
                SendPlayerDataSample();
            }
        }

        private void CalculateVelocities(float deltaTime)
        {
            if (headTransform != null)
            {
                Vector3 currentHeadPos = headTransform.position;
                CurrentHeadVelocity = Vector3.Distance(currentHeadPos, _prevHeadPos) / deltaTime;
                _prevHeadPos = currentHeadPos;
            }

            if (leftHandTransform != null)
            {
                Vector3 currentLeftPos = leftHandTransform.position;
                CurrentLeftHandVelocity = Vector3.Distance(currentLeftPos, _prevLeftHandPos) / deltaTime;
                _prevLeftHandPos = currentLeftPos;
            }

            if (rightHandTransform != null)
            {
                Vector3 currentRightPos = rightHandTransform.position;
                CurrentRightHandVelocity = Vector3.Distance(currentRightPos, _prevRightHandPos) / deltaTime;
                _prevRightHandPos = currentRightPos;
            }
        }

        private void SendPlayerDataSample()
        {
            PlayerData dataSample = new PlayerData(
                CurrentHeadVelocity,
                CurrentLeftHandVelocity,
                CurrentRightHandVelocity
            );

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SendData(dataSample);
            }
        }
    }
}
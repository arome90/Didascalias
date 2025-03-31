using Meta.WitAi.TTS.Utilities;
using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR
{
    [RequireComponent(typeof(AudioSource))]
    public class JawMove2 : MonoBehaviour
    {
        [SerializeField] private Transform _jaw;
        [SerializeField] private float _sizeReduction = 500f;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _fastSmoothSpeed = 20f;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _timeUpdate = 0.4f;

        private AudioSource _audioSource;
        private ResponseStudent _response;
        private Vector3 _initialJawAngles;
        private float _targetZRotation;
        private float _lastMaxValue;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _response = GetComponent<ResponseStudent>();
            _initialJawAngles = _jaw.localRotation.eulerAngles;
        }

        /// <summary>
        /// Actualiza la rotación de la mandíbula basada en el espectro de audio.
        /// </summary>
        private void UpdateJaw()
        {
            float[] spectrum = new float[1024];
            _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

            int maxIndex = 0;
            float maxValue = 0f;
            for (int i = 0; i < spectrum.Length; i++)
            {
                if (spectrum[i] > maxValue)
                {
                    maxIndex = i;
                    maxValue = spectrum[i];
                }
            }

            float frequency = (maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length) / _sizeReduction;
            frequency = Mathf.Clamp(frequency, 0, 40);

            float valueDifference = Mathf.Abs(maxValue - _lastMaxValue);
            float currentSmoothSpeed = valueDifference > _sensitivity * _lastMaxValue ? _fastSmoothSpeed : _smoothSpeed;

            _targetZRotation = Mathf.Lerp(_targetZRotation, frequency * 10, currentSmoothSpeed * _timeUpdate);
            _targetZRotation = Mathf.Clamp(_targetZRotation, 0, 40);
            _jaw.localRotation = Quaternion.Euler(_initialJawAngles.x, _initialJawAngles.y, _initialJawAngles.z + _targetZRotation);

            _lastMaxValue = maxValue;
        }

        /// <summary>
        /// Coroutine que actualiza la rotación de la mandíbula mientras el audio está reproduciéndose o se está hablando.
        /// </summary>
        public IEnumerator OnCompleteSpeach()
        {
            while (_audioSource.isPlaying || _response.IsSpeaking)
            {
                UpdateJaw();
                yield return new WaitForSeconds(_timeUpdate);
            }
            _jaw.localRotation = Quaternion.Euler(_initialJawAngles.x, _initialJawAngles.y, _initialJawAngles.z);
        }
    }
}

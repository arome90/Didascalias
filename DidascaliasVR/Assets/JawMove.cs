using System.Collections;
using UnityEngine;

namespace ClassRoomVR
{
    [RequireComponent(typeof(AudioSource))]
    public class JawMove : MonoBehaviour
    {
        [SerializeField] private Transform _jaw;
        [SerializeField] private float _sizeReduction = 500f;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private float _fastSmoothSpeed = 20f;
        [SerializeField] private float _sensitivity = 1f;
        [SerializeField] private float _timeUpdate = 0.4f;

        private AudioSource _audioSource;
        private Vector3 _initialJawAngles;
        private float _targetZRotation;
        private float _lastMaxValue;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
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
            _jaw.localRotation = Quaternion.Euler(_initialJawAngles.x + _targetZRotation, _initialJawAngles.y, _initialJawAngles.z);

            _lastMaxValue = maxValue;
        }

        /// <summary>
        /// Coroutine que actualiza la rotación de la mandíbula mientras el audio está reproduciéndose o se está hablando.
        /// </summary>
        public IEnumerator OnCompleteSpeach()
        {
            while (_audioSource.isPlaying)
            {

                UpdateJaw();
                yield return new WaitForSeconds(_timeUpdate);
            }
            // Debug.Log("cERRAR");
            _jaw.localRotation = Quaternion.Euler(_initialJawAngles.x, _initialJawAngles.y, _initialJawAngles.z);
        }
    }
}

using UnityEngine;
using System;

namespace Didascalia
{
    public class ConversationalFlowAnalyzer : Singleton<ConversationalFlowAnalyzer>
    {
        [Header("Referencias")]
        [SerializeField] private MicrophoneInputHandler micHandler;

        private float _lastTurnEndTime = -1f;
        private float _currentTurnStartTime;
        private string _currentSpeaker;
        private bool _isTurnActive = false;
        private bool _wasCurrentTurnInterruption = false;
        private string _interruptedSpeaker = "";

        protected override void Awake()
        {
            _destroyOnLoad = true;
            base.Awake();
        }

        private void OnEnable()
        {
            if (micHandler == null)
                micHandler = GetComponent<MicrophoneInputHandler>();

            if (micHandler != null)
            {
                micHandler.OnSpeechStarted += OnSpeakerStarted;
                micHandler.OnSpeechEnded += OnSpeakerEnded;
            }
        }

        private void OnDisable()
        {
            if (micHandler != null)
            {
                micHandler.OnSpeechStarted -= OnSpeakerStarted;
                micHandler.OnSpeechEnded -= OnSpeakerEnded;
            }
        }

        private void OnUserStarted(string speakerId) => OnSpeakerStarted("Teacher");
        private void OnUserEnded(string speakerId) => OnSpeakerEnded("Teacher");

        /// <summary>
        /// Se invoca cuando cualquier participante (Teacher o un Student por su nombre) empieza a hablar.
        /// </summary>
        public void OnSpeakerStarted(string speakerId)
        {
            float now = Time.time;
            
            // Si ya había un turno activo de otro participante, es una interrupción
            if (_isTurnActive && _currentSpeaker != speakerId)
            {
                _wasCurrentTurnInterruption = true;
                _interruptedSpeaker = _currentSpeaker;

                Debug.LogWarning($"[TurnTaking] ¡INTERRUPCIÓN! {speakerId} cortó la palabra a {_interruptedSpeaker}.");
                
                // Finalizar el turno del interlocutor cortado
                OnSpeakerEnded(_currentSpeaker);
            }
            else
            {
                _wasCurrentTurnInterruption = false;
                _interruptedSpeaker = "";
            }

            float waitTime = 0f;
            if (!_wasCurrentTurnInterruption && _lastTurnEndTime > 0f)
            {
                waitTime = now - _lastTurnEndTime; // Wait Time
            }

            _currentSpeaker = speakerId;
            _currentTurnStartTime = now;
            _isTurnActive = true;

            Debug.Log($"[TurnTaking] {speakerId} inició voz. Wait Time: {waitTime:F2}s");
        }

        /// <summary>
        /// Se invoca cuando cualquier participante termina de hablar.
        /// </summary>
        public void OnSpeakerEnded(string speakerId)
        {
            if (!_isTurnActive || _currentSpeaker != speakerId) return;

            float now = Time.time;
            float duration = now - _currentTurnStartTime;
            float latency = (_lastTurnEndTime > 0f) ? (_currentTurnStartTime - _lastTurnEndTime) : 0f;

            _lastTurnEndTime = now;
            _isTurnActive = false;

            // Crear y enviar la métrica al GameDataManager
            TurnData turnRecord = new TurnData(
                speaker: speakerId,
                duration: (float)Math.Round(duration, 2),
                latency: (float)Math.Round(latency, 2),
                wasInterruption: _wasCurrentTurnInterruption
            );

            if (GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SendData(turnRecord);
            }
        }
    }
}
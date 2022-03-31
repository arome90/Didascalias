using System.Collections;
using System.Collections.Generic;
using EmteqLabs.MaskProtocol;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmteqLabs
{
    public class ContactPrompt : MonoBehaviour
    {
        private const float _fitStateBufferInSeconds = 5;
        [SerializeField]
        private FitState _fitStateThreshold = FitState.Average;
        [SerializeField]
        private float _hidePromptIfStableForSeconds = 3;
        [SerializeField]
        private string _promptMessage;
        private EmteqVRMaskGUI _emteqVRMaskGUI;
        
        private float _currentCountdownValue;
        private bool _isEvaluating = false;
        private FitState _currentFitState = FitState.None;
        private bool _isBuffering = false;
        private bool _startContactBuffer = false;
        private bool _initialised = false;

        void Start()
        {
            _initialised = true;
            _emteqVRMaskGUI = GetComponentInChildren <EmteqVRMaskGUI>();
            EmteqVRManager.Instance.OnDeviceFitStateChange += OnFitStateChange;
            _emteqVRMaskGUI.SetInstructions(_promptMessage);
        }
        
        private void Update()
        {
            EvaluateFitStateStability();
            if (_startContactBuffer)
            {
                _startContactBuffer = false;
                StartCoroutine("FitStateBufferTimer");
            }
        }

        private void OnDestroy()
        {
            if (_initialised == true)
            {
                EmteqVRManager.Instance.OnDeviceFitStateChange -= OnFitStateChange;
            }
        }

        private void OnFitStateChange(FitState fitState)
        {
            _currentFitState = fitState;
            
            if (!EmteqVRManager.ShowContactPrompt)
            {
                return;
            }
            
            Logger.LogMessage(string.Format("FitState: {0}", _currentFitState.ToString()), LogType.Log);
            
            //if fitstate falls below threshold, prompt VR User
            if (fitState < _fitStateThreshold)
            {
                if (!_isBuffering)
                {
                    _startContactBuffer = true;
                }
            }
        }

        private void StartEvaluatingStability()
        {
            if (!_emteqVRMaskGUI.IsActive())
            {
                _emteqVRMaskGUI.Show();
            }
            _currentCountdownValue = _hidePromptIfStableForSeconds;
            _isEvaluating = true;
        }

        private void EvaluateFitStateStability()
        {
            if (_isEvaluating)
            {
                // fitstate must stay stable for a number of seconds
                if (_currentFitState < _fitStateThreshold)
                {
                    StartEvaluatingStability();
                }
                else
                {
                    _currentCountdownValue -= Time.deltaTime;
                    if(_currentCountdownValue <= 0f)
                    {
                        _emteqVRMaskGUI.Hide();
                        _isEvaluating = false;
                    }
                }
            }
        }

        private IEnumerator FitStateBufferTimer()
        {
            _isBuffering = true;
            yield return new WaitForSeconds(_fitStateBufferInSeconds);
            if (_currentFitState < _fitStateThreshold)
            {
                //trigger only if not already evaluating
                if (!_isEvaluating)
                {
                    StartEvaluatingStability();
                }
            }
            _isBuffering = false;
        }

        public void DismissContactPrompt()
        {
            _emteqVRMaskGUI.Hide();
            StopEvaluatingFitStateStability();
            StopBufferingFitStateChange();
        }

        private void StopEvaluatingFitStateStability()
        {
            _isEvaluating = false;
        }
        
        private void StopBufferingFitStateChange()
        {
            StopAllCoroutines();
            _isBuffering = false;
        }

        public void DisableContactPrompt()
        {
            DismissContactPrompt();
            EmteqVRManager.ShowContactPrompt = false;
        }
    }
}
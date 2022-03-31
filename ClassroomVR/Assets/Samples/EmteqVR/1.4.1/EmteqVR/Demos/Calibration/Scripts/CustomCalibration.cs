using System.Collections;
using EmteqLabs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmteqLabs
{
    public class CustomCalibration : MonoBehaviour
    {
        // Subscribe to this to remove the panel once calibration is finished
        public delegate void OnExpressionCalibrationCompleteDelegate(EmgCalibrationData expressionCalibrationData);
        public event OnExpressionCalibrationCompleteDelegate OnExpressionCalibrationComplete;
        public bool UseRecordingButton = false;

        [SerializeField]
        private int _numberCalibrationSteps = 6;
        [SerializeField]
        private string[] _calibrationStepInstructions;
        [SerializeField]
        private string[] _calibrationStepNames;
        [SerializeField]
        private ExpressionAvatar[] _calibrationExpressions;
        [SerializeField]
        private int _recordingTime = 3;
        [SerializeField]
        private int _instructionsDisplayTime = 10;
        [SerializeField]
        private GameObject _rootCanvas;
        [SerializeField]
        private Button _recordingButton;
        [SerializeField]
        private TMP_Text _recordingCountdownTextfield;
        [SerializeField]
        private TMP_Text _instructionsCountdownTextfield;
        [SerializeField]
        private GameObject _instructionsCountdownParent;
        [SerializeField]
        private TMP_Text _calibrationStepInstructionsTextfield;
        [SerializeField]
        private TMP_Text _calibrationStepNameTextfield;
        [SerializeField]
        private GameObject _calibrationStepInstructionsPanel;
        [SerializeField]
        private Animator _progressUIAnimator;
        [SerializeField]
        private Slider _calibrationProgressBar;
        [SerializeField]
        private GameObject _calibrationCompletePanel;
        [SerializeField]
        private int _currentCalibrationStep;
        private EmgCalibrationData _emgCalibrationData;
        private string _currentCalibrationStepInstructions;
        private string _currentCalibrationStepName;
        [SerializeField]
        private int _currentCountdownTime = 0;
        void Start()
        {
            //Sync framerate to monitors refresh rate
            QualitySettings.vSyncCount = 1;

            Initialise();
            UpdateCalibrationStepText();
            ChangeExpressionAvatar();
            _recordingButton.gameObject.SetActive(UseRecordingButton);
            HandleRecording(true);
        }

        public void RecordExpression()
        {
            StartCoroutine("HandleCalibrationProgress");
            EnableCalibrationStepUI(false);
        }

        public void EndCurrentExpression()
        {
            EmteqVRManager.EndExpressionCalibration();
            MoveToNextCalibrationStep();
            UpdateCalibrationProgressBar((float)_currentCalibrationStep / (_numberCalibrationSteps));
        }

        public void EndCalibration()
        {
            EmgCalibrationData data = EmteqVRManager.EndExpressionCalibration();
            Logger.LogMessage($"Calibration Complete: {data.ToString()}");

            UpdateCalibrationProgressBar(1);
            AnimateCalibrationStepUI("Complete");
            OnExpressionCalibrationComplete?.Invoke(data);
        }

        private void HandleRecording(bool displayButton)
        {
            if (UseRecordingButton)
            {
                _recordingButton.gameObject.SetActive(displayButton);
            }
            else
            {
                _instructionsCountdownParent.SetActive(displayButton);
                if (displayButton)
                {
                    StartCoroutine(HandleCalibrationInstructionsTimer());
                }
            }
        }

        private IEnumerator HandleCalibrationInstructionsTimer()
        {
            InitialiseInstructionsCountdownTime();

            for (int i = 0; i < _instructionsDisplayTime; i++)
            {
                DecreaseInstructionsCountdownTimer();
                yield return new WaitForSeconds(1f);

                if (_currentCountdownTime == 0)
                {
                    ResetInstructionsCountdownTimer();
                    RecordExpression();
                }
            }
        }

        private void ResetInstructionsCountdownTimer()
        {
            _instructionsCountdownTextfield.text = "";
            _currentCountdownTime = _recordingTime;
        }

        private void DecreaseInstructionsCountdownTimer()
        {
            _instructionsCountdownTextfield.text = _currentCountdownTime.ToString();
            _currentCountdownTime--;
        }

        private void InitialiseInstructionsCountdownTime()
        {
            _currentCountdownTime = _instructionsDisplayTime;
        }

        private void Initialise()
        {
            if (_calibrationExpressions == null)
            {
                _calibrationExpressions = new ExpressionAvatar[_numberCalibrationSteps];
            }
            if (_calibrationStepInstructions == null)
            {
                _calibrationStepInstructions = new string[_numberCalibrationSteps];
            }
            if (_calibrationStepNames == null)
            {
                _calibrationStepNames = new string[_numberCalibrationSteps];
            }
            _rootCanvas.SetActive(true);
            _currentCalibrationStep = 0;
            _calibrationProgressBar.value = 0;
        }

        private void UpdateCalibrationStepText()
        {
            if (UseRecordingButton)
            {
                _currentCalibrationStepInstructions = _calibrationStepInstructions[_currentCalibrationStep] + "\nPress the RECORD button when you're ready.";
            }
            else
            {
                _currentCalibrationStepInstructions = _calibrationStepInstructions[_currentCalibrationStep];
            }
            _currentCalibrationStepName = _calibrationStepNames[_currentCalibrationStep];
            _calibrationStepInstructionsTextfield.text = _currentCalibrationStepInstructions;
            _calibrationStepNameTextfield.text = _currentCalibrationStepName;
        }

        private void ChangeExpressionAvatar()
        {
            ResetExpressionAvatars();
            EnableNextExpressionAvatar();
        }

        private void ResetExpressionAvatars()
        {
            foreach (ExpressionAvatar expressionAvatar in _calibrationExpressions)
            {
                expressionAvatar.gameObject.SetActive(false);
            }
        }

        private void EnableNextExpressionAvatar()
        {
            _calibrationExpressions[_currentCalibrationStep].gameObject.SetActive(true);
            _calibrationExpressions[_currentCalibrationStep].GetComponent<ExpressionAvatar>().ChangeExpression();
        }

        private IEnumerator HandleCalibrationProgress()
        {
            InitialiseCountdownTime();
            AnimateCalibrationStepUI("Recording");

            EmteqVRManager.StartExpressionCalibration(_calibrationExpressions[_currentCalibrationStep].SelectedExpression);

            for (int i = 0; i < _recordingTime; i++)
            {
                DecreaseRecordingCountdownTimer();
                yield return new WaitForSeconds(1f);

                if (_currentCountdownTime == 0)
                {
                    if (_currentCalibrationStep < _numberCalibrationSteps - 1)
                    {
                        EndCurrentExpression();
                        UpdateCalibrationStepText();
                        ResetRecordingCountdownTimer();
                        AnimateCalibrationStepUI("Finished");
                        EnableCalibrationStepUI(true);
                        ChangeExpressionAvatar();
                    }
                    else
                    {
                        EndCalibration();
                        EnableCalibrationCompletePanel();
                    }
                }
            }
        }

        private void DecreaseRecordingCountdownTimer()
        {
            _recordingCountdownTextfield.text = _currentCountdownTime.ToString();
            _currentCountdownTime--;
        }

        private void ResetRecordingCountdownTimer()
        {
            _recordingCountdownTextfield.text = "";
            _currentCountdownTime = _instructionsDisplayTime;
        }

        private void InitialiseCountdownTime()
        {
            _currentCountdownTime = _recordingTime;
        }

        private void EnableCalibrationStepUI(bool enabled)
        {
            _calibrationStepInstructionsPanel.SetActive(enabled);
            EnableInstructionsTransition(enabled);
        }

        private void AnimateCalibrationStepUI(string animationTrigger)
        {
            _progressUIAnimator.SetTrigger(animationTrigger);
        }
        private void EnableCalibrationCompletePanel()
        {
            _calibrationCompletePanel.SetActive(true);
        }

        private void UpdateCalibrationProgressBar(float progress)
        {
            _calibrationProgressBar.value = progress;
        }

        private void MoveToNextCalibrationStep()
        {
            if (_currentCalibrationStep < _numberCalibrationSteps - 1)
            {
                _currentCalibrationStep++;
            }
        }
        private void EnableInstructionsTransition(bool enabled)
        {
            if (UseRecordingButton)
            {
                HandleRecording(enabled);
            }
            else if (enabled)
            {
                StartCoroutine(HandleCalibrationInstructionsTimer());
            }
        }
    }
}

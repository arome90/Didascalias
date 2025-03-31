using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using Meta.WitAi.TTS.Utilities;
using Utilities.Extensions;
using ClassRoomVR;
using Meta.WitAi.TTS.Data;
using MathNet.Numerics;
using UnityEngine.InputSystem;
using MathNet.Numerics.Distributions;
using NUnit.Framework.Constraints;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;

/// <summary>
/// Representa un paso en el tutorial.
/// </summary>
[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string StepText;  // Texto que describe el paso del tutorial
    public UnityEvent Action;  // Acción que debe ser ejecutada en este paso
    public bool ConditionMet;  // Verifica si la condición para este paso ha sido cumplida
    public float Actual;  // Progreso actual en el paso
    public int Objective;  // Objetivo que se debe cumplir en este paso
}

/// <summary>
/// Clase principal que gestiona el tutorial del juego.
/// </summary>
public class Tutorial : MonoBehaviour
{
    private List<Toggle> _tutorialToggles;  // Lista de toggles para el tutorial
    [SerializeField] private int _currentPhase = 0;  // Fase actual del tutorial
    [SerializeField] private Button _nextButton;  // Botón para avanzar al siguiente paso
    [SerializeField] private TextMeshProUGUI _tutorialText;  // Texto que muestra las instrucciones del tutorial
    [SerializeField] private TutorialStep[] _tutorialSteps;  // Array que contiene los pasos del tutorial
    [SerializeField] private Transform _player;  // Referencia al jugador
    [SerializeField] private Tuple<VisualController, VisualController> _controllers;  // Controladores para ambas manos
    [SerializeField] private Transform _destParent;  // Objeto destino que se activará en ciertos pasos
    [SerializeField] private Button _skipTutorial;  // Botón para saltarse el tutorial
    [SerializeField] private VisualController _handIzq;  // Controlador de la mano izquierda
    [SerializeField] private VisualController _handDer;  // Controlador de la mano derecha
    [SerializeField] private TTSSpeaker _speaker;  // Controlador de texto a voz
    [SerializeField] private string _dateId = "[DATE]";  // Identificador para el formato de fecha

    private Student2 _student;  // Estudiante que participa en el tutorial
    private StudentsController2 _studentControl;  // Controlador de los estudiantes

    bool _phaseSkipped = false;

    AudioSource explanationSrc = null;

    [Serializable]
    private class LanguageClips
    {
        public List<AudioClip> clips = new List<AudioClip>();
    }
    [SerializeField, UnityEngine.Tooltip("Please sort by the Language enum present in Didascalia_LocalizationManager")]
    List<LanguageClips> _clipsByLanguage = new List<LanguageClips>();
    // List<AudioClip> _ptClips = new List<AudioClip>();
    Dictionary<string, AudioClip> _actualLanguageClips = new Dictionary<string, AudioClip>();
    bool _playingExplanationAudio = false;

    /// <summary>
    /// Método que se ejecuta al iniciar el tutorial.
    /// </summary>
    private void Start()
    {
        ChangeLanguageClips();

        explanationSrc = GetComponent<AudioSource>();
        explanationSrc ??= gameObject.AddComponent<AudioSource>();
        explanationSrc.playOnAwake = false;

        InitializeTutorial();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnLanguageChanged.AddListener(ChangeLanguageClips);
    }

    private void OnDisable()
    {
        GameManager.Instance.OnLanguageChanged.RemoveListener(ChangeLanguageClips);
    }

    private void ChangeLanguageClips()
    {
        _actualLanguageClips.Clear();
        var clips = _clipsByLanguage[(int)Didascalia_LocalizationManager.CurrentLanguage].clips;
        foreach (AudioClip clip in clips)
        {
            _actualLanguageClips.Add(clip.name, clip);
        }
    }

    /// <summary>
    /// Inicializa los componentes del tutorial.
    /// </summary>
    private void InitializeTutorial()
    {
        SetDestinationParentInactive();
        InitializeToggles();
        InitializeFirstStep();
        InitializeButtons();
    }

    /// <summary>
    /// Desactiva el objeto destino y cambia su color a rojo.
    /// </summary>
    private void SetDestinationParentInactive()
    {
        foreach (Transform dest in _destParent)
        {
            dest.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        _destParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Inicializa los toggles del tutorial.
    /// </summary>
    private void InitializeToggles()
    {
        _tutorialToggles = new List<Toggle>();
        foreach (Transform t in transform)
        {
            Toggle toggle = t.GetComponent<Toggle>();
            _tutorialToggles.Add(toggle);
            toggle.isOn = t.GetSiblingIndex() < _currentPhase;
            toggle.interactable = t.GetSiblingIndex() <= _currentPhase;
        }
    }

    /// <summary>
    /// Inicializa el primer paso del tutorial con un mensaje de bienvenida.
    /// </summary>
    private void InitializeFirstStep()
    {
        string initText = "initialTextTTS";
        ModifyTextToSpeech(initText, true);
        FirstStep();
    }

    /// <summary>
    /// Inicializa los botones del tutorial.
    /// </summary>
    private void InitializeButtons()
    {
        _nextButton.onClick.AddListener(NextStep);
        UpdateToggles();
    }

    /// <summary>
    /// Actualiza los toggles dependiendo de la fase actual.
    /// </summary>
    private void UpdateToggles()
    {
        for (int i = 0; i < _tutorialToggles.Count; i++)
        {
            _tutorialToggles[i].isOn = i <= _currentPhase;
            _tutorialToggles[i].interactable = i <= _currentPhase;
        }
    }
    bool finish = false;
    private void Finish()
    {
        if (finish) return;
        finish = true;
        _handIzq.SetRed(VisualAction.Activate);
        _handDer.SetRed(VisualAction.Activate);
        _handIzq.transform.parent.GetChild(3).SetActive(true);
        _handDer.transform.parent.GetChild(2).SetActive(true);
        _tutorialSteps[_currentPhase].ConditionMet = true;
        _speaker.Events.OnPlaybackComplete.RemoveListener(Finish);
        Debug.Log("FINISHED TTS");
        UpdateTutorial();
    }

    /// <summary>
    /// Finaliza la fase actual del tutorial y actualiza el estado.
    /// </summary>
    private void Finish(TTSSpeaker speaker, TTSClipData data)
    {
        Finish();
    }

    /// <summary>
    /// Actualiza el estado del tutorial asincrónicamente.
    /// </summary>
    private async void UpdateTutorial()
    {
        Debug.Log("UPDATING TUTORIAL");
        UpdateToggles();
        if (_currentPhase < _tutorialSteps.Length)
        {
            string key = _tutorialSteps[_currentPhase].StepText;
            ModifyTextToSpeech(key, true);
            // _tutorialText.text = TranslatedText(key);
            _nextButton.interactable = false;
            await CurrentState();
            if (_phaseSkipped) { 
                _nextButton.onClick.Invoke();
                _phaseSkipped = false;
            } 
            else _nextButton.interactable = true;
        }
    }

    /// <summary>
    /// Verifica el estado actual del paso del tutorial.
    /// </summary>
    private async System.Threading.Tasks.Task CurrentState()
    {
        while (!_tutorialSteps[_currentPhase].ConditionMet)
        {
            _tutorialSteps[_currentPhase].Action.Invoke();
            await System.Threading.Tasks.Task.Delay(10);
        }
        if (_currentPhase == _tutorialSteps.Length - 1)
        {
            ModifyTextToSpeech("winTTS", true);
            Invoke(nameof(GoMenu), 3.5f);
        }
        else if (_currentPhase != 0)
        {
            TextToSpeech(GetNextText());
        }
    }

    private string[] _nextText = { "tutorialStepCompleted_TTS_0", "tutorialStepCompleted_TTS_1" };

    /// <summary>
    /// Devuelve un mensaje aleatorio para continuar con el siguiente paso.
    /// </summary>
    private string GetNextText()
    {
        return _nextText[UnityEngine.Random.Range(0, _nextText.Length)];
    }

    public void Skip()
    {
        if (_currentPhase == 0 && !finish)
        {
            Finish();
        }
        else {
            if(_currentPhase == _tutorialSteps.Length - 1)
            {
                Debug.Log("Finalmente");
            }
            if(_nextButton.interactable == false)
            {
                _tutorialSteps[_currentPhase].ConditionMet = true;
                _phaseSkipped = true;
            }
            else
            {
                _nextButton.onClick.Invoke();
            }

            // _nextButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// Avanza al siguiente paso del tutorial.
    /// </summary>
    public void NextStep()
    {
        CleanHandActions();
        _currentPhase++;
        UpdateTutorial();
        if (_currentPhase == _tutorialSteps.Length - 1)
        {
            _skipTutorial.onClick.RemoveAllListeners();
            _skipTutorial.onClick.AddListener(GameManager.Instance.LoadMainMenu);
            _nextButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Limpia las acciones asignadas a las manos.
    /// </summary>
    private void CleanHandActions()
    {
        _handIzq.CleanRed(VisualAction.Activate);
        _handDer.CleanRed(VisualAction.Activate);
    }

    /// <summary>
    /// Redirige al menú principal.
    /// </summary>
    private void GoMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }

    /// <summary>
    /// Ejecuta el primer paso del tutorial.
    /// </summary>
    public void FirstStep()
    {
        StartCoroutine(EndFirstStep());
        //_speaker.Events.OnPlaybackComplete.AddListener(Finish);
    }

    IEnumerator EndFirstStep()
    {
        while (_currentPhase == 0 && _playingExplanationAudio) yield return new WaitForEndOfFrame();
        Finish();
    }

    /// <summary>
    /// Verifica el movimiento del jugador en el tutorial.
    /// </summary>
    public void Movement()
    {
        if (_tutorialSteps[_currentPhase].Objective > 0)
        {
            _tutorialSteps[_currentPhase].Actual += Mathf.Max(_handIzq.ThumbStickVector().y, _handDer.ThumbStickVector().y);
            if (_tutorialSteps[_currentPhase].Actual > _tutorialSteps[_currentPhase].Objective)
            {
                _tutorialSteps[_currentPhase].Objective *= -1;
                _tutorialSteps[_currentPhase].Actual = 0;
                ModifyTextToSpeech("moveBackTTS", false);
            }
        }
        else if (_tutorialSteps[_currentPhase].Objective < 0)
        {
            _tutorialSteps[_currentPhase].Actual += Mathf.Min(_handIzq.ThumbStickVector().y, _handDer.ThumbStickVector().y);
            if (_tutorialSteps[_currentPhase].Actual < _tutorialSteps[_currentPhase].Objective)
            {
                _tutorialSteps[_currentPhase].ConditionMet = true;
            }
        }
    }

    /// <summary>
    /// Verifica si el jugador ha alcanzado el destino en el tutorial.
    /// </summary>
    public void Destino()
    {
        _destParent.SetActive(true);
        if (_destParent.childCount == 0)
        {
            _tutorialSteps[_currentPhase].ConditionMet = true;
        }
    }

    /// <summary>
    /// Ejecuta las acciones de botones en el tutorial.
    /// </summary>
    public void Botones()
    {
        if (_tutorialSteps[_currentPhase].Actual == 0)
        {
            _handDer.InputActions[(int)VisualAction.PrimaryButton].action.performed += Action_performed;
            _handDer.SetRed(VisualAction.PrimaryButton);
            Invoke(nameof(Generate), 12);
            _tutorialSteps[_currentPhase].Actual = 1;
            _tutorialSteps[_currentPhase].Objective = 0;
        }
        else if (_tutorialSteps[_currentPhase].Actual == 2 && _tutorialSteps[_currentPhase].Objective == 1)
        {
            _tutorialSteps[_currentPhase].Objective = 0;
            _handDer.SetRed(VisualAction.PrimaryButton);
            ModifyTextToSpeech("cantMoveTTS", true);
        }
        else if (_tutorialSteps[_currentPhase].Actual == 3 && _tutorialSteps[_currentPhase].Objective == 1)
        {
            _tutorialSteps[_currentPhase].Objective = 0;
            _handDer.InputActions[(int)VisualAction.PrimaryButton].action.performed -= Action_performed;
            _handIzq.InputActions[(int)VisualAction.Menu].action.performed += Action_performed;
            _handIzq.SetRed(VisualAction.Menu);
            ModifyTextToSpeech("menuActivationTTS", true);
        }
        else if (_tutorialSteps[_currentPhase].Actual == 4 && _tutorialSteps[_currentPhase].Objective == 1)
        {
            _tutorialSteps[_currentPhase].Objective = 0;
            ModifyTextToSpeech("closeMenuTTS", true);
        }
        else if ((_tutorialSteps[_currentPhase].Actual == 5 && _tutorialSteps[_currentPhase].Objective == 1))
        {
            _handIzq.InputActions[(int)VisualAction.Menu].action.performed -= Action_performed;
            _tutorialSteps[_currentPhase].ConditionMet = true;
        }

    }

    public void Ordenes()
    {

        switch (_tutorialSteps[_currentPhase].Actual)
        {
            case 0:
                ClassManager.Instance.Generate();
                GameManager.Instance.GetVoiceActivation().ActiveText(true);
                GameManager.Instance.GetVoiceActivation().Activate();
                _student = ClassManager.Instance.GetStudents()[0];
                _studentControl = ClassManager.Instance.GetStudentsController();
                _tutorialSteps[_currentPhase].Actual = 1;
                break;
            case 1:
                if (!_student.GetNavMeshAgent().enabled && Vector2.Distance(_student.transform.position, _studentControl.Door.position) < 0.2)
                {
                    ModifyTextToSpeech("studentSitTTS", true);
                    _tutorialSteps[_currentPhase].Actual = 2;
                }
                break;
            case 2:
                if (_student.GetState() == State.Sitting)
                {
                    GameManager.Instance.GetVoiceActivation().ActiveText(false);
                    _tutorialSteps[_currentPhase].ConditionMet = true;

                }
                break;
        }
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        if (_tutorialSteps[_currentPhase].Objective == 0)
        {
            _tutorialSteps[_currentPhase].Actual++;
            _tutorialSteps[_currentPhase].Objective = 1;
        }
    }

    /// <summary>
    /// Genera texto de audio.
    /// </summary>
    public void ModifyTextToSpeech(string key, bool cleanTutorialText)
    {
        string text = TextToSpeech(key);
        if(cleanTutorialText)
        {
            _tutorialText.text = text;
        }
        else _tutorialText.text += '\n'+text;
    }

    public string TextToSpeech(string key)
    {
        string translation = TranslatedText(key);
        Didascalia_LocalizationManager.Languages language = Didascalia_LocalizationManager.CurrentLanguage;
        AudioClip clip = _actualLanguageClips[key];
        if (explanationSrc.clip != clip)
        {
            StopCoroutine(PlayAudio(explanationSrc.clip));
            explanationSrc.clip = clip;
            StartCoroutine(PlayAudio(explanationSrc.clip));
        }
        //_speaker.Speak(FormatText(translation));
        return translation;
    }

    IEnumerator PlayAudio(AudioClip clip)
    {
        _playingExplanationAudio = true;
        explanationSrc.Play();
        yield return new WaitForSeconds(clip.length);
        if(explanationSrc.clip == clip) explanationSrc.Stop();
        _playingExplanationAudio = false;
    }

    /// <summary>
    /// Formatea el texto con la fecha actual si es necesario.
    /// </summary>
    private string FormatText(string text)
    {
        string result = text;
        if (result.Contains(_dateId))
        {
            DateTime now = DateTime.Now;
            string dateString = $"{now.ToLongDateString()} at {now.ToLongTimeString()}";
            result = text.Replace(_dateId, dateString);
        }
        return result;
    }

    private string TranslatedText(string key)
    {
        string translation;
        Didascalia_LocalizationManager.Instance.GetTranslation(key,
            Didascalia_LocalizationManager.TableCollections.TUTORIAL, out translation);
        return translation;
    }

}

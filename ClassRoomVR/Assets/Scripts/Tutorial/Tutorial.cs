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

    private Student _student;  // Estudiante que participa en el tutorial
    private StudentsController _studentControl;  // Controlador de los estudiantes

    /// <summary>
    /// Método que se ejecuta al iniciar el tutorial.
    /// </summary>
    private void Start()
    {
        InitializeTutorial();
        // _skipTutorial.onClick.AddListener(GoMenu);
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
            t.GetChild(0).GetComponent<TextMeshProUGUI>().text = t.name;
            toggle.isOn = t.GetSiblingIndex() < _currentPhase;
            toggle.interactable = t.GetSiblingIndex() <= _currentPhase;
        }
    }

    /// <summary>
    /// Inicializa el primer paso del tutorial con un mensaje de bienvenida.
    /// </summary>
    private void InitializeFirstStep()
    {
        string initText = "¡Bienvenido al Tutorial para Classroom VR! Aquí te " +
                          "enseñaremos todo lo que necesitas saber para " +
                          "ser un profesor excepcional en nuestro aula virtual.";
        ModifyTextToSpeech(initText, true);
        Invoke(nameof(FirstStep), 1);
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

    /// <summary>
    /// Finaliza la fase actual del tutorial y actualiza el estado.
    /// </summary>
    private void Finish(TTSSpeaker speaker, TTSClipData data)
    {
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
    /// Actualiza el estado del tutorial asincrónicamente.
    /// </summary>
    private async void UpdateTutorial()
    {
        Debug.Log("UPDATING TUTORIAL");
        UpdateToggles();
        if (_currentPhase < _tutorialSteps.Length)
        {
            ModifyTextToSpeech(_tutorialSteps[_currentPhase].StepText, true);
            _tutorialText.text = _tutorialSteps[_currentPhase].StepText;
            _nextButton.interactable = false;
            await CurrentState();
            _nextButton.interactable = true;
        }
    }

    /// <summary>
    /// Verifica el estado actual del paso del tutorial.
    /// </summary>
    private async Task CurrentState()
    {
        while (!_tutorialSteps[_currentPhase].ConditionMet)
        {
            _tutorialSteps[_currentPhase].Action.Invoke();
            await Task.Delay(1000);
        }
        if (_currentPhase == _tutorialSteps.Length - 1)
        {
            ModifyTextToSpeech("Has superado el tutorial.", true);
            Invoke(nameof(GoMenu), 1.5f);
        }
        else if (_currentPhase != 0)
        {
            TextToSpeech(GetNextText());
        }
    }

    private string[] _nextText = { "Genial. Vamos con el siguiente paso.", "Ahora vamos con el siguiente paso." };

    /// <summary>
    /// Devuelve un mensaje aleatorio para continuar con el siguiente paso.
    /// </summary>
    private string GetNextText()
    {
        return _nextText[UnityEngine.Random.Range(0, _nextText.Length)];
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
        _speaker.Events.OnPlaybackComplete.AddListener(Finish);
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
                ModifyTextToSpeech("Ahora haz lo mismo pero hacia atrás.", false);
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
            ModifyTextToSpeech("Observa que ya no puedes moverte, vuelve a pulsar el botón para volver a la normalidad", true);
        }
        else if (_tutorialSteps[_currentPhase].Actual == 3 && _tutorialSteps[_currentPhase].Objective == 1)
        {
            _tutorialSteps[_currentPhase].Objective = 0;
            _handDer.InputActions[(int)VisualAction.PrimaryButton].action.performed -= Action_performed;
            _handIzq.InputActions[(int)VisualAction.Menu].action.performed += Action_performed;
            _handIzq.SetRed(VisualAction.Menu);
            ModifyTextToSpeech("Activa el menu de mano pulsando sobre el botón 'Menú' del controlador izquierdo", true);
        }
        else if (_tutorialSteps[_currentPhase].Actual == 4 && _tutorialSteps[_currentPhase].Objective == 1)
        {
            _tutorialSteps[_currentPhase].Objective = 0;
            ModifyTextToSpeech("Sal del menú pulsando en 'Resume' o usando el boton menú", true);
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
                    ModifyTextToSpeech("Vuelve a mandar al alumno a sentarse", true);
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
    public void ModifyTextToSpeech(string text, bool cleanTutorialText)
    {
        TextToSpeech(text);
        if(cleanTutorialText)
        {
            _tutorialText.text = text;
        }
        else _tutorialText.text += '\n'+text;
    }

    public void TextToSpeech(string text)
    {
        _speaker.Speak(FormatText(text));
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
}

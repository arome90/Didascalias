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
        _skipTutorial.onClick.AddListener(GoMenu);
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
        GenerateText(initText);
        _tutorialText.text = initText;
        Invoke(nameof(FirstStep), 10);
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
        _tutorialSteps[_currentPhase].ConditionMet = true;
        _speaker.Events.OnPlaybackComplete.RemoveListener(Finish);
        UpdateTutorial();
    }

    /// <summary>
    /// Actualiza el estado del tutorial asincrónicamente.
    /// </summary>
    private async void UpdateTutorial()
    {
        UpdateToggles();
        if (_currentPhase < _tutorialSteps.Length)
        {
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
            GenerateText("Has superado el tutorial.");
            Invoke(nameof(GoMenu), 3f);
        }
        else if (_currentPhase != 0)
        {
            GenerateText(GetNextText());
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
        GenerateText(_tutorialSteps[_currentPhase].StepText);
        if (_currentPhase == _tutorialSteps.Length - 1)
        {
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
        GenerateText(_tutorialSteps[_currentPhase].StepText);
        _tutorialText.text = _tutorialSteps[_currentPhase].StepText;
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
                GenerateText("Ahora haz lo mismo pero hacia atrás.");
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
        // Lógica para acciones de botones...
    }

    /// <summary>
    /// Genera texto de audio.
    /// </summary>
    public void GenerateText(string text)
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

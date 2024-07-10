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
using UnityEngine.InputSystem;

[System.Serializable]
public class TutorialStep
{
    [TextArea]
    public string stepText;
    public UnityEvent action;
    public bool conditionMet;
    public float actual;
    public int objective;
}

public class Tutorial : MonoBehaviour
{

    private List<Toggle> tutorialToggles;
    public int currentPhase = 0;
    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private TextMeshProUGUI tutorialText;

    [SerializeField]
    private TutorialStep[] tutorialSteps;

    [SerializeField] Transform player;

    [SerializeField] Tuple<VisualController, VisualController> controllers;


    [SerializeField] Transform destParent;
    [SerializeField] Button skipTutorial;
    [SerializeField] VisualController handIzq;
    [SerializeField] VisualController handDer;
    private void Start()
    {

        foreach (Transform dest in destParent)
        {
            dest.GetComponent<MeshRenderer>().material.color = Color.red;
        }
        destParent.SetActive(false);
        tutorialToggles = new List<Toggle>();
        nextButton.onClick.AddListener(NextStep);
        foreach (Transform t in transform)
        {
            tutorialToggles.Add(t.GetComponent<Toggle>());
            t.GetChild(0).GetComponent<TextMeshProUGUI>().text = t.name;
        }
        string initText = "¡Bienvenido al Tutorial para Classroom VR! Aquí te" +
            " enseñaremos todo lo que necesitas saber para " +
            "ser un profesor excepcional en nuestro aula virtual";
        GenerateText(initText);
        tutorialText.text = initText;
        Invoke(nameof(FirstStep),10);
        skipTutorial.onClick.AddListener(GoMenu);
        for (int i = 0; i < tutorialToggles.Count; i++)
        {
            tutorialToggles[i].isOn = i < currentPhase;
            tutorialToggles[i].interactable = i <= currentPhase;
        }

    }

    private void Finish(TTSSpeaker s, TTSClipData data)
    {
        handIzq.SetRed(VisualAction.Activate);
        handDer.SetRed(VisualAction.Activate);
        tutorialSteps[currentPhase].conditionMet = true;
        _speaker.Events.OnPlaybackComplete.RemoveListener(Finish);
        UpdateTutorial();

    }
    private async void UpdateTutorial()
    {

        for (int i = 0; i < tutorialToggles.Count; i++)
        {
            tutorialToggles[i].isOn = i <= currentPhase;
            tutorialToggles[i].interactable = i <= currentPhase;
        }

        if (currentPhase < tutorialSteps.Length)
        {
            tutorialText.text = tutorialSteps[currentPhase].stepText;
            nextButton.interactable = false;
            await CurrentState();
            nextButton.interactable = true;

        }
    }
    private async Task CurrentState()
    {
        while (!tutorialSteps[currentPhase].conditionMet)
        {
            tutorialSteps[currentPhase].action.Invoke();
            // Espera un breve periodo de tiempo antes de verificar de nuevo
            await Task.Delay(1000);
        }
        if (currentPhase == tutorialSteps.Length - 1)
        {
            GenerateText("Has superado el tutorial");
            Invoke(nameof(GoMenu), 3f);
        }
        else if (currentPhase != 0 ) GenerateText(GetNextText());
      
    }
    string[] nextText = { "Genial. Vamos con el siguiente paso", "Ahora vamos con el siguiente paso"};
    string GetNextText() 
    {
        return nextText[UnityEngine.Random.Range(0, nextText.Length)];
    }


    public void NextStep()
    {
        handIzq.CleanRed(VisualAction.Activate);
        handDer.CleanRed(VisualAction.Activate);
        currentPhase++;
        UpdateTutorial();
        GenerateText(tutorialSteps[currentPhase].stepText);
        if (currentPhase == tutorialSteps.Length - 1)
        {
            nextButton.SetActive(false);
        }

    }

    void GoMenu() 
    {
        GameManager.Instance.LoadMainMenu();
    }
    public void FirstStep()
    {
        _speaker.Events.OnPlaybackComplete.AddListener(Finish);
        GenerateText(tutorialSteps[currentPhase].stepText);
        tutorialText.text = tutorialSteps[currentPhase].stepText;       
    }

    public void Movement()
    {
        if (tutorialSteps[currentPhase].objective > 0)
        {
            tutorialSteps[currentPhase].actual += Mathf.Max(handIzq.ThumbStickVector().y, handDer.ThumbStickVector().y);
            if (tutorialSteps[currentPhase].actual > tutorialSteps[currentPhase].objective)
            {
                tutorialSteps[currentPhase].objective *= -1;
                tutorialSteps[currentPhase].actual = 0;
                GenerateText("Ahora haz lo mismo pero hacia detrás");

            }
        }
        else if (tutorialSteps[currentPhase].objective < 0)
        {
            tutorialSteps[currentPhase].actual += Mathf.Min(handIzq.ThumbStickVector().y, handDer.ThumbStickVector().y);
            if (tutorialSteps[currentPhase].actual < tutorialSteps[currentPhase].objective)
            {
                tutorialSteps[currentPhase].conditionMet = true;
            }

        }
    }

    public void Destino()
    {
        destParent.SetActive(true);
        if (destParent.childCount==0)
        {
            tutorialSteps[currentPhase].conditionMet = true;
        }
    }


    public void Botones()
    {
        if (tutorialSteps[currentPhase].actual == 0)
        {
            handDer.InputActions[(int)VisualAction.PrimaryButton].action.performed += Action_performed;
            handDer.SetRed(VisualAction.PrimaryButton);
            Invoke(nameof(Generate), 12);
            tutorialSteps[currentPhase].actual = 1;
            tutorialSteps[currentPhase].objective = 0;
        }
        else if (tutorialSteps[currentPhase].actual== 2 && tutorialSteps[currentPhase].objective == 1)
        {
            tutorialSteps[currentPhase].objective = 0;
            handDer.SetRed(VisualAction.PrimaryButton);
            GenerateText("Observa que ya no puedes moverte, vuelve a pulsar el botón para volver a la normalidad");
        }
        else if (tutorialSteps[currentPhase].actual == 3 && tutorialSteps[currentPhase].objective == 1)
        {
            tutorialSteps[currentPhase].objective = 0;
            handDer.InputActions[(int)VisualAction.PrimaryButton].action.performed -= Action_performed;
            handIzq.InputActions[(int)VisualAction.Menu].action.performed += Action_performed;
            handIzq.SetRed(VisualAction.Menu);
            GenerateText("Activa el menu de mano pulsando sobre el menu");

        }
        else if (tutorialSteps[currentPhase].actual == 4 && tutorialSteps[currentPhase].objective == 1)
        {
            tutorialSteps[currentPhase].objective = 0;
            GenerateText("Sal del menú pulsando en resume o usa el boton menú");
        }
        else if ((!GameManager.Instance.IsPause && tutorialSteps[currentPhase].actual == 4) || (tutorialSteps[currentPhase].actual == 5 &&  tutorialSteps[currentPhase].objective == 1)) 
        {
            handIzq.InputActions[(int)VisualAction.Menu].action.performed -= Action_performed;
            tutorialSteps[currentPhase].conditionMet = true;
        }

    }
    void Generate() 
    {
        GenerateText("Pulsa el boton en rojo para activar el modo pensar");
    }
    private void Action_performed(InputAction.CallbackContext obj)
    {
        if (tutorialSteps[currentPhase].objective == 0)
        {
            tutorialSteps[currentPhase].actual++;
            tutorialSteps[currentPhase].objective = 1;
        }
    }

    Student student;
    StudentsController studentControl;
    public void Ordenes()
    {

        switch (tutorialSteps[currentPhase].actual)
        {
            case 0:
                ClassManager.Instance.Generate();
                GameManager.Instance.GetVoiceActivation().ActiveText(true);
                GameManager.Instance.GetVoiceActivation().Activate();
                student = ClassManager.Instance.GetStudents()[0];
                studentControl = ClassManager.Instance.GetStudentsController();
                tutorialSteps[currentPhase].actual = 1;
                break;
            case 1:
                if (!student.GetNavMeshAgent().enabled && Vector2.Distance(student.transform.position, studentControl.Door.position) < 0.2)
                {
                    GenerateText("Vuelve a mandar al alumno a sentarse");
                    tutorialSteps[currentPhase].actual = 2;
                }
                break;
            case 2:
                if (student.state == State.Sitting)
                {
                    GameManager.Instance.GetVoiceActivation().ActiveText(false);
                    tutorialSteps[currentPhase].conditionMet = true;

                }
                break;
        }

    }

    //public void SolucionarConflicto() 
    //{

    //}
 

    [SerializeField] TTSSpeaker _speaker;
    public void GenerateText(string text)
    {
        // Speak phrase
        string phrase = FormatText(text);
        // Speak async
        _speaker.Speak(phrase);
    }
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
    [SerializeField] private string _dateId = "[DATE]";

}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using Utilities.WebRequestRest;
using Meta.WitAi.TTS.Interfaces;
using Meta.WitAi.TTS.Utilities;
using static System.Net.Mime.MediaTypeNames;

[System.Serializable]
public class TutorialStep
{
    public string stepText;
    public UnityEvent action;
    public bool conditionMet;
    public float actual;
    public float objective;
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

    private void Start()
    {
        tutorialToggles = new List<Toggle>();
        nextButton.onClick.AddListener(NextStep);
        foreach (Transform t in transform) 
        {
            tutorialToggles.Add(t.GetComponent<Toggle>());
            t.GetChild(0).GetComponent<TextMeshProUGUI>().text = t.name;
        }
        GenerateText(tutorialSteps[currentPhase].stepText);
        FirstStep();
    }
    private async void UpdateTutorial()
    {

        for (int i = 0; i < tutorialToggles.Count; i++)
        {
            tutorialToggles[i].isOn = i < currentPhase;
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
        GetVariable();
        while (!tutorialSteps[currentPhase].conditionMet)
        {
            tutorialSteps[currentPhase].action.Invoke();
            // Espera un breve periodo de tiempo antes de verificar de nuevo
            await Task.Delay(1000);
        }
        GenerateText("Genial. Vamos con el siguiente paso");
    }

    private void GetVariable() 
    {
        
        switch(currentPhase) 
        {
            case 1: tutorialSteps[currentPhase].actual = player.position.z;
                break;
            case 2:
                tutorialSteps[currentPhase].actual = player.position.z;
                break;
            case 3:
                tutorialSteps[currentPhase].actual = player.localEulerAngles.y;
                break;
            case 4:
                tutorialSteps[currentPhase].actual = 0;
                break;
        }
    }
    public void NextStep()
    {
        if (currentPhase < tutorialToggles.Count)
        {
            currentPhase++;
            UpdateTutorial();
            GenerateText(tutorialSteps[currentPhase].stepText);

        }
        else
        {
            GenerateText("Has superado el tutorial");
        }
    
    }
    ContinuousTurnProviderBase b;
    public async void FirstStep()
    {
        tutorialText.text = tutorialSteps[currentPhase].stepText;
        // Esperar 5 segundos antes de marcar la condición como cumplida
        await Task.Delay(17000); 
        // Marcar la condición como cumplida después de esperar los 5 segundos
        tutorialSteps[currentPhase].conditionMet = true;
        UpdateTutorial();
    }
    private void Update()
    {
        //transform.parent.LookAt(player);
        //transform.parent.rotation = Quaternion.LookRotation(player.forward);
    }
    public void Giro()
    {
        //float diferenciaRotacion = Mathf.Abs(tutorialSteps[currentPhase].objective - tutorialSteps[currentPhase].actual);
        Debug.Log(Math.Abs(player.localEulerAngles.y - tutorialSteps[currentPhase].actual));
        // Verifica si la diferencia de rotación es aproximadamente igual a 360 grados
        if (Math.Abs(player.localEulerAngles.y - tutorialSteps[currentPhase].actual) >= tutorialSteps[currentPhase].objective)
        {
            Debug.Log("vuelta");
            // Marca la condición como cumplida si se ha dado una vuelta completa
            tutorialSteps[currentPhase].conditionMet = true;
        }
    }

    public void Front()
    {
        // Verifica si la diferencia de rotación es aproximadamente igual a 360 grados
        if (tutorialSteps[currentPhase].actual - player.position.z >= tutorialSteps[currentPhase].objective)
        {
            // Marca la condición como cumplida si se ha dado una vuelta completa
            tutorialSteps[currentPhase].conditionMet = true;
        }
    }

    public void Back()
    {
        

        // Verifica si la diferencia de rotación es aproximadamente igual a 360 grados
        if (tutorialSteps[currentPhase].actual - player.position.z <= tutorialSteps[currentPhase].objective)
        {
            // Marca la condición como cumplida si se ha dado una vuelta completa
            tutorialSteps[currentPhase].conditionMet = true;
        }
    }

   public void Destino() 
    {
        tutorialSteps[currentPhase].actual++;
        Debug.Log(tutorialSteps[currentPhase].actual);
        if (tutorialSteps[currentPhase].actual >= tutorialSteps[currentPhase].objective)
        {
            tutorialSteps[currentPhase].conditionMet = true;
        }
    }
    
    void MandarFuera()
    {

    }

    void SentarAlumno() 
    {

    }

    [SerializeField ]TTSSpeaker _speaker;

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



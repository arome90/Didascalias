using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TutorialStep
{
    public string stepText;
    public UnityEvent action;
    public bool conditionMet;
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

    private UnityEvent tutorialAction;

    private void Start()
    {
        tutorialToggles = new List<Toggle>();
        nextButton.onClick.AddListener(NextStep);
        foreach(Transform t in transform) 
        {
            tutorialToggles.Add(t.GetComponent<Toggle>());
            t.GetChild(0).GetComponent<TextMeshProUGUI>().text = t.name;
        }

        UpdateTutorial();
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
        while (!Input.GetKeyDown(KeyCode.I))
        {
            // Espera un breve periodo de tiempo antes de verificar de nuevo
            await Task.Delay(10);
        }

    }

    public void NextStep()
    {
        if (currentPhase < tutorialToggles.Count - 1)
        {
            currentPhase++;
            UpdateTutorial();
        }
    }

}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    
    public GameObject tutorialCanvas;
    public GameObject player;
    public GameObject teleportTarget;
    public GameObject blackboard;
    public GameObject students;

    private int currentPhase = 0;
    private bool teleportationActivated = false;
    private bool objectInteractionActivated = false;
    private bool drawingActivated = false;

    void Update()
    {
        // Lógica para avanzar al siguiente paso del tutorial cuando el usuario complete una fase
        if (teleportationActivated && objectInteractionActivated && drawingActivated)
        {
            currentPhase++;
            ActivatePhase(currentPhase);
        }
    }

    public void ActivatePhase(int phase)
    {
        // Método para activar la fase actual del tutorial
        switch (phase)
        {
            case 1:
                // Fase 1: Controles y Movimientos
                tutorialCanvas.SetActive(true);
                break;
            case 2:
                // Fase 2: Teletransporte
                teleportTarget.SetActive(true);
                break;
            case 3:
                // Fase 3: Interacción con Objetos
                tutorialCanvas.SetActive(false);
                teleportTarget.SetActive(false);
                objectInteractionActivated = true;
                break;
            case 4:
                // Fase 4: Dibujar con Tiza en la Pizarra
                blackboard.SetActive(true);
                drawingActivated = true;
                break;
            case 5:
                // Fase 5: Mantener la Disciplina en el Aula
                students.SetActive(true);
                break;
            case 6:
                // Fase 6: Resolver un Pequeño Conflicto
                // Implementa lógica para resolver el conflicto entre estudiantes
                break;
            case 7:
                // FIN: Tutorial completado
                EndTutorial();
                break;
        }
    }

    public void EndTutorial()
    {
        // Método llamado cuando el usuario ha completado el tutorial
        tutorialCanvas.SetActive(false);
        teleportTarget.SetActive(false);
        blackboard.SetActive(false);
        students.SetActive(false);
        // Puedes realizar acciones adicionales al completar el tutorial
    }

    // Implementa métodos adicionales para manejar las interacciones del usuario, como el teletransporte y la interacción con objetos.
}



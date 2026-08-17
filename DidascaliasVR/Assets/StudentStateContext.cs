using UnityEngine;

public class StudentStateContext : StateMachineBehaviour
{
    [Header("Información del Estado para el LLM")]
    public string stateName;
    [TextArea(2, 5)]
    public string stateDescription;

    // Se ejecuta automáticamente al entrar en esta animación
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Buscamos el gestor de contexto en el GameObject que tiene el Animator
        var contextManager = animator.GetComponentInParent<Student>();

        if (contextManager != null)
        {
            contextManager.SetStateContext(stateName, stateDescription);
        }
    }
}
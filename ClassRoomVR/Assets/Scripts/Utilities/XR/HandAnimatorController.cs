using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorController : MonoBehaviour
{
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimator(triggerAction.action.ReadValue<float>(), gripAction.action.ReadValue<float>());
    }

    private void UpdateAnimator(float triggerValue, float gripValue)
    {
        animator.SetFloat("Trigger", triggerValue);
        animator.SetFloat("Grip", gripValue);
    }
}

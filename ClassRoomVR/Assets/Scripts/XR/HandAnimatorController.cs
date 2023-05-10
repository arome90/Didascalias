using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimatorController : MonoBehaviour
{
    [SerializeField] InputActionProperty triggerAction;
    [SerializeField] InputActionProperty gripAction;

    private Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float trValue = triggerAction.action.ReadValue<float>();
        float grValue = gripAction.action.ReadValue<float>();

        anim.SetFloat("Trigger", trValue);
        anim.SetFloat("Grip", grValue);
    }
}

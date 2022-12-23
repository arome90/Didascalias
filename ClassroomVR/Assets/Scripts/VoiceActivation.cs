using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;

public class VoiceActivation : MonoBehaviour
{
    private Wit wit ;

    private void Start()
    {
        wit = GetComponent<Wit>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V)) 
        {
            wit.Activate();
        }
    }

}

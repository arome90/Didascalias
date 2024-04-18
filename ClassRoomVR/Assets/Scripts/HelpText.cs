using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpText : MonoBehaviour
{

    public string[] texts;
    TMPro.TextMeshProUGUI  text;
    bool activate;

    public void Disenable() 
    {
        if (!activate)
        {
            text.gameObject.SetActive(false);
        }
        else
        {
            text.gameObject.SetActive(true);
        }
    }

    public void ChangeScreen(int i ) 
    {
        text.text = texts[i];
    }

}

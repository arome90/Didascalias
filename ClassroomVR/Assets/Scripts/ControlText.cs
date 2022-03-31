using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlText : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text heartrateText;
    public TMP_Text expressionsText;
     

    public void ChangeRate(string heartRate) 
    {
        Debug.Log(heartRate);
        heartrateText.text = "Pulso : " + heartRate;
    }
    public void ChangeExpressions(string expression)
    {
        expressionsText.text = "Expression :" + expression;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CustomButtons : MonoBehaviour
{
    [SerializeField] private Button prevButton;
    public Button PrevButton => prevButton;

    [SerializeField] private Button nextButton;
    public Button NextButton => nextButton;

    
    private int value;
    
    public int GetValue()
    {
        return value;
    }

    public void SetValue(int v)
    {
        value = v;
    }
}

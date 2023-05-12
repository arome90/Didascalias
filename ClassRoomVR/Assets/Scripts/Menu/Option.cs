using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI optionName;
    [SerializeField] TextMeshProUGUI optionValue;
    [SerializeField] Button add;
    [SerializeField] Button sub;

    //Variables
    [SerializeField] float value;

    [SerializeField] float min;
    [SerializeField] float max;
    [SerializeField] float step;

    [HideInInspector]public UnityEvent<float> onValueChanged;


    // Start is called before the first frame update
    void Start()
    {
        optionValue.text = value.ToString("0.##");
        add.onClick.AddListener(Add);
        sub.onClick.AddListener(Sub);
        
    }

    void Add() 
    {
        value += step;
        if (value >max) value = max;
        else
        {
            optionValue.text = value.ToString("0.##");
            onValueChanged.Invoke(value);
        }
    }
    void Sub()
    {
        
        value -= step;
        if (value < min) value = min;
        else
        {
            optionValue.text = value.ToString("0.##");
            onValueChanged.Invoke(value);
        }
    }


    public void SetMax(float value) 
    {
        max = value;
    }


    public void SetMin(float value)
    {
        min = value;
    }



    public void SetValue(float v)
    {
        value = v;
        optionValue.text = value.ToString("0.##");
    }
}

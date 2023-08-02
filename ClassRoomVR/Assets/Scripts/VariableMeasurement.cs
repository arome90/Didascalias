using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class VariableMeasurementFloat: MonoBehaviour 
{
    private float variable;
    public float Variable { get => variable; set { variable = value; Measure(); } }

    private float maximum;
    private float minimum;

    private int count;

    private float variableAverage;

    public void Set()
    {
        maximum = int.MinValue;
        minimum = int.MaxValue;
        count = 1;
        list = new List<float>();
    }

    List<float> list;

    //public void SeeList()
    //{
    //    var a = list;
    //    Debug.Log(variableAverage);
    //    return;
    //}

    public float GetAverage()
    {
        return variableAverage;
    }

    public float GetMaximum()
    {
        return maximum;
    }

    public float GetMinimum()
    {
        return minimum;
    }

    void Measure() 
    {
        
        float sum = variableAverage * count + variable;
        count++;
        variableAverage = sum / count;
    }

}


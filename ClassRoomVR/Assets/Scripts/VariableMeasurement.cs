using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class VariableMeasurement : MonoBehaviour
{
    private float maximum;
    private float minimum;

    private int count;
    private float time;

    private float variableAverage;

    private float currenntVariable;


   public delegate float GetVariable(); // declare a delegate

   public GetVariable del;
    public void Set(float t)
    {
        time = t;
        maximum = int.MinValue;
        minimum = int.MaxValue;
        count = 1;
        StartCoroutine("MeasureVariable");
    }

    public float GetAverage()
    {
        return variableAverage;
    }

    public float GetMaximum()
    {
        return maximum;
    }

    public  float GetMinimum()
    {
        return minimum;
    }


    IEnumerator MeasureVariable()
    {
        variableAverage = 0;
        while (true)
        {
            yield return new WaitForSecondsRealtime(time);
            currenntVariable = del();
           // Debug.Log(currenntVariable+"A");
            float sum = variableAverage * count + currenntVariable;
            count++;
            variableAverage = sum / count;
            Debug.Log(variableAverage);
        }
    }


    //public void ValueChanged(float newValue)
    //{
    //    currenntVariable=newValue;
    //}

    //public float A
    //{
    //    get { return a; }
    //    set
    //    {
    //        a = value;
    //        // Trigger the event when the value changes
    //        OnValueChanged?.Invoke(a);
    //    }
    //}

}

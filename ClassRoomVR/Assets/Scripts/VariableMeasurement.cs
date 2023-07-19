using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//public class VariableMeasurement<T> : MonoBehaviour where T : struct, IComparable<T>
//{

//   

//    private T maximum;
//    private T minimum;

//    private int count;
//    private float time;

//    private T variableSum;
//    private T variableAverage;

//    private T currentVariable;

//    public delegate T GetVariable(); // Declarar un delegado

//    public GetVariable del;

//    public void Set(float t)
//    {
//        time = t;
//        maximum = default(T);
//        minimum = default(T);
//        count = 1;
//        variableSum = default(T);
//        variableAverage = default(T);
//        list = new System.Collections.Generic.List<T>();
//       // StartCoroutine(MeasureVariable());
//    }

//    public T GetAverage()
//    {

//        return variableAverage;
//    }

//    public T GetMaximum()
//    {
//        return maximum;
//    }

//    public T GetMinimum()
//    {
//        return minimum;
//    }

//    IEnumerator MeasureVariable()
//    {
//        while (true)
//        {
//            yield return new WaitForSecondsRealtime(time);
//            currentVariable = del();
//            list.Add(currentVariable);
//            // Actualizar máximo y mínimo
//            if (currentVariable.CompareTo(maximum) > 0)
//            {
//                maximum = currentVariable;
//            }
//            if (currentVariable.CompareTo(minimum) < 0)
//            {
//                minimum = currentVariable;
//            }

//            // Calcular suma y promedio
//            dynamic sum = Add(variableSum, currentVariable);
//            variableSum = sum;
//            variableAverage = Divide(variableSum, count);
//            count++;
//        }
//    }

//    private dynamic Add(dynamic a, dynamic b)
//    {
//        return a + b;
//    }

//    private dynamic Divide(dynamic a, int b)
//    {
//        return a / b;
//    }

//    System.Collections.Generic.List<T> list;

//    public void SeeList() 
//    {
//        var a = list ;
//        Debug.Log(variableAverage);
//        return;
//    }
//}

public class VariableMeasurementFloat : MonoBehaviour 
{
    private float maximum;
    private float minimum;

    private int count;
    private float time;

    private float variableAverage;

    private float currentVariable;


    public delegate float GetVariable(); // declare a delegate

    public GetVariable del;
    public void Set(float t)
    {
        time = t;
        maximum = int.MinValue;
        minimum = int.MaxValue;
        count = 1;
        list = new List<float>();
        StartCoroutine("MeasureVariable");
    }

    List<float> list;

    public void SeeList()
    {
        var a = list;
        Debug.Log(variableAverage);
        return;
    }

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


    IEnumerator MeasureVariable()
    {
        variableAverage = 0;
        while (true)
        {
            yield return new WaitForSecondsRealtime(time);
            currentVariable = del();
            float sum = variableAverage * count + currentVariable;
            count++;
            variableAverage = sum / count;
        }
    }


}

public class VariableMeasurementInt : MonoBehaviour
{
    private int maximum;
    private int minimum;

    private int count;
    private float time;

    private float variableAverage;

    private int currentVariable;

    public delegate int GetVariable(); // declare a delegate

    public GetVariable del;

    public void Set(float t)
    {
        time = t;
        maximum = int.MinValue;
        minimum = int.MaxValue;
        count = 1;
        StartCoroutine(MeasureVariable());
    }

    public float GetAverage()
    {
        return variableAverage;
    }

    public int GetMaximum()
    {
        return maximum;
    }

    public int GetMinimum()
    {
        return minimum;
    }

    IEnumerator MeasureVariable()
    {
        variableAverage = 0;
        while (true)
        {
            yield return new WaitForSecondsRealtime(time);
            currentVariable = del();
            float sum = variableAverage * count + currentVariable;
            count++;
            variableAverage = sum / count;
            Debug.Log(variableAverage);
        }
    }
}




public class VariableMeasurement<T>
{



    private T maximum;
    private T minimum;

    private int count;
    private float time;

    private T variableSum;
    private T variableAverage;

    private T currentVariable;

    public delegate T GetVariable(); // Declarar un delegado

    public GetVariable del;

    public void Set(float t)
    {
        time = t;
        maximum = default(T);
        minimum = default(T);
        count = 1;
        variableSum = default(T);
        variableAverage = default(T);
        list = new System.Collections.Generic.List<T>();
        // StartCoroutine(MeasureVariable());
    }

    public T GetAverage()
    {

        return variableAverage;
    }

    public T GetMaximum()
    {
        return maximum;
    }

    public T GetMinimum()
    {
        return minimum;
    }

    //IEnumerator MeasureVariable()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSecondsRealtime(time);
    //        currentVariable = del();
    //        list.Add(currentVariable);
    //        // Actualizar máximo y mínimo
    //        //if (currentVariable.CompareTo(maximum) > 0)
    //        //{
    //        //    maximum = currentVariable;
    //        //}
    //        //if (currentVariable.CompareTo(minimum) < 0)
    //        //{
    //        //    minimum = currentVariable;
    //        //}

    //        // Calcular suma y promedio
    //        dynamic sum = Add(variableSum, currentVariable);
    //        variableSum = sum;
    //        variableAverage = Divide(variableSum, count);
    //        count++;
    //    }
    //}

    public void MeasureVariable() 
    {
        currentVariable = del();
        list.Add(currentVariable);
        // Calcular suma y promedio
        dynamic sum = Add(variableSum, currentVariable);
        variableSum = sum;
        variableAverage = Divide(variableSum, count);
        count++;
    }

    private dynamic Add(dynamic a, dynamic b)
    {
        return a + b;
    }

    private dynamic Divide(dynamic a, int b)
    {
        return a / b;
    }

    System.Collections.Generic.List<T> list;

    public void SeeList()
    {
        var a = list;
        Debug.Log(variableAverage);
        return;
    }
}
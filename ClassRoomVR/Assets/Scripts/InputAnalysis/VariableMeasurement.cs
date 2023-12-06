using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
/// <summary>
/// Calcula las estadisticas de una variable especifica
/// Cada vez que se actualiza una variable se agrega ese valor y se calculan la nueva media, maximo,...
/// </summary>
public class VariableMeasurement
{
    //media,moda,rango,desviacion estandar,variaza,cuartiles,asimetria,curtosis,boxplot
    private float variable;
    
    // private float variableAverage;
    RunningStatistics runStats;
    List<float> actionStats;
   // MovingStatistics movStats;
    public RunningStatistics Run => runStats;
    //public MovingStatistics Mov => movStats;

    public double ActionMean => actionStats.Mean();

    public float Variable
    {
        get => variable;
        set
        {
            variable = value;
            UpdateStats();
        }
    }

   
    public VariableMeasurement(int windowSize)
    {
        
        //list = new List<float>();
        //StreamingStatistics.Mean(list);
        // Statistics.Mean(list);
      //  movStats = new MovingStatistics(windowSize);
        runStats = new RunningStatistics();
        actionStats = new List<float>();
        //TO DO ? 1: usar streamingstatistics en vez de running
        //TO DO ? 2: Usar moving statistics solo cuando se pide y no cuando se agrega una variable 
    }

    public void NewAction() 
    {
        actionStats.Clear();
    }

    /// <summary>
    /// Actualizar las stats agregando la variable a las diferentes estadisticas
    /// </summary>
    private void UpdateStats()
    {
        runStats.Push(variable);
       // movStats.Push(variable);
        actionStats.Add(variable);
    }



}
/// <summary>
/// Calcula las estadisticas de una vector de tres dimensiones
/// Cada vez que se actualiza el vector, se agrega ese valor y se calculan las estadisticas de cada dimension por separado
/// </summary>
public class VariableMeasurementVector3
{
    private Vector3 variable;
    public Vector3 Variable
    {
        get => variable;
        set
        {
            variable = value;
            UpdateStats();
        }
    }

    private RunningStatistics[] runStats;
    private MovingStatistics[] movStats;

    public RunningStatistics RunX => runStats[0];
    public RunningStatistics RunY => runStats[1];
    public RunningStatistics RunZ => runStats[2];

    public MovingStatistics MovX => movStats[0];
    public MovingStatistics MovY => movStats[1];
    public MovingStatistics MovZ => movStats[2];

    public VariableMeasurementVector3(int windowSize)
    {
        runStats = new RunningStatistics[3];
        movStats = new MovingStatistics[3];

        for (int i = 0; i < 3; i++)
        {
            runStats[i] = new RunningStatistics();
            movStats[i] = new MovingStatistics(windowSize);
        }
    }

    /// <summary>
    /// Actualizar las stats agregando el vector a las diferentes estadisticas
    /// </summary>
    private void UpdateStats()
    {
        float[] components = { variable.x, variable.y, variable.z };

        for (int i = 0; i < 3; i++)
        {
            runStats[i].Push(components[i]);
            movStats[i].Push(components[i]);
        }
    }
}







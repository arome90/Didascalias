using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

public class VariableMeasurement
{
    //media,moda,rango,desviacion estandar,variaza,cuartiles,asimetria,curtosis,boxplot
    private float variable;
    
    // private float variableAverage;
    RunningStatistics runStats;
    MovingStatistics movStats;
    public RunningStatistics Run => runStats;
    public MovingStatistics Mov => movStats;

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
        movStats = new MovingStatistics(windowSize);
        runStats = new RunningStatistics();
        
        //TO DO ? 1: usar streamingstatistics en vez de running
        //TO DO ? 2: Usar moving statistics solo cuando se pide y no cuando se agrega una variable 
        //asi se le pasa una lista y coge

    }


    //List<float> list;

    private void UpdateStats()
    {
        runStats.Push(variable);
        movStats.Push(variable);
    }



}


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







//public enum StatisticType
//{
//    Average,
//    Maximum,
//    Minimum
//}



//public double GetStatistic(StatisticType type)
//{

//    switch (type)
//    {
//        case StatisticType.Average:
//            return run.Mean;
//        case StatisticType.Maximum:
//            return run.Maximum;
//        case StatisticType.Minimum:
//            return run.Minimum;
//        default:
//            throw new ArgumentException("Invalid statistic type");
//    }
//}


//public double GetStatisticWindows(StatisticType type)
//{

//    switch (type)
//    {
//        case StatisticType.Average:
//            return mov.Mean;
//        case StatisticType.Maximum:
//            return mov.Maximum;
//        case StatisticType.Minimum:
//            return mov.Minimum;
//        default:
//            throw new ArgumentException("Invalid statistic type");
//    }
//}

//public interface IStatisticsProvider
//{
//    double Mean { get; }
//    double Maximum { get; }
//    double Minimum { get; }
//    void Push(float value);
//}

//public class RunningStatistics : IStatisticsProvider
//{
//    private MathNet.Numerics.Statistics.RunningStatistics statistics = new MathNet.Numerics.Statistics.RunningStatistics();

//    public double Mean => statistics.Mean;
//    public double Maximum => statistics.Maximum;
//    public double Minimum => statistics.Maximum;

//    public void Push(float value)
//    {
//        statistics.Push(value);
//    }
//}

//public class MovingStatistics : IStatisticsProvider
//{
//    private MathNet.Numerics.Statistics.MovingStatistics statistics = new MathNet.Numerics.Statistics.MovingStatistics(5);

//    public double Mean => statistics.Mean;
//    public double Maximum => statistics.Maximum;
//    public double Minimum => statistics.Minimum;

//    public void Push(float value)
//    {

//        statistics.Push(value);
//    }
//}







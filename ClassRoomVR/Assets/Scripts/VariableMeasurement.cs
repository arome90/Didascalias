using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

public class VariableMeasurementFloat
{
    //media,moda,rango,desviacion estandar,variaza,cuartiles,asimetria,curtosis,boxplot
    private float variable;
    public float Variable
    {
        get => variable;
        set
        {
            variable = value;
            UpdateStats();
        }
    }

    // private float variableAverage;
    RunningStatistics run;
    MovingStatistics mov;

    public VariableMeasurementFloat()
    {
        
        list = new List<float>();
        //StreamingStatistics.Mean(list);
        // Statistics.Mean(list);
        mov = new MovingStatistics();
        run = new RunningStatistics();
        //TO DO ? 1: usar streamingstatistics en vez de running
        //TO DO ? 2: Usar moving statistics solo cuando se pide y no cuando se agrega una variable 
        //asi se le pasa una lista y coge

    }


    List<float> list;

    private void UpdateStats()
    {
        run.Push(variable);
        mov.Push(variable);
    }

    public enum StatisticType
    {
        Average,
        Maximum,
        Minimum
    }


    public double GetStatistic(StatisticType type)
    {
        return GetStatisticFromProvider(run, type);
    }

    public double GetStatisticWindows(StatisticType type)
    {
        return GetStatisticFromProvider(mov, type);
    }

    private double GetStatisticFromProvider(IStatisticsProvider provider, StatisticType type)
    {
        switch (type)
        {
            case StatisticType.Average:
                return provider.Mean;
            case StatisticType.Maximum:
                return provider.Maximum;
            case StatisticType.Minimum:
                return provider.Minimum;
            default:
                throw new ArgumentException("Invalid statistic type");
        }
    }
}
    public interface IStatisticsProvider
    {
        double Mean { get; }
        double Maximum { get; }
        double Minimum { get; }
        void Push(float value);
    }

    public class RunningStatistics : IStatisticsProvider
    {
        private MathNet.Numerics.Statistics.RunningStatistics statistics = new MathNet.Numerics.Statistics.RunningStatistics();

        public double Mean => statistics.Mean;
        public double Maximum => statistics.Maximum;
        public double Minimum => statistics.Maximum;

        public void Push(float value)
        {
            statistics.Push(value);
        }
    }

    public class MovingStatistics : IStatisticsProvider
    {
        private MathNet.Numerics.Statistics.MovingStatistics statistics = new MathNet.Numerics.Statistics.MovingStatistics(5);

        public double Mean => statistics.Mean;
        public double Maximum => statistics.Maximum;
        public double Minimum => statistics.Minimum;

        public void Push(float value)
        {
        
            statistics.Push(value);
        }
    }

   
    




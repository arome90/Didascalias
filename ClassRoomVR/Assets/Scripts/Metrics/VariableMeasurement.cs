using UnityEngine;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

/// <summary>
/// Calcula las estadísticas de una variable específica.
/// Cada vez que se actualiza una variable, se agrega ese valor y se calculan la nueva media, máximo, etc.
/// </summary>
public class VariableMeasurement
{
    private float _variable;
    private RunningStatistics _runStats;
    private List<float> _actionStats;

    public RunningStatistics Run => _runStats;
    public double ActionMean => _actionStats.Mean();

    /// <summary>
    /// Variable cuyo valor actualizamos y medimos.
    /// </summary>
    public float Variable
    {
        get => _variable;
        set
        {
            _variable = value;
            UpdateStats();
        }
    }

    /// <summary>
    /// Constructor de VariableMeasurement.
    /// </summary>
    public VariableMeasurement()
    {
        _runStats = new RunningStatistics();
        _actionStats = new List<float>();
    }

    /// <summary>
    /// Limpia las estadísticas de acción al comenzar una nueva.
    /// </summary>
    public void NewAction()
    {
        _actionStats.Clear();
    }

    /// <summary>
    /// Actualiza las estadísticas agregando la variable a las diferentes estadísticas.
    /// </summary>
    private void UpdateStats()
    {
        _runStats.Push(_variable);
        _actionStats.Add(_variable);
    }
}

/// <summary>
/// Calcula las estadísticas de un vector de tres dimensiones (Vector3).
/// Cada vez que se actualiza el vector, se agrega el valor y se calculan estadísticas de cada dimensión por separado.
/// </summary>
public class VariableMeasurementVector3
{
    private Vector3 _variable; // Campo privado con guion bajo
    private RunningStatistics[] _runStats;
    private MovingStatistics[] _movStats;

    // Propiedades públicas que exponen estadísticas para cada componente del Vector3
    public RunningStatistics RunX => _runStats[0];
    public RunningStatistics RunY => _runStats[1];
    public RunningStatistics RunZ => _runStats[2];

    public MovingStatistics MovX => _movStats[0];
    public MovingStatistics MovY => _movStats[1];
    public MovingStatistics MovZ => _movStats[2];

    /// <summary>
    /// Constructor de VariableMeasurementVector3.
    /// </summary>
    /// <param name="windowSize">Tamaño de la ventana para MovingStatistics.</param>
    public VariableMeasurementVector3(int windowSize)
    {
        _runStats = new RunningStatistics[3];
        _movStats = new MovingStatistics[3];

        for (int i = 0; i < 3; i++)
        {
            _runStats[i] = new RunningStatistics();
            _movStats[i] = new MovingStatistics(windowSize);
        }
    }

    /// <summary>
    /// Vector3 cuyo valor actualizamos y medimos.
    /// </summary>
    public Vector3 Variable
    {
        get => _variable;
        set
        {
            _variable = value;
            UpdateStats();
        }
    }

    /// <summary>
    /// Actualiza las estadísticas de cada componente del vector.
    /// </summary>
    private void UpdateStats()
    {
        float[] components = { _variable.x, _variable.y, _variable.z };

        for (int i = 0; i < 3; i++)
        {
            _runStats[i].Push(components[i]);
            _movStats[i].Push(components[i]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Statistics;
using Newtonsoft.Json;

// Class that defines the class environment
public class ClassEnvironment : MonoBehaviour
{
    int disposicion;
    bool trabajogrupal;
}

// Class that handles time information
public class TimeClass : MonoBehaviour
{
    private string horaInicial;

    private class TimeEntry
    {
        public string Hora { get; }
        public double Participacion { get; }

        public TimeEntry(string hora, double participacion)
        {
            Hora = hora;
            Participacion = participacion;
        }
    }

    private TimeEntry[] horarios = new TimeEntry[]
    {
        new TimeEntry("08:00", 0.8),
        new TimeEntry("8:55", 0.9),
        new TimeEntry("9:50", 1.0),
        new TimeEntry("10:45", 0.9),
        new TimeEntry("12:10", 1),
        new TimeEntry("13:05", 1.1),
        new TimeEntry("14:00", 0.9)
    };

    private void Start()
    {
        horaInicial = horarios[0].Hora;
    }
}

// Class that defines the class climate properties
[System.Serializable]
public class Clima
{
    // Variables defining different aspects of the class climate
    float[] clima = new float[9];
    float relacionesInterpersonales;
    float comunicacion;
    float participacion;
    float apoyoEmocional;
    float gestionAula;
    float diversidad;
    float motivacion;
    float seguridad;
    float pertenencia;

    // Adjustment factors for each aspect of the climate
    [JsonIgnore] float factorRelacionesInterpersonales = 1.0f;
    [JsonIgnore] float factorComunicacion = 1.0f;
    [JsonIgnore] float factorParticipacion = 1.0f;
    [JsonIgnore] float factorApoyoEmocional = 1.0f;
    [JsonIgnore] float factorGestionAula = 1.0f;
    [JsonIgnore] float factorDiversidad = 1.0f;
    [JsonIgnore] float factorMotivacion = 1.0f;
    [JsonIgnore] float factorSeguridad = 1.0f;
    [JsonIgnore] float factorPertenencia = 1.0f;

    // Method to update the climate based on current factors and values
    public void UpdateClima()
    {
        clima[0] = relacionesInterpersonales * factorRelacionesInterpersonales;
        clima[1] = comunicacion * factorComunicacion;
        clima[2] = participacion * factorParticipacion;
        clima[3] = apoyoEmocional * factorApoyoEmocional;
        clima[4] = gestionAula * factorGestionAula;
        clima[5] = diversidad * factorDiversidad;
        clima[6] = motivacion * factorMotivacion;
        clima[7] = seguridad * factorSeguridad;
        clima[8] = pertenencia * factorPertenencia;
    }

    // Method to get the average value of the climate
    public double GetClimaMean()
    {
        return clima.Mean();
    }
}

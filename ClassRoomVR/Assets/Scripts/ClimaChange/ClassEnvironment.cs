using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Statistics;
using Newtonsoft.Json;

public class ClassEnvironment : MonoBehaviour
{
    int disposicion;
    //U
    //factorcomunicasion 1.2
    //O
    //factorparticipacion 1.1

    //Si el trabajo es grupal la participacion aumenta 
    bool trabajogrupal;

}

public class TimeClass : MonoBehaviour
{
    string horaInicial;
    class TimeEntry
    {
        public string hora;
        public double participacion;

        public TimeEntry(string hora, double participacion)
        {
            this.hora = hora;
            this.participacion = participacion;
        }
    }

    TimeEntry[] horarios = new TimeEntry[]
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
        horaInicial = horarios[0].hora;
    }

}

//la hora y el tipo de aula afectan al clima
[System.Serializable]
public class Clima
{
    //TO DO ? CAMBIAR LAS VARIABLES EN OTRA PARTE DEL CODIGO 
    VariableMeasurement climaVariable;
     float[] clima = new float[9];
    float relacionesInterpersonales;
    float comunicacion;
    //?Comunicacion negativa entre alumnos ??
    float participacion;
    float apoyoEmocional;
    float gestionAula;
    float diversidad;
    float motivacion;
    float seguridad;
    float pertenencia;
    //float atencion;

    [JsonIgnore] float factorRelacionesInterpersonales = 1.0f;
    [JsonIgnore] float factorComunicacion = 1.0f;
    [JsonIgnore] float factorParticipacion = 1.0f;
    [JsonIgnore] float factorApoyoEmocional = 1.0f;
    [JsonIgnore] float factorGestionAula = 1.0f;
    [JsonIgnore] float factorDiversidad = 1.0f;
    [JsonIgnore] float factorMotivacion = 1.0f;
    [JsonIgnore] float factorSeguridad = 1.0f;
    [JsonIgnore] float factorPertenencia = 1.0f;
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
        climaVariable.Variable = (float)clima.Mean();
    }

    public double getClima()
    {
        return clima.Mean();
    }
}


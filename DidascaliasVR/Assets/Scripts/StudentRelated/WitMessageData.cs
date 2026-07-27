using System.Collections.Generic;
using System;

/// <summary>
/// Representa la intención de un mensaje de Wit
/// </summary>
[Serializable]
public enum Intention
{
    None = 0,
    Expulsion = 1,
    CambiarAlumno = 2,
    Sentarse = 3,
    Postponer = 4
}

/// <summary>
/// Estructura que guarda la intención de una transcripción de wit, además
/// de sus correspondientes estudiantes afectados
/// </summary>
public struct  WitMessageData
{
    public List<string> Names;
    public Intention Intention;
    public string Transcription;
}

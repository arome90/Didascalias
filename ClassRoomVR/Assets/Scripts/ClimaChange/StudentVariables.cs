using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class StudentVariables
{
    // Variables de comportamiento del alumno en clase
    public float attentionLevel;
    public float mood;
    public float participation;
    public float socialInteraction;
    public float understandingLevel;
    public float interestLevel;
    public float activityParticipation;
    public float confidenceLevel;
    public float motivation;
    public float preparationLevel;
    public float shynessLevel;
    public float teacherRelationship;
    public float sustainedAttentionTime;
    public float questionParticipation;
    public float disruptiveBehavior;
    public float teamworkLevel;
    public float effortLevel;

    // Constructor
    public StudentVariables()
    {
        attentionLevel = 50.0f;
        mood = 50.0f;
        participation = 25.0f;
        socialInteraction = 50.0f;
        understandingLevel = 30.0f;
        interestLevel = 60.0f;
        activityParticipation = 0.0f;
        confidenceLevel = 50.0f;
        motivation = 50.0f;
        preparationLevel = 30.0f;
        shynessLevel = 50.0f;
        teacherRelationship = 50.0f;
        sustainedAttentionTime = 60.0f;
        questionParticipation = 0.0f;
        disruptiveBehavior = 0.0f;
        teamworkLevel = 50.0f;
        effortLevel = 50.0f;
    }


    enum Aptitudes 
    {
        OrigenExtranjero,
        AprendizajeRapido,
        Empatia,
        ResolucionCreativa,
        Liderazgo,
        GestionEmociones,
        ComunicacionAsertiva,
        PersonaConDiscapacidad,
        PadresSeparados,
        PadresSobreProtectores,
        HijxUnico,
    }
}

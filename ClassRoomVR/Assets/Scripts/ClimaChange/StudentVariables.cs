public class StudentVariables
{
    // Behavioral variables of the student in class
    public float AttentionLevel { get; set; }
    public float Mood { get; set; }
    public float Participation { get; set; }
    public float SocialInteraction { get; set; }
    public float UnderstandingLevel { get; set; }
    public float InterestLevel { get; set; }
    public float ActivityParticipation { get; set; }
    public float ConfidenceLevel { get; set; }
    public float Motivation { get; set; }
    public float PreparationLevel { get; set; }
    public float ShynessLevel { get; set; }
    public float TeacherRelationship { get; set; }
    public float SustainedAttentionTime { get; set; }
    public float QuestionParticipation { get; set; }
    public float DisruptiveBehavior { get; set; }
    public float TeamworkLevel { get; set; }
    public float EffortLevel { get; set; }

    // Default constructor
    public StudentVariables()
    {
        // Initialize default values for the student variables
        AttentionLevel = 50.0f;
        Mood = 50.0f;
        Participation = 25.0f;
        SocialInteraction = 50.0f;
        UnderstandingLevel = 30.0f;
        InterestLevel = 60.0f;
        ActivityParticipation = 0.0f;
        ConfidenceLevel = 50.0f;
        Motivation = 50.0f;
        PreparationLevel = 30.0f;
        ShynessLevel = 50.0f;
        TeacherRelationship = 50.0f;
        SustainedAttentionTime = 60.0f;
        QuestionParticipation = 0.0f;
        DisruptiveBehavior = 0.0f;
        TeamworkLevel = 50.0f;
        EffortLevel = 50.0f;
    }

    // Enumeration that defines student aptitudes
    public enum Aptitudes
    {
        ForeignOrigin,
        QuickLearner,
        Empathy,
        CreativeProblemSolving,
        Leadership,
        EmotionalManagement,
        AssertiveCommunication,
        PersonWithDisability,
        SeparatedParents,
        OverprotectiveParents,
        OnlyChild,
    }
}

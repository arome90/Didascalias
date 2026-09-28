using System;
using System.Collections.Generic;
using UnityEngine;

public enum ConflictType
{
    Disrespect = 0,
    SitTogether,
    StandUp,

    // Autism
    Hyperstimulation,
    DistractionTEA,

    // ADHD
    MaterialOutWrong,
    BotherStudents,
    DrawDistracted,


    UNKNOWN
}


public enum ConflictGenerationError
{
    None,
    MaxActiveConflictsReached,
    NotFeasible,
    NoValidStudent,
    AlreadyActiveConflictForStudent,
    Unimplemented
}

public struct ConflictGenerationResult
{
    public ConflictGenerationError Error;
    public string errorWhy;
#nullable enable
    public Conflict? ConflictInstance;
#nullable restore
}

public static class ConflictFactory 
{
    // Diccionario que mapea el Enum con el Type concreto de la subclase
    private static readonly Dictionary<ConflictType, Type> _conflictRegistry = new()
    {
        { ConflictType.SitTogether,         typeof(SitTogetherConflict) },
        { ConflictType.StandUp,             typeof(StandUpConflict) },
        { ConflictType.Hyperstimulation,    typeof(HyperstimulationConflict) },
        { ConflictType.DistractionTEA,      typeof(GetDistractedTEAConflict) },
        { ConflictType.MaterialOutWrong,    typeof(GetMaterialWrongConflict) },
        { ConflictType.BotherStudents,      typeof(BotherSomeoneConflict) },
        { ConflictType.DrawDistracted,      typeof(DrawDistractedConflict) },
    };

    /// <summary>
    /// Crea e instancia el conflicto adecuado según el tipo enviado.
    /// </summary>
    public static ConflictGenerationResult CreateConflict(ConflictType type)
    {
        ConflictGenerationResult result = new ConflictGenerationResult();

        if (!_conflictRegistry.TryGetValue(type, out Type conflictType))
        {
            result.errorWhy = $"[ConflictFactory] ConflictType '{type}' is not registered.";
            result.Error = ConflictGenerationError.Unimplemented;

            Debug.LogError(result.errorWhy);

            result.ConflictInstance = null;

            return result;
        }

        // ScriptableObject requiere CreateInstance en lugar de 'new'
        Conflict conflict = (Conflict)ScriptableObject.CreateInstance(conflictType);

        result.ConflictInstance = conflict;
        result.Error = ConflictGenerationError.None;

        return result;
    }
}

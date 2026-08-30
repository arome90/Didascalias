public static class LLMSchemas
{
    public static readonly object InputEvaluation = new
    {
        type = "object",
        properties = new
        {
            Intent = new
            {
                type = "string",
                pattern = "^[a-zA-Z0-9_]+$",
                description = "Intención detrás del mensaje."
            }
        },
        required = new[] { "Intent" },
        additionalProperties = false
    };

    public static readonly object StudentDialogue = new
    {
        type = "object",
        properties = new
        {
            Answer = new
            {
                type = "string",
                description = "Respuesta del estudiante."
            },
            EndOfConversation = new
            {
                type = "boolean",
                description = "true si la conversación terminó. false si todavía sigue."
            }
        },
        required = new[] { "Answer", "EndOfConversation" },
        additionalProperties = false
    };

    public static readonly object ActionSelector = new
    {
        type = "object",
        properties = new
        {
            Answer = new
            {
                type = new[] { "string", "null" },
                pattern = "^[a-zA-Z0-9_]+$",
                description = "Solo el nombre del método sin paréntesis."
            }
        },
        required = new[] { "Answer" },
        additionalProperties = false
    };

    public static readonly object ClassContextSummary = new
    {
        type = "object",
        properties = new
        {
            LessonSummary = new
            {
                type = "string",
                description = "Resumen de la lección y lo dicho por el profesor al general de la clase."
            },
        },
        required = new[] { "LessonSummary" },
        additionalProperties = false
    };

    public static readonly object PeersContextSummary = new
    {
        type = "object",
        properties = new
        {
            PeerEventsSummary = new
            {
                type = "string",
                description = "Resumen de las interacciones entre profesor y alumnos."
            }
        },
        required = new[] { "PeerEventsSummary" },
        additionalProperties = false
    };

    public static readonly object GenericAnswer = new
    {
        type = "object",
        properties = new
        {
            Answer = new
            {
                type = "string",
                description = "Responde aquí."
            }
        },
        required = new[] { "Answer" },
        additionalProperties = false
    };
}

using Didascalia.LLM;
using System;
using System.Threading.Tasks;

public class InputEvaluatorAgent : LLMAgent<InputEvaluatorAgent>
{
    public override object ResponseSchema => LLMSchemas.InputEvaluation;

    [Serializable]
    public class InputEvaluationResult
    {
        public string Transcription;
        public string Intent;
    }

    public async Task<InputEvaluationResult> EvaluateInputAsync(string rawTeacherQuery, Student st)
    {
        string prompt = ReplacePlaceholder(_promptTemplate, "{TEACHER_QUERY}", rawTeacherQuery);
        if (st == null) prompt = ReplacePlaceholder(prompt, "{CONVERSATION_HISTORY}", "No ha habido conversación previa");
        else prompt = ReplacePlaceholder(prompt, "{CONVERSATION_HISTORY}", LLMHelpers.BuildConversationHistory(st.GetInteractionHistory()));

        InputEvaluationResult result = await ExecuteJsonAsync<InputEvaluationResult>(prompt);

        if (result == null)
            result = new InputEvaluationResult { Transcription = string.Empty, Intent = "Desconocida" };
        else
            result.Transcription = rawTeacherQuery;

        return result;
    }
}
using Didascalia.LLM;
using System.Threading.Tasks;

public class ClassInterruptionAgent : StudentBaseAgent<ClassInterruptionAgent>
{
    public override object ResponseSchema => LLMSchemas.StudentDialogue;

    public async Task<LLMHelpers.LLMGenericAnswerResult> GenerateResponseAsync(Student st, string query)
    {
        string prompt = ReplacePlaceholder(_promptTemplate, "{STUDENT_PROFILE}", GetStudentProfile(st));
        prompt = ReplacePlaceholder(prompt, "{USER_QUERY}", query);
        prompt = ReplacePlaceholder(prompt, "{CLASS_CONTEXT}", GetWholeClassContext());

        return await ExecuteJsonAsync<LLMHelpers.LLMGenericAnswerResult>(prompt);
    }
}


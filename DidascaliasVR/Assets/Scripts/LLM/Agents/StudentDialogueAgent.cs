using System.Threading.Tasks;
using Didascalia.LLM;

public class StudentDialogueAgent : StudentBaseAgent<StudentDialogueAgent>
{
    public async Task<StudentDialogueResult> GenerateResponseAsync(Student st, InputEvaluatorAgent.InputEvaluationResult eval)
    {
        string prompt = ReplacePlaceholder(_promptTemplate, "{STUDENT_PROFILE}", GetStudentProfile(st)); ;
        prompt = ReplacePlaceholder(prompt, "{CURRENT_ACTION}", st.GetActionContext().ToString());
        prompt = ReplacePlaceholder(prompt, "{CONVERSATION_HISTORY}", LLMHelpers.BuildConversationHistory(st.GetInteractionHistory()));
        prompt = ReplacePlaceholder(prompt, "{TEACHER_QUERY}", eval.Transcription);
        prompt = ReplacePlaceholder(prompt, "{INTENT}", eval.Intent);
        prompt = ReplacePlaceholder(prompt, "{CLASS_CONTEXT}", GetWholeClassContext());

        return await ExecuteJsonAsync<StudentDialogueResult>(prompt);
    }
}
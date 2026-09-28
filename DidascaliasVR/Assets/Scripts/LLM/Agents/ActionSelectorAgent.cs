using Didascalia.LLM;
using System.Text;
using System.Threading.Tasks;
using static InputEvaluatorAgent;

public class ActionSelectorAgent : LLMAgent<ActionSelectorAgent>
{
    public override object ResponseSchema => LLMSchemas.ActionSelector;

    public bool CanDoAction(Student st)
    {
        StudentActionContext currentActionContext = st.GetActionContext();
        return currentActionContext.avaliableActions != null && currentActionContext.avaliableActions.Count > 0;
    }

    public async Task<LLMHelpers.LLMGenericAnswerResult> SelectActionAsync(Student st, InputEvaluationResult eval, string studentAnswer)
    {
        StudentActionContext currentActionContext = st.GetActionContext();
        string prompt = _promptTemplate;

        prompt = ReplacePlaceholder(prompt, "{CURRENT_ACTION}", currentActionContext.ToString());
        prompt = ReplacePlaceholder(prompt, "{TEACHER_QUERY}", eval.Transcription);
        prompt = ReplacePlaceholder(prompt, "{STUDENT_ANSWER}", studentAnswer);
        prompt = ReplacePlaceholder(prompt, "{INTENT}", studentAnswer);

        if (CanDoAction(st))
        {
            StringBuilder actions = new StringBuilder();
            foreach (string action in currentActionContext.avaliableActions)
            {
                actions.AppendLine("  - " + action);
            }
            prompt = ReplacePlaceholder(prompt, "{AVAILABLE_ACTIONS}", actions.ToString());
        }
        else
        {
            prompt = ReplacePlaceholder(prompt, "{AVAILABLE_ACTIONS}", "NINGUNA. PON null COMO ACCIÓN");
        }

        return await ExecuteJsonAsync<LLMHelpers.LLMGenericAnswerResult>(prompt);
    }
}
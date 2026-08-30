using Didascalia.LLM;
using System;
using System.Text;
using UnityEngine;

[Serializable]
public class StudentDialogueResult
{
    public string Answer;
    public bool EndOfConversation;
}

public abstract class StudentBaseAgent<T> : LLMAgent<T> where T : MonoBehaviour
{

    public override object ResponseSchema => LLMSchemas.StudentDialogue;

    protected string GetStudentProfile(Student st)
    {
        if (st.GetProfile() == null) {
            string prompt = LLMHelpers.GetPlaceholderStudentProfile()
            .Replace("{NAME}", st.Name)
            .Replace("{AGE}", st.Age.ToString())
            .Replace("{GENDER}", st.Gender.ToString())
            .Replace("{BEHAVIOUR}", st.StType.ToString())
            .Replace("{BEHAVIOUR_PATTERNS}", LLMHelpers.StudentContextByType[st.StType]);

            st.SetProfile(prompt);
        }

        return st.GetProfile();
    }

    protected string GetWholeClassContext()
    {
        StringBuilder wholeContext = new StringBuilder();

        if (ClassroomContextSummarizer.Exists)
            wholeContext.Append($"Contexto de clase:\n{ClassroomContextSummarizer.Instance.GetWholeContext()}\n");

        if (PeersContextSummarizer.Exists)
            wholeContext.Append($"Interacciones con alumnos:\n{PeersContextSummarizer.Instance.GetWholeContext()}");

        return wholeContext.ToString();
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class LLMManager : Singleton<LLMManager>
{
    private string _systemPromptStart = "";
    private string _systemPromptEnd = "";
    private string _answerTeacherPrompt = "";
    // private string _generalContext = "";
    private string _path = Application.streamingAssetsPath + "/Context/";

    Dictionary<StudentType, string> _contextByType = null;

    private void Start()
    {
        // setting system prompt
        _systemPromptStart = GetTextFromFile(_path + "SystemPromptStart.txt");
        _systemPromptEnd = GetTextFromFile(_path + "SystemPromptEnd.txt");
        _answerTeacherPrompt = GetTextFromFile(_path + "AnswerTeacher.txt");
        // _generalContext = GetTextFromFile(_path + "GeneralContext.txt");

        // getting all different contexts
        PopulateContextDictionary();
    }

    private void PopulateContextDictionary()
    {
        _contextByType = new Dictionary<StudentType, string>();

        for (int i = 0; i < (int)StudentType.Problematic + 1; ++i)
        {
            string text = GetTextFromFile((_path + ((StudentType)i).ToString() + ".txt"));
            _contextByType.Add((StudentType)i, text);        
        }
    }

    private string GetTextFromFile(string path)
    {
        StreamReader context = new StreamReader(path);
        return context.ReadToEnd();
    }

    public void GenerateStudentContext(Student st, List<string> studentHistory = null)
    {
        StringBuilder sb = new StringBuilder();

        // specifics
        if (st.Gender == Gender.Girl)
        {
            sb.AppendLine($"Eres una estudiante de {st.Age} años llamada {st.Name} en un aula escolar.");
            sb.AppendLine($"Tu lenguaje, vocabulario, sintaxis y nivel conceptual DEBEN adaptarse estrictamente a una niña/adolescente de {st.Age} años.");
        }
        else
        {
            sb.AppendLine($"Eres un estudiante de {st.Age} años llamado {st.Name} en un aula escolar.");
            sb.AppendLine($"Tu lenguaje, vocabulario, sintaxis y nivel conceptual DEBEN adaptarse estrictamente a un niño/adolescente de {st.Age} años.");
        }

        sb.AppendLine($"No utilices explicaciones maduras, lenguaje académico complejo ni un tono formal de adulto a menos que tu perfil o edad lo justifiquen.");

        // per-role
        sb.AppendLine($"Tu Categoría es: {st.StType.ToString()}");
        sb.AppendLine(_contextByType[st.StType]);

        // rules
        sb.AppendLine("¡RECUERDA! Estas son tus reglas de formato:");
        sb.AppendLine($"Responde SIEMPRE en primera persona interpretando a {st.Name}");
        sb.AppendLine($"JAMÁS rompas el personaje. No agregues saludos fuera de rol, notas aclaratorias ni explicaciones de por qué respondes así");

        st.SetContext(sb.ToString());
    }

    public string AddHistoryToContext(string ctx, List<string> interactionHistory)
    {
        // history - the interactions that the student has seen
        if (interactionHistory != null && interactionHistory.Count > 0)
        {
            ctx += "\n" + "Estas son las interacciones que has visto o tenido:\n";
            foreach (string context in interactionHistory) ctx += "\t- " + context + "\n";
        }

        return ctx;
    }

    private string BuildFinalPrompt(string studentContext, string userQuery = null)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(_systemPromptStart);

        // TODO: Add general context, such as general class ambience and subject. since general context may change, we should add that here
        // sb.Append(_generalContext);

        sb.AppendLine(studentContext);

        sb.AppendLine(_systemPromptEnd);

        if (userQuery != null && userQuery.Length > 0)
        {
            sb.AppendLine(_answerTeacherPrompt);

            sb.Append($"\"{userQuery}\"");
        }

        return sb.ToString();
    }

    public void LLMInteraction_TeacherSpeaksToStudent(string userQuery, Student st)
    {
        string prompt = BuildFinalPrompt(st.GetContext(), userQuery);

        st.AddTeacherInteractionContext(userQuery);
        LLMNetworkManager.Instance.QueryLLM(prompt, st);
    }
}

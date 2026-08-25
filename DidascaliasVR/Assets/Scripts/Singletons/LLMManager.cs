using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LLMManager : Singleton<LLMManager>
{
    //private string _systemPromptStart = "";
    //private string _systemPromptEnd = "";
    //private string _answerTeacherPrompt = "";
    // private string _generalContext = "";

    private string _systemPromptTemplate = "";

    private string _path = Application.streamingAssetsPath + "/Context/";

    Dictionary<StudentType, string> _contextByType = null;

    protected override void Awake()
    {
        base.Awake();

        _systemPromptTemplate = FileHelper.GetTextFromFile(_path + "SystemPromptTemplate.txt");

        // getting all different contexts
        PopulateContextDictionary();
    }

    private void Start()
    {
        SpeechManager.Instance.OnTranscriptionReceived.AddListener(OnTranscriptionReceived);
    }

    private void PopulateContextDictionary()
    {
        _contextByType = new Dictionary<StudentType, string>();

        for (int i = 0; i < (int)StudentType.Autistic + 1; ++i)
        {
            string text = FileHelper.GetTextFromFile((_path + ((StudentType)i).ToString() + ".txt"));
            _contextByType.Add((StudentType)i, text);        
        }
    }

    public void GenerateStudentContext(Student st, List<string> studentHistory = null)
    {
        StringBuilder sb = new StringBuilder();

        // we copy the string
        string context = new string(_systemPromptTemplate);

        context = context
            .Replace("{NAME}", st.Name)
            .Replace("{AGE}", st.Age.ToString())
            .Replace("{GENDER}", st.Gender.ToString())
            .Replace("{BEHAVIOUR}", st.StType.ToString())
            .Replace("{BEHAVIOUR_PATTERNS", _contextByType[st.StType]);

        st.SetContext(context);
    }

    public string AddHistoryToContext(List<string> interactionHistory)
    {
        string ctx = "";
        // history - the interactions that the student has seen
        if (interactionHistory != null && interactionHistory.Count > 0)
        {
            foreach (string context in interactionHistory) ctx += "\t- " + context + "\n";
        }

        return ctx;
    }

    private string BuildFinalPrompt(string userQuery, Student st)
    {
        string context = st.GetContext();

        StudentActionContext currentActionContext = st.GetActionContext();
        List<StudentActionContext> previousActionHistory = st.GetPreviousActionContext();

        if (previousActionHistory == null) context = context.Replace("{ACTION_HISTORY}", "NO HAN HABIDO ACCIONES PREVIAS");
        else
        {
            StringBuilder actionSb = new StringBuilder();
            foreach (StudentActionContext action in previousActionHistory)
            {
                actionSb.AppendLine(action.ToString());
                actionSb.AppendLine("================");
            }
            context = context.Replace("{ACTION_HISTORY}", actionSb.ToString());
        }
        context = context.Replace("{CURRENT_ACTION}", currentActionContext.ToString());

        List<string> interactionHistory = st.GetInteractionHistory();

        if (interactionHistory == null) { context = context.Replace("{CONVERSATION_HISTORY}", "NO HAN HABIDO INTERACCIONES"); }
        else
        {
            StringBuilder interactionSb = new StringBuilder();
            foreach(string interaction in interactionHistory)
            {
                interactionSb.AppendLine("  - " + interaction);
            }
            context = context.Replace("{CONVERSATION_HISTORY}", interactionSb.ToString());
        }

        context = context.Replace("{TEACHER_QUERY}", userQuery);
        if (currentActionContext.avaliableActions != null && currentActionContext.avaliableActions.Count > 0)
        {
            StringBuilder actions = new StringBuilder();
            foreach (string action in currentActionContext.avaliableActions) 
            {
                actions.AppendLine("  - " + action);
            }

            context = context.Replace("{AVALIABLE_ACTIONS}", actions.ToString());
        }
        else
            context = context.Replace("{AVALIABLE_ACTIONS}", "NINGUNA. PONE 'NONE' COMO ACCIÓN");

        return context;
    }

    public void LLMInteraction_TeacherSpeaksToStudent(string userQuery, Student st)
    {
        string prompt = BuildFinalPrompt(userQuery, st);

        LLMNetworkManager.Instance.QueryLLM(prompt, st);
        
        Debug.Log($"[LLMManager] Sent prompt: {prompt}");

        st.AddTeacherInteractionContext(userQuery);
    }

    private void OnTranscriptionReceived(string transcription)
    {

        List<Student> sts = StudentManager.Instance.GetSelectedStudents();
        Student st = sts != null && sts.Count > 0 ? sts[0] : null;
        if (st == null) return;

        LLMInteraction_TeacherSpeaksToStudent(transcription, st);
    }
}

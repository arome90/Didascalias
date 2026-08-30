using Didascalia.LLM;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LLMManager : Singleton<LLMManager>
{
    private void Start() => SpeechManager.Instance.OnTranscriptionReceived.AddListener(OnTranscriptionReceived);

    #region Flujo de Ejecución Secuencial
    public async void LLMInteraction_TeacherSpeaksToStudent(string userQuery, Student st)
    {
        try
        {
            InputEvaluatorAgent.InputEvaluationResult inputEval = await InputEvaluatorAgent.Instance.EvaluateInputAsync(userQuery, st);

            inputEval.Transcription = userQuery;

            if (inputEval == null)
                inputEval = new InputEvaluatorAgent.InputEvaluationResult 
                { Transcription = userQuery, Intent = "Desconocida" };

            if (inputEval.Intent.ToLower().Trim() == "general")
            {
                st = null; // se dirige a todo el mundo
                return;
            }

            else if (st == null && inputEval.Intent.ToLower().Trim().Substring(0, inputEval.Intent.Length - 1) == "pregunt")
                st = StudentManager.Instance.TryGetStudentByNameOrGetRandom(null);

            else if (st == null && inputEval.Intent.ToLower().Trim() == "sacarmaterial")
            {
                StudentManager.Instance.GetMaterialOutAllStudents();
                return;
            }

            else if (st == null) return;

            st.AddTeacherInteractionContext(inputEval.Transcription);

            Debug.Log($"Transcription: {inputEval.Transcription}\nIntent: {inputEval.Intent}");

            bool doAction = inputEval.Intent.ToLower().Trim().Substring(0, inputEval.Intent.Length - 1) != "desconocid";

            StudentDialogueResult studentResult = null;
            if (!doAction) st.SpeakDidNotUnderstand();
            else
            {
                studentResult = await StudentDialogueAgent.Instance.GenerateResponseAsync(st, inputEval);
                st.Speak(studentResult.Answer);

                if (studentResult.EndOfConversation)
                    StudentManager.Instance.DeselectStudent(st);

                PeersContextSummarizer.Instance.RegisterOneToOneInteraction(st.Name, inputEval.Transcription, studentResult.Answer);

                if (doAction && ActionSelectorAgent.Exists && ActionSelectorAgent.Instance.CanDoAction(st))
                {
                    LLMHelpers.LLMGenericAnswerResult actionResult =
                        await ActionSelectorAgent.Instance.SelectActionAsync(st, inputEval, studentResult.Answer);

                    if (actionResult != null && !string.IsNullOrEmpty(actionResult.Answer) 
                        && actionResult.Answer != "null")
                        st.Behaviour.ExecuteActionByNameReflection(actionResult.Answer);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LLMManager] Error en la cadena de agentes: {ex.Message}");
        }
    }

    private void OnTranscriptionReceived(string transcription)
    {
        List<Student> sts = StudentManager.Instance.GetSelectedStudents();
        Student st = sts != null && sts.Count > 0 ? sts[0] : null;

        LLMInteraction_TeacherSpeaksToStudent(transcription, st);
    }
    #endregion

    #region Prompt Student to do Something
    //public async Task LLMInteraction_StudentSpeaksToTeacher(Student who, string query)
    //{
    //    string prompt = BuildStudentContext(_studentInterruptsClass, who);

    //    prompt = prompt.Replace("{USER_QUERY}", query);

    //    string result = await LLMNetworkManager.Instance.QueryLLMAsync(prompt);

    //    who.Speak(result);
    //}
    #endregion
}
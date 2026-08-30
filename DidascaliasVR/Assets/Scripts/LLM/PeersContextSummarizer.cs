using UnityEngine;

public class PeersContextSummarizer : SummarizerAgent<PeersContextSummarizer>
{
    public override string logPath => "peers";

    /// <summary>
    /// Registra interacciones 1 a 1 entre el profesor y un alumno específico
    /// </summary>
    public void RegisterOneToOneInteraction(string studentName, string teacherQuery, string studentAnswer)
    {
        lock (_interactionBuffer)
        {
            string interaction = $"- Profesor a {studentName}: \"{teacherQuery}\" \n - {studentName} respondió: \"{studentAnswer}\"";
            _interactionBuffer.Add(interaction);
            FileHelper.AppendLog(Application.persistentDataPath + $"/context/{logPath}/interactions.txt", interaction, true);
        }

        if (ShouldRegister) TriggerSummary();
    }
}
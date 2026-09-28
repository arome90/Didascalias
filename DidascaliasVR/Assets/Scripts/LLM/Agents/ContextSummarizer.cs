using UnityEngine;

public class ClassroomContextSummarizer : SummarizerAgent<ClassroomContextSummarizer>
{
    public override string logPath => "classroom";
    private void Start() =>     SpeechManager.Instance.OnTranscriptionReceived.AddListener(OnSpeechReceived);
    private void OnDisable() => SpeechManager.Instance.OnTranscriptionReceived.RemoveListener(OnSpeechReceived);
    private void OnDestroy() => SpeechManager.Instance.OnTranscriptionReceived.RemoveListener(OnSpeechReceived);

    /// <summary>
    /// Escucha el STT y evalúa si dispara el resumen inmediatamente por longitud
    /// </summary>
    public void OnSpeechReceived(string speech)
    {
        if (string.IsNullOrWhiteSpace(speech)) return;

        // if we have NO selected students
        if (StudentManager.Exists && !StudentManager.Instance.HasSelectedStudents())
        {
            lock (_interactionBuffer)
            {
                string interaction = $"- Profesor: \"{speech}\"";
                _interactionBuffer.Add(interaction);
                FileHelper.AppendLog(Application.persistentDataPath + $"/context/{logPath}/interactions.txt", interaction, true);
            }
        }

        // Si la frase es especialmente larga, forzamos el resumen de inmediato
        if (ShouldRegister) TriggerSummary();
    }
}
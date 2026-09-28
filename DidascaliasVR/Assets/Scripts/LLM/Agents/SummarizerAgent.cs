using Didascalia.LLM;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SummarizerAgent<T> : Didascalia.LLM.LLMAgent<T> where T : MonoBehaviour
{
    public override object ResponseSchema => LLMSchemas.GenericAnswer; // we set it to null always
    public virtual string logPath => null; 

    protected List<string> _interactionBuffer = new List<string>();
    protected bool _isProcessing = false;

    [SerializeField,
        Tooltip("How many interactions we need to log before asking for a summary")]
    protected int _howManyInteractionsToRegister = 10;

    protected int _currentLogIndex = 0;
    StringBuilder _parsedLog = new StringBuilder();

    protected LLMHelpers.LLMGenericAnswerResult _summary = new LLMHelpers.LLMGenericAnswerResult();
    public string Summary => _summary.Answer;
    public string GetWholeContext()
    {
        StringBuilder nonProcessedInteractions = new StringBuilder();

        for (int i = _currentLogIndex; i < _interactionBuffer.Count; i++)
        {
            nonProcessedInteractions.AppendLine(_interactionBuffer[i]);
        }

        return $"Resumen: {Summary}\nInteracciones sin procesar: {nonProcessedInteractions}";
    }

    int _count = 0;

    protected bool ShouldRegister => _interactionBuffer.Count - _currentLogIndex >= _howManyInteractionsToRegister;

    protected async void TriggerSummary()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        try
        {
            string newContext = string.Empty;
            do
            {
                newContext = await ExecuteSummary();
                if (newContext != null && newContext.Trim() != string.Empty)
                {
                    _summary.Answer = newContext;
                    Debug.Log($"[Peer Context Summary] New Context: {Summary}");
                    FileHelper.SaveToFile(Application.persistentDataPath + $"/context/{logPath}/{_count++}.txt", Summary);
                }
            }
            while (newContext.Trim() == string.Empty);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ClassroomContextSummarizer] Error al resumir contexto: {ex.Message}");
        }
        finally
        {
            _isProcessing = false;
        }
    }

    /// <summary>
    /// Returns raw log and clears speechBuffer
    /// </summary>
    /// <returns></returns>
    public string GetLog()
    {
        lock (_interactionBuffer)
        {
            if (_interactionBuffer.Count == 0) return null;

            for (int i = _currentLogIndex; i < _interactionBuffer.Count; ++i)
            {
                _parsedLog.AppendLine(_interactionBuffer[i]);
            }

            _currentLogIndex = _interactionBuffer.Count;
        }

        return _parsedLog.ToString();
    }

    public async Task<string> ExecuteSummary()
    {
        string prompt = ReplacePlaceholder(_promptTemplate, "{RAW_CLASSROOM_LOG}", GetLog());

        return await ExecuteAsync(prompt);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Didascalia.LLM
{
    /// <summary>
    /// Base abstract class for LLM specialized agents using CRTP for Singleton instance management.
    /// </summary>
    public abstract class LLMAgent<T> : Singleton<T> where T : MonoBehaviour
    {
        static int _count = 0;

        [Header("Agent Configuration")]
        [SerializeField] protected string _promptFileName;

        [SerializeField, Range(0.0f, 1.0f)] protected float _temperature = 0.5f;

        protected string _promptTemplate = string.Empty;

        /// <summary>
        /// JSON Schema definition for structured response validation (optional).
        /// </summary>
        public virtual object ResponseSchema => null;

        protected static string PromptsPath => Application.streamingAssetsPath + "/Context/Prompts/";

        protected override void Awake()
        {
            base.Awake();
            LoadPromptTemplate();
        }

        /// <summary>
        /// Loads the prompt template file from StreamingAssets.
        /// </summary>
        protected virtual void LoadPromptTemplate()
        {
            if (string.IsNullOrEmpty(_promptFileName))
            {
                Debug.LogWarning($"[{GetType().Name}] Prompt file name is not assigned in the Inspector.");
                return;
            }

            string path = PromptsPath + _promptFileName;
            _promptTemplate = FileHelper.GetTextFromFile(path);

            if (string.IsNullOrEmpty(_promptTemplate))
            {
                Debug.LogError($"[{GetType().Name}] Failed to load prompt template from path: {path}");
            }
        }

        /// <summary>
        /// Executes an asynchronous text request to the LLM.
        /// </summary>
        public virtual async Task<string> ExecuteAsync(string promptPayload)
        {
            string formattedPrompt = promptPayload;
            string rawResponse = await LLMNetworkManager.Instance.QueryLLMAsync(formattedPrompt, ResponseSchema);
            return rawResponse != null ? rawResponse.Trim().Replace("\"", "") : string.Empty;
        }

        /// <summary>
        /// Executes an asynchronous request to the LLM and deserializes the JSON result into a DTO.
        /// </summary>
        public virtual async Task<TResult> ExecuteJsonAsync<TResult>(string promptPayload) where TResult : class
        {
            string formattedPrompt = promptPayload;
            string rawResponse = await LLMNetworkManager.Instance.QueryLLMAsync(formattedPrompt, ResponseSchema, _temperature);

            if (string.IsNullOrEmpty(rawResponse))
            {
                Debug.LogWarning($"[{GetType().Name}] Received empty response from LLM.");
                return null;
            }

            try
            {
                FileHelper.SaveToFile(Application.persistentDataPath + "/prompts/" + _count++ + ".txt", rawResponse);
                return JsonConvert.DeserializeObject<TResult>(rawResponse);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] Error deserializing response to {typeof(TResult).Name}: {ex.Message}\nRaw: {rawResponse}");
                return null;
            }
        }

        #region Helper Methods
        protected string ReplacePlaceholder(string template, string key, string value)
        {
            return template.Replace($"{key}", value ?? string.Empty);
        }
        #endregion
    }

    public static class LLMHelpers
    {
        public static string BuildConversationHistory(List<string> conversation)
        {
            if (conversation == null) return "No hay conversación previa";

            StringBuilder interactionSb = new StringBuilder();
            foreach (string interaction in conversation)
            {
                interactionSb.AppendLine("  - " + interaction);
            }
            return interactionSb.ToString();
        }

        public class LLMGenericAnswerResult
        {
            public string Answer;
        }

        private static Dictionary<StudentType, string> _stContextByType = null;

        public static Dictionary<StudentType, string> StudentContextByType
        {
            get
            {
                if (_stContextByType == null) PopulateContextDictionary();
                return _stContextByType;
            }
        }

        private static string _contextPath = Application.streamingAssetsPath + "/Context/";

        private static void PopulateContextDictionary()
        {
            _stContextByType = new Dictionary<StudentType, string>();

            for (int i = 0; i < (int)StudentType.Autistic + 1; ++i)
            {
                string text = FileHelper.GetTextFromFile(_contextPath + ((StudentType)i).ToString() + ".txt");
                _stContextByType.Add((StudentType)i, text);
            }
        }

        private static string _studentPlaceholderProfile = null;
        private static string _studentContextPath = _contextPath + "Prompts/StudentProfile.txt";
        public static string GetPlaceholderStudentProfile()
        {
            if (_studentPlaceholderProfile == null)
                _studentPlaceholderProfile = FileHelper.GetTextFromFile(_studentContextPath);

            return _studentPlaceholderProfile;
        }
    }
}
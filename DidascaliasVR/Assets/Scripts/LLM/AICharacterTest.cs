using TMPro;
using UnityEngine;

public class AICharacterTest : MonoBehaviour
{
    LLMUnity.LLMAgent _agent = null;

    [SerializeField] TextMeshProUGUI _agentText = null;

    public void Start()
    {
        _agent = GetComponent<LLMUnity.LLMAgent>();
    }

    public void PromptLLM(string text)
    {
        _agent.Chat(text, ShowLLMAnswer);
    }

    private void ShowLLMAnswer(string answer)
    {
        _agentText.SetText(answer);
    }

}

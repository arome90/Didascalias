using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitForLoadingGame : MonoBehaviour
{
    [SerializeField]
    [TextArea]
    private string format = "{0}";
    [SerializeField]
    TextMeshProUGUI _text = null;

    [SerializeField]
    Button _button = null;

    private void OnEnable()
    {
        StartCoroutine(LookForSession());
    }

    IEnumerator LookForSession()
    {
        _button.interactable = false;
        // waiting for session
        yield return new WaitUntil(() => WebDashboardManager.Instance.IsSessionAvaliable());
        Debug.Log("[Loading Menu] Session set up... Waiting for Speech set up.");
        if (SpeechManager.Instance != null)
            yield return new WaitUntil(() => SpeechManager.Instance.IsReadyForTranscription);
        
        Debug.Log("[Loading Menu] Speech set up. Game is ready.");

        AnimateCharacters anim = _text.GetComponent<AnimateCharacters>();
        if (anim != null) anim.StopAnimation();
        _text.text = string.Format(format, WebDashboardManager.Instance.SessionID);
        _button.interactable = true;
    }
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIDisplay : MonoBehaviour
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
        while (!WebDashboardManager.Instance.IsSessionAvaliable()) yield return null;

        AnimateCharacters anim = _text.GetComponent<AnimateCharacters>();
        if (anim != null) anim.StopAnimation();
        _text.text = string.Format(format, WebDashboardManager.Instance.SessionID);
        _button.interactable = true;
    }
}
